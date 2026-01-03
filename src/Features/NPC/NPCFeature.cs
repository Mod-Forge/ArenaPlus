using ArenaPlus.Lib;
using ArenaPlus.Utils;
using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ZeldackLib.DebugDraw;

namespace ArenaPlus.Features.NPC
{
    public class ArenaNPCPlayer : Player, Weapon.INotifyOfFlyingWeapons
    {
        public int NPCLevel => NPCLevelButtons.NPCLevels[playerState.playerNumber] + 1;

        public float JumpPower => 0.5f;
        public List<JumpFinder> jumpFinders = new List<JumpFinder>();
        public PathFinder.PathingCell jumpCell;

        public JumpFinder actOnJump;

        private int collisionDodgeCooldown;
        private int voidTeleportCooldown;
        public int dodgeCooldown;
        private Vector2 lastGroundPos;
        public ArenaNPCPlayer(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
        {
            if (this.isNPC)
            {
                LogInfo($"$lv {NPCLevel} npc created");
            }
            lastGroundPos = mainBodyChunk.pos;
            abstractCreature.personality.aggression = 1f;
            abstractCreature.personality.bravery = 1f;
        }

        private void LogNPC(params object[] data)
        {
            if (!NPCFeature.ncpLogs)
                return;
            LogUnity([$"[NPC: {this?.playerState.playerNumber}]", .. data]);
        }
        private void Replace()
        {
            WorldCoordinate worldPos = this.abstractCreature.pos;

            IntVector2 tilePos = worldPos.Tile;
            ShortcutData shortcut = room.shortcutData(tilePos);
            if (shortcut.shortCutType != ShortcutData.Type.DeadEnd)
            {
                worldPos.Tile = tilePos + room.ShorcutEntranceHoleDirection(tilePos) * 2;
            }

            AbstractCreature abstractCreature = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC), null, worldPos, room.game.GetNewID());
            abstractCreature.state = new PlayerNPCState(abstractCreature, playerState.playerNumber);
            ArenaNPCPlayer creature = new ArenaNPCPlayer(abstractCreature, room.world);
            creature.npcCharacterStats = new SlugcatStats(MoreSlugcatsEnums.SlugcatStatsName.Artificer, false);
            this.initPlayer(creature);
            creature.abstractCreature.abstractAI = new SlugNPCAbstractAI(room.world, creature.abstractCreature);
            creature.abstractCreature.abstractAI.RealAI = new SlugNPCAI(creature.abstractCreature, room.world);
            (creature.abstractCreature.abstractAI.RealAI as SlugNPCAI).itemTracker.stopTrackingCarried = false;
            //(creature.abstractCreature.abstractAI.RealAI as SlugNPCAI).itemTracker.visualize = true;
            room.abstractRoom.AddEntity(abstractCreature);
            abstractCreature.RealizeInRoom();


            AbstractSpear abstStick = new AbstractSpear(room.world, null, worldPos, room.game.GetNewID(), false, false);
            room.abstractRoom.AddEntity(abstStick);
            abstStick.RealizeInRoom();
            creature.SlugcatGrab(abstStick.realizedObject as Spear, 0);

            creature.SuperHardSetPosition(room.MiddleOfTile(worldPos) + Custom.RNV() * 10f);
            //ExitGameOver(creature);

            int index = this.room.game.GetArenaGameSession.Players.FindIndex(p => (p.realizedCreature as Player).playerState.playerNumber == this.playerState.playerNumber);
            this.room.game.GetArenaGameSession.Players[index] = abstractCreature;

            this.Destroy();

            if (GameUtils.rainWorldGame.cameras[0]?.hud == null)
                return;

            foreach (var part in GameUtils.rainWorldGame.cameras[0].hud.parts)
            {
                if (part is not HUD.PlayerSpecificMultiplayerHud playerHud)
                    continue;

                if (playerHud.abstractPlayer == base.abstractCreature)
                {
                    playerHud.abstractPlayer = abstractCreature;
                }
            }
        }

        private void initPlayer(Player pl)
        {
            pl.SlugCatClass = MoreSlugcatsEnums.SlugcatStatsName.Artificer;
            pl.slugcatStats.name = MoreSlugcatsEnums.SlugcatStatsName.Artificer;

            if (GameUtils.IsCompetitiveOrSandboxSession)
                pl.playerState.slugcatCharacter = SlugcatStats.Name.ArenaColor(pl.playerState.playerNumber);
            else
                pl.playerState.slugcatCharacter = MoreSlugcatsEnums.SlugcatStatsName.Artificer;

            pl.setPupStatus(false);
            pl.playerState.forceFullGrown = true;
            IntVector2 foodNeeded = SlugcatStats.SlugcatFoodMeter(MoreSlugcatsEnums.SlugcatStatsName.Artificer);
            pl.slugcatStats.maxFood = foodNeeded.x;
            pl.slugcatStats.foodToHibernate = foodNeeded.y;
            pl.playerState.foodInStomach = pl.slugcatStats.maxFood;
        }


        public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
        {
            base.Collide(otherObject, myChunk, otherChunk);
            if (NPCLevel >= 5 && otherObject is Creature creature && creature.abstractCreature.creatureTemplate.type != CreatureTemplate.Type.Fly)
            {
                if (collisionDodgeCooldown <= 0 && !creature.dead && creature.Consious)
                {
                    OldDodge(null, otherObject.bodyChunks[otherChunk].pos, Custom.DirVec(otherObject.bodyChunks[otherChunk].pos, bodyChunks[myChunk].pos));
                    collisionDodgeCooldown = 40;
                }
                else if (otherObject.bodyChunks[otherChunk].pos.y < mainBodyChunk.pos.y && canJump > 0)
                {
                    wantToJump = 2;
                }
            }
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            if (dead || slatedForDeletetion || room == null)
                return;

            if (!this.isNPC)
            {
                Replace();
                return;
            }

            if (NPCLevel <= 1)
                return;

            if (dodgeCooldown > 0)
                dodgeCooldown--;

            if (bodyChunks[1].contactPoint.y == -1)
            {
                lastGroundPos = mainBodyChunk.pos;
            }

            if (voidTeleportCooldown <= 0 && mainBodyChunk.pos.y < room.RoomRect.bottom && Consious)
            {
                DodgeVoid();
            }
            if (voidTeleportCooldown > 0) voidTeleportCooldown--;

            if (NPCLevel <= 2)
                return;

            if (AI == null)
                return;

            AIUpdate(eu);

            //AI.pathingAssist ??= new(abstractCreature);
            AI.toldToPlay = 0;
            AI.abstractAI.toldToStay = null;
            JumpLogicUpdate();

        }

        public void AIUpdate(bool eu)
        {


            ThrowToGetFreeUpdate(eu);

            if (Consious && Random.value < 0.25f)
            {
                PlayerMurderIntent();
            }

            JumpLogicUpdate();
            if (collisionDodgeCooldown > 0) collisionDodgeCooldown--;
        }

        public void ThrowToGetFreeUpdate(bool eu)
        {
            if (dangerGrasp == null || dangerGrasp.discontinued || dangerGraspTime >= 30)
                return;

            if (base.dead || base.stun >= 30)
                return;

            if (dangerGraspTime > 20)
            {
                this.ThrowToGetFree(eu);
            }
            this.DangerGraspPickup(eu);

            if (FeaturesManager.GetFeature("NormalNPC").configurable.Value != true && !grasps.Any(g => g != null && g.grabbed is Weapon))
            {
                AllGraspsLetGoOfThisObject(true);
                stun = 0;
                dangerGraspTime = 0;
                pyroJumpped = false;
                pyroJumpCooldown = 0;
                bodyMode = BodyModeIndex.Default;
                PyroParry();
            }
        }

        public void PlayerMurderIntent()
        {
            if (NPCLevel < 6)
                return;

            foreach (var absPlayer in room.game.Players)
            {
                if (absPlayer.realizedCreature is Player player && player != this && !player.dead && (absPlayer.realizedCreature is not ArenaNPCPlayer npc || NPCFeature.NPCAttackNpc))
                {
                    if (this.AI.HasLethal(player))
                    {
                        var target = new Tracker.SimpleCreatureRepresentation(AI.tracker, absPlayer, 1f, false);
                        this.AI.FindAttackPosition(target);
                        int chunk = Random.Range(0, target.representedCreature.realizedCreature.bodyChunks.Length - 1);
                        if (this.AI.GoodAttackPos(target, chunk))
                        {
                            BodyChunk bodyChunk = target.representedCreature.realizedCreature.bodyChunks[chunk];
                            AI.throwAtTarget = (int)Mathf.Sign(bodyChunk.pos.x - firstChunk.pos.x);
                        }
                    }
                }
            }
        }

        public void DodgeVoid()
        {
            LogNPC("[Dodge] Void");
            Array.ForEach(bodyChunks, c => c.vel *= -1f);
            SuperHardSetPosition(lastGroundPos);

            room.AddObject(new ShockWave(mainBodyChunk.pos, 125f, 0.35f, 15, true));
            room.PlaySound(SoundID.HUD_Pause_Game, mainBodyChunk.pos, 1f, UnityEngine.Random.Range(0.5f, 0.75f));
            voidTeleportCooldown = 100;

        }


        public bool CNPCGrabCheck(PhysicalObject item)
        {
            if (item.room == null || item.room != this.room || item.grabbedBy.Any(g => g != null && g.grabber == this))
            {
                return false;
            }
            if (item.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.SLOracleSwarmer)
            {
                return false;
            }
            if (item.abstractPhysicalObject.type == MoreSlugcatsEnums.AbstractObjectType.MoonCloak)
            {
                return false;
            }
            int index = 0;
            if (this.Grabability(item) == Player.ObjectGrabability.Drag)
            {
                float lowestDist = float.MaxValue;
                for (int i = 0; i < item.bodyChunks.Length; i++)
                {
                    if (Custom.DistLess(base.mainBodyChunk.pos, item.bodyChunks[i].pos, lowestDist))
                    {
                        lowestDist = Vector2.Distance(base.mainBodyChunk.pos, item.bodyChunks[i].pos);
                        index = i;
                    }
                }
            }

            Vector2 objPos = item.room.MiddleOfTile(item.abstractPhysicalObject.pos);
            var forbidenCheck = (!(item is PlayerCarryableItem) || (item as PlayerCarryableItem).forbiddenToPlayer < 1);
            var distCheck = Custom.DistLess(base.bodyChunks[0].pos, objPos, item.bodyChunks[index].rad + 40f) && (Custom.DistLess(base.bodyChunks[0].pos, objPos, item.bodyChunks[index].rad + 20f) || room.VisualContact(base.bodyChunks[0].pos, objPos));
            var canPickup = this.CanIPickThisUp(item);
            //LogNPC("trying to grab", item, "forbiden:", forbidenCheck, "distCheck", distCheck, "canPickup", canPickup);
            //LogNPC("distcheck", "dist1", Custom.Dist(base.bodyChunks[0].pos, objPos), "<", item.bodyChunks[index].rad + 40f, "dist2", Custom.Dist(base.bodyChunks[0].pos, objPos), "<", item.bodyChunks[index].rad + 20f, "|| visual", room.VisualContact(base.bodyChunks[0].pos, objPos));
            return forbidenCheck && distCheck && canPickup;
            //return (!(item is PlayerCarryableItem) || (item as PlayerCarryableItem).forbiddenToPlayer < 1) && Custom.DistLess(base.bodyChunks[0].pos, item.bodyChunks[index].pos, item.bodyChunks[index].rad + 40f) && (Custom.DistLess(base.bodyChunks[0].pos, item.bodyChunks[index].pos, item.bodyChunks[index].rad + 20f) || room.VisualContact(base.bodyChunks[0].pos, item.bodyChunks[index].pos)) && this.CanIPickThisUp(item);
        }

        #region dodge

        #region dodge moves

        public void Crouch()
        {
            base.bodyChunks[0].pos = this.room.MiddleOfTile(base.bodyChunks[0].pos) + new Vector2(0f, -20f);
            base.bodyChunks[0].vel = new Vector2(0f, -11f);
            base.bodyChunks[1].pos = Vector2.Lerp(base.bodyChunks[1].pos, base.bodyChunks[0].pos + new Vector2(0f, this.bodyChunkConnections[0].distance), 0.5f);
            base.bodyChunks[1].vel = new Vector2(0f, -11f);
            this.animation = Player.AnimationIndex.None;
            this.standing = false;
            base.GoThroughFloors = true;
            this.rollDirection = 0;
        }
        public void Drop()
        {
            base.bodyChunks[0].pos = this.room.MiddleOfTile(base.bodyChunks[0].pos) + new Vector2(0f, -20f);
            base.bodyChunks[0].vel = new Vector2(0f, -11f);
            base.bodyChunks[1].pos = Vector2.Lerp(base.bodyChunks[1].pos, base.bodyChunks[0].pos + new Vector2(0f, this.bodyChunkConnections[0].distance), 0.5f);
            base.bodyChunks[1].vel = new Vector2(0f, -11f);
            this.animation = Player.AnimationIndex.None;
            this.standing = false;
            base.GoThroughFloors = true;
            this.rollDirection = 0;
        }

        private bool PyroCheck()
        {
            if (FeaturesManager.GetFeature("NormalNPC").configurable.Value == true)
                return false;

            return !this.pyroJumpped && base.Consious && this.bodyMode != Player.BodyModeIndex.Crawl && this.bodyMode != Player.BodyModeIndex.CorridorClimb && this.bodyMode != Player.BodyModeIndex.ClimbIntoShortCut && this.animation != Player.AnimationIndex.HangFromBeam && this.animation != Player.AnimationIndex.ClimbOnBeam && this.bodyMode != Player.BodyModeIndex.WallClimb && this.bodyMode != Player.BodyModeIndex.Swimming && this.animation != Player.AnimationIndex.AntlerClimb && this.animation != Player.AnimationIndex.VineGrab && this.animation != Player.AnimationIndex.ZeroGPoleGrab && this.onBack == null;
        }
        public bool CanPyro()
        {
            int max = Mathf.Max(1, MoreSlugcats.MoreSlugcats.cfgArtificerExplosionCapacity.Value - 5);
            return Consious && PyroCheck() && this.pyroJumpCounter < max;
        }

        public void PyroJump(int dir = 0)
        {
            standing = true;
            wantToJump = 5;
            this.input[0].pckp = true;
            this.input[0].y = 1;
            if (dir != 0)
                this.input[0].x = (int)Mathf.Clamp01(dir);
            this.canJump = 0;
            ClassMechanicsArtificer();
        }

        public bool CanFlip()
        {
            bool zgCheck = this.bodyMode == Player.BodyModeIndex.ZeroG || this.room.gravity == 0f || base.gravity == 0f;
            bool flipCheck = !this.pyroJumpped && this.canJump <= 0 && !(this.eatMeat >= 20 || this.maulTimer >= 15) && (this.input[0].y >= 0 || (this.input[0].y < 0 && (this.bodyMode == Player.BodyModeIndex.ZeroG || base.gravity <= 0.1f))) && base.Consious && this.bodyMode != Player.BodyModeIndex.Crawl && this.bodyMode != Player.BodyModeIndex.CorridorClimb && this.bodyMode != Player.BodyModeIndex.ClimbIntoShortCut && this.animation != Player.AnimationIndex.HangFromBeam && this.animation != Player.AnimationIndex.ClimbOnBeam && this.bodyMode != Player.BodyModeIndex.WallClimb && this.bodyMode != Player.BodyModeIndex.Swimming && this.animation != Player.AnimationIndex.AntlerClimb && this.animation != Player.AnimationIndex.VineGrab && this.animation != Player.AnimationIndex.ZeroGPoleGrab && this.onBack == null;
            return flipCheck && !zgCheck && lowerBodyFramesOnGround > 2;
        }

        public void Flip()
        {
            if (this.input[0].x != 0)
            {
                base.bodyChunks[0].vel.y = Mathf.Min(base.bodyChunks[0].vel.y, 0f) + 8f;
                base.bodyChunks[1].vel.y = Mathf.Min(base.bodyChunks[1].vel.y, 0f) + 7f;
                this.jumpBoost = 6f;
            }
            if (this.input[0].x == 0 || this.input[0].y == 1)
            {
                if (this.pyroJumpCounter >= Mathf.Max(1, MoreSlugcats.MoreSlugcats.cfgArtificerExplosionCapacity.Value - 3))
                {
                    base.bodyChunks[0].vel.y = 16f;
                    base.bodyChunks[1].vel.y = 15f;
                    this.jumpBoost = 10f;
                }
                else
                {
                    base.bodyChunks[0].vel.y = 11f;
                    base.bodyChunks[1].vel.y = 10f;
                    this.jumpBoost = 8f;
                }
            }
            if (this.input[0].y == 1)
            {
                base.bodyChunks[0].vel.x = 10f * (float)this.input[0].x;
                base.bodyChunks[1].vel.x = 8f * (float)this.input[0].x;
            }
            else
            {
                base.bodyChunks[0].vel.x = 15f * (float)this.input[0].x;
                base.bodyChunks[1].vel.x = 13f * (float)this.input[0].x;
            }
            this.animation = Player.AnimationIndex.Flip;
            this.pyroJumpCounter++;
            this.pyroJumpCooldown = 150f;
            this.bodyMode = Player.BodyModeIndex.Default;
        }

        public void PyroParry()
        {
            wantToJump = 5;
            this.input[0].pckp = true;
            this.input[0].y = -1;
            this.canJump = 0;
            ClassMechanicsArtificer();
        }
        #endregion

        private bool OutOfDanger(Vector2 weaponLast, Vector2 weaponNext, Vector2[] tryPositions, float weaponRad, float gravity)
        {
            for (int i = 0; i < tryPositions.Length; i++)
            {
                if (this.BallisticCollision(tryPositions[i], weaponLast, weaponNext, base.bodyChunks[i].rad + weaponRad, gravity))
                {
                    return false;
                }
            }
            return true;
        }

        private bool BallisticCollision(Vector2 checkPos, Vector2 weaponLast, Vector2 weaponNext, float rad, float gravity)
        {
            if ((checkPos - weaponLast).sqrMagnitude <= (checkPos - weaponNext).sqrMagnitude)
                return false;

            float weaponY = this.CrossHeight(checkPos.x, weaponLast, weaponNext, gravity);
            return weaponY > checkPos.y - rad && weaponY < checkPos.y + rad;
        }

        private float CrossHeight(float xPos, Vector2 weaponLast, Vector2 weaponNext, float gravity)
        {
            if (Mathf.Abs(weaponLast.x - weaponNext.x) < 1f)
            {
                return -1000f;
            }
            float num = Mathf.Abs(xPos - weaponNext.x) / Mathf.Abs(weaponLast.x - weaponNext.x);
            return Custom.VerticalCrossPoint(weaponLast, weaponNext, xPos).y - gravity * num * num;
        }



        public void FlyingWeapon(Weapon weapon)
        {
            if (NPCLevel < 3 && Random.Range(0, 9) > NPCLevel)
            {
                dodgeCooldown = 5;
                return;
            }

            LogNPC("=========== FlyingWeapon =========");
            if (weapon.slatedForDeletetion || weapon.room != room)
                return;

            if (dodgeCooldown > 0 || !Consious)
                return;
            LogNPC("can dodge");

            if (AI != null && !AI.VisualContact(weapon.firstChunk.pos, 1f))
            {
                return;
            }
            LogNPC("see weapon");

            if (!Custom.DistLess(weapon.firstChunk.pos, mainBodyChunk.pos, 350f))
            {
                return;
            }

            LogNPC("weapon in range");

            bool hitChunk = false;
            bool[] hitChunks = new bool[bodyChunks.Length];
            for (int i = 0; i < bodyChunks.Length; i++)
            {
                hitChunks[i] = false;
                if (this.BallisticCollision(bodyChunks[i].pos, weapon.firstChunk.lastPos, weapon.firstChunk.pos, bodyChunks[i].rad + weapon.firstChunk.rad + 5f, (weapon is Spear) ? 0.45f : 0.9f))
                {
                    hitChunk = true;
                    hitChunks[i] = true;
                    //break;
                }
            }

            if (!hitChunk)
            {
                return;
            }
            LogNPC("weapon will hit");


            if (AI != null)
            {
                if (grasps.Any(g => g != null && g.grabbed is Weapon))
                {
                    AI.behaviorType = MoreSlugcats.SlugNPCAI.BehaviorType.Attacking;
                    PlayerMurderIntent();
                }

                for (int j = 0; j < this.AI.tracker.CreaturesCount; j++)
                {
                    if (this.AI.tracker.GetRep(j).representedCreature.realizedCreature is Creature creature && creature != this && creature.room == this.room && creature != weapon.thrownBy && Custom.DistLess(creature.mainBodyChunk.pos, Custom.ClosestPointOnLineSegment(base.mainBodyChunk.pos, weapon.firstChunk.pos, creature.mainBodyChunk.pos), 100f))
                    {
                        for (int k = 0; k < creature.bodyChunks.Length; k++)
                        {
                            if (Custom.DistLess(creature.bodyChunks[k].pos, Custom.ClosestPointOnLineSegment(base.mainBodyChunk.pos, weapon.firstChunk.pos, creature.bodyChunks[k].pos), creature.bodyChunks[k].rad + 10f))
                            {
                                LogNPC("other will take hit", creature);
                                return;
                            }
                        }
                    }
                }
                LogNPC("skip other take the hit");
            }

            int weaponDir = (int)Mathf.Sign(weapon.firstChunk.vel.x);
            float weaponDist = Custom.Dist(weapon.firstChunk.pos, mainBodyChunk.pos);

            if (NPCLevel >= 5 && standing && lowerBodyFramesOnGround >= 5 && hitChunks[0] && !hitChunks[1] && weaponDist > 60f)
            {
                Vector2[] dodgePos = [
                    bodyChunks[0].pos + Vector2.down * 5f,
                    bodyChunks[1].pos  + Vector2.down * 5f,
                ];

                var dist = Custom.Dist(bodyChunks[0].pos, bodyChunks[1].pos);
                dodgePos[0].y = dodgePos[1].y;
                dodgePos[0].x = dodgePos[1].x + dist * ThrowDirection;

                if (OutOfDanger(weapon.firstChunk.lastPos, weapon.firstChunk.pos, dodgePos, weapon.firstChunk.rad + 5f, (weapon is Spear) ? 0.45f : 0.9f))
                {
                    LogNPC("CROUCH!");
                    Crouch();
                    return;
                }
            }
            LogNPC("skip crouch");


            if (NPCLevel >= 4 && !room.GetTile(bodyChunks[1].pos + Vector2.down * 20f).Solid && canJump > 0)
            {
                Vector2[] dodgePos = [
                    bodyChunks[0].pos + Vector2.down * 20f,
                    bodyChunks[1].pos + Vector2.down * 20f
                ];

                if (OutOfDanger(weapon.firstChunk.lastPos, weapon.firstChunk.pos, dodgePos, weapon.firstChunk.rad + 5f, (weapon is Spear) ? 0.45f : 0.9f))
                {
                    LogNPC("DROP!");
                    mainBodyChunk.pos.y -= 10f;
                    Drop();
                }
            }
            LogNPC("skip drop");


            if (NPCLevel >=  4 && !room.GetTile(bodyChunks[0].pos + Vector2.up * 20f).Solid && weaponDist > 60f)
            {
                Vector2[] dodgePos = [
                    bodyChunks[0].pos + Vector2.up * 20f,
                    bodyChunks[1].pos + Vector2.up * 20f
                ];

                if (OutOfDanger(weapon.firstChunk.lastPos, weapon.firstChunk.pos, dodgePos, weapon.firstChunk.rad + 5f, (weapon is Spear) ? 0.45f : 0.9f))
                {
                    if (CanPyro()) // can pyrro jump
                    {
                        LogNPC("PYRRO JUMP!");
                        mainBodyChunk.pos.y += 10f;
                        PyroJump(-weaponDir);
                    }
                    else if (lowerBodyFramesOnGround >= 5 && CanFlip())
                    {
                        LogNPC("FLIP!");
                        mainBodyChunk.pos.y += 10f;
                        Flip();
                    }
                    else if (canJump > 0)
                    {
                        LogNPC("JUMP!");
                        canJump = 0;
                        wantToJump = 0;
                        mainBodyChunk.pos.y += 10f;
                        Jump();
                    }
                }
            }
            LogNPC("skip jumps");

            // check for throw parry
            if (NPCLevel >= 5 && weaponDist > 60f && weaponDist < 120f && grasps.Any(g => g != null && g.grabbed is Weapon) && BallisticCollision(bodyChunks[0].pos, weapon.firstChunk.lastPos, weapon.firstChunk.pos, weapon.firstChunk.rad * 2f, (weapon is Spear) ? 0.45f : 0.9f))
            {
                LogNPC("TAKE IT DOWN!");
                flipDirection = -weaponDir;
                mainBodyChunk.vel.x = Mathf.Abs(mainBodyChunk.vel.x) * flipDirection;
                wantToThrow = 5;
                dodgeCooldown = 5;
            }
            LogNPC("projectile block");


            if (CanPyro() && Custom.DistLess(weapon.firstChunk.pos, mainBodyChunk.pos, 60f))
            {
                LogNPC("PARRY!");
                PyroParry();
                return;
            }
            LogNPC("skip parry");


            if (NPCLevel >= 5)
            {
                LogNPC("use the old dodge");
                OldDodge(weapon, weapon.firstChunk.pos, weapon.firstChunk.vel.normalized);
            }
        }

        public void OldDodge(Weapon proj, Vector2 projPos, Vector2 dir)
        {
            LogNPC("[OldDodge] dodge");

            int pDir = -(int)Mathf.Sign(dir.x);
            pDir = pDir == 0 ? ThrowDirection : pDir;

            // if it hit high you should crouch
            //LogNPC("diff y", (projPos.y - proj.firstChunk.rad) - (bodyChunks[1].pos.y + bodyChunks[1].rad));
            bool hitHeigh = proj != null && (projPos.y - proj.firstChunk.rad) > (bodyChunks[1].pos.y + bodyChunks[1].rad);
            if (lowerBodyFramesOnGround > 0 && hitHeigh)
            {
                LogNPC("[OldDodge] crouch dodge");

                Crouch();
                return;
            }

            if (proj is Spear && Custom.DistLess(projPos, firstChunk.pos, 300f) && CanPyro() && Random.value < 0.20f)
            {
                LogNPC("[OldDodge] lucky parry");
                PyroParry();
                return;
            }

            // if you can pyro you jump hover
            if (CanPyro())
            {
                LogNPC("[OldDodge] pyro jump dodge");
                PyroJump(pDir);

                return;
            }
            else if (lowerBodyFramesOnGround >= 5 && CanFlip())
            {
                input[0].x = pDir;
                LogNPC("[OldDodge] flip dodge");
                Flip();
                return;
            }


            // if its all you can do throw a weapon to parry

            LogNPC("[OldDodge] weapon parry dodge");

            flipDirection = pDir;
            mainBodyChunk.vel.x = Mathf.Abs(mainBodyChunk.vel.x) * flipDirection;
            wantToThrow = 5;
        }
        #endregion

        private void JumpLogicUpdate()
        {
            if (base.dead)
            {
                return;
            }

            int max = Mathf.Max(1, MoreSlugcats.MoreSlugcats.cfgArtificerExplosionCapacity.Value - (15 - NPCLevel));
            bool canJump = base.Consious && PyroCheck() && pyroJumpCounter < max;
            for (int i = this.jumpFinders.Count - 1; i >= 0; i--)
            {
                if (this.jumpFinders[i].slatedForDeletion)
                {
                    this.jumpFinders.RemoveAt(i);
                }
                else
                {
                    this.jumpFinders[i].Update();
                }
            }

            if (canJump)
            {
                List<MovementConnection> upcoming = this.AI.GetUpcoming();
                if (upcoming != null)
                {
                    this.jumpCell = this.AI.pathFinder.PathingCellAtWorldCoordinate(base.abstractCreature.pos);

                    int jumpFinderCount = (int)Mathf.Round(
                            Mathf.Clamp(
                                Custom.MapRange(NPCLevel, 6, 9, 1, 4)
                            , 0, 4)
                        );
                    if (this.jumpFinders.Count < jumpFinderCount && upcoming.Count > 1)
                    {
                        WorldCoordinate dest = upcoming[global::UnityEngine.Random.Range(0, upcoming.Count)].destinationCoord;
                        if (dest.TileDefined && dest.Tile.FloatDist(abstractCreature.pos.Tile) > 2f && !this.room.aimap.getAItile(dest).narrowSpace && LizardJumpModule.PathWeightComparison(jumpCell, AI.pathFinder.PathingCellAtWorldCoordinate(dest)))
                        {
                            this.jumpFinders.Add(new JumpFinder(this.room, this, dest.Tile));
                        }
                        //RoomDebugDraw.SetupWithColor(Color.red);
                        //RoomDebugDraw.DrawRect(room.MiddleOfTile(dest), Vector2.one * 18f);
                        LogNPC("creating a new jump finder");
                    }
                }


                for (int i = this.jumpFinders.Count - 1; i >= 0; i--)
                {
                    if (!this.jumpFinders[i].slatedForDeletion && base.abstractCreature.pos.Tile == this.jumpFinders[i].startPos && this.jumpFinders[i].BeneficialMovement)
                    {
                        this.InitiateJump(this.jumpFinders[i]);
                        LogNPC("InitiateJump");
                        break;
                    }
                }

                if (actOnJump != null && Consious && Custom.DistLess(bodyChunks[0].pos, room.MiddleOfTile(actOnJump.bestJump.startPos), 50f))
                {
                    bodyChunks[0].vel *= 0.5f;
                    bodyChunks[0].pos = Vector2.Lerp(bodyChunks[0].pos, room.MiddleOfTile(actOnJump.bestJump.startPos) - actOnJump.bestJump.initVel.normalized, 0.2f);
                    bodyChunks[1].vel -= actOnJump.bestJump.initVel.normalized * 2f;
                    ControlledPyroJump();
                }
            }
        }

        public void InitiateJump(JumpFinder jump)
        {
            if (jump == null || jump.bestJump == null || jump.bestJump.goalCell == null)
            {
                return;
            }
            this.actOnJump = jump;
            for (int i = 0; i < this.jumpFinders.Count; i++)
            {
                this.jumpFinders[i].Destroy();
            }
        }

        public void ControlledPyroJump()
        {
            if (this.actOnJump == null || this.actOnJump.bestJump == null)
            {
                return;
            }
            LogNPC("controlled pyro jump");
            PyroJump();
            foreach (var chunk in bodyChunks)
            {
                var newVel = this.actOnJump.bestJump.initVel.normalized * Mathf.Min(this.actOnJump.bestJump.initVel.magnitude, 19f);
                chunk.vel.x = newVel.x;
                if (newVel.y > 0)
                    chunk.vel.y = newVel.y;
            }
            actOnJump.Destroy();
            actOnJump = null;
        }
    }

    [ImmutableFeature]
    internal class NPCFeature : ImmutableFeature
    {
        public static SlugcatStats.Name NPCName;

        internal static bool NPCAttackNpc => NPCAttackPlayers.npcAttackNPC.Value;
        internal static bool NPCAttackPlayer => FeaturesManager.GetFeature("NPCAttackPlayers").configurable.Value;


        [MyCommand("npc_logging")]
        internal static bool ncpLogs { get; set; } = false;

        protected override void Register()
        {
            if (!ModManager.MSC)
                return;
            NPCName = new SlugcatStats.Name("ArenaNPC", true);

            On.AbstractCreature.Realize += AbstractCreature_Realize;
            On.SlugcatStats.getSlugcatName += SlugcatStats_getSlugcatName;
            On.JollyCoop.JollyMenu.JollySlidingMenu.NextClass += JollySlidingMenu_NextClass;

            On.MoreSlugcats.SlugNPCAI.DecideBehavior += SlugNPCAI_DecideBehavior;
            On.MoreSlugcats.SlugNPCAI.PassingGrab += SlugNPCAI_PassingGrab;
            On.SandboxGameSession.ShouldSessionEnd += SandboxGameSession_ShouldSessionEnd;
            On.CompetitiveGameSession.ShouldSessionEnd += CompetitiveGameSession_ShouldSessionEnd;
            On.Menu.ArenaOverlay.Update += ArenaOverlay_Update;
            On.MoreSlugcats.SlugNPCAI.IUseARelationshipTracker_UpdateDynamicRelationship += SlugNPCAI_IUseARelationshipTracker_UpdateDynamicRelationship;
            On.MoreSlugcats.SlugNPCAI.Move += SlugNPCAI_Move;
            On.ArtificialIntelligence.VisualContact_BodyChunk += ArtificialIntelligence_VisualContact_BodyChunk;
            On.MoreSlugcats.SlugNPCAI.GoodAttackPos += SlugNPCAI_GoodAttackPos;
            On.Player.Die += Player_Die;
            On.ArenaGameSession.EndOfSessionLogPlayerAsAlive += ArenaGameSession_EndOfSessionLogPlayerAsAlive;
            On.ArenaGameSession.Killing += ArenaGameSession_Killing;
            On.MoreSlugcats.SlugNPCAI.LethalWeaponScore += SlugNPCAI_LethalWeaponScore;
            On.MoreSlugcats.SlugNPCAI.NearestLethalWeapon += SlugNPCAI_NearestLethalWeapon;
            On.MoreSlugcats.SlugNPCAI.CanGrabItem += SlugNPCAI_CanGrabItem;
            On.Player.NPCForceGrab += Player_NPCForceGrab;
            On.Player.ThrowObject += Player_ThrowObject;

            // hide slugcat
            On.SlugcatStats.HiddenOrUnplayableSlugcat += SlugcatStats_HiddenOrUnplayableSlugcat;

        }

        private bool SlugcatStats_HiddenOrUnplayableSlugcat(On.SlugcatStats.orig_HiddenOrUnplayableSlugcat orig, SlugcatStats.Name i)
        {
            return orig(i) || i == NPCName;
        }

        private void Player_ThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
        {
            if (self is ArenaNPCPlayer npc && Random.Range(0, 9) > npc.NPCLevel)
            {
                self.wantToThrow = 0;
                return;
            }
            orig(self, grasp, eu);
        }

        private SlugcatStats.Name JollySlidingMenu_NextClass(On.JollyCoop.JollyMenu.JollySlidingMenu.orig_NextClass orig, JollyCoop.JollyMenu.JollySlidingMenu self, SlugcatStats.Name curClass, int playerIndex)
        {
            var nClass = orig(self, curClass, playerIndex);
            if (nClass == NPCName)
                nClass = orig(self, nClass, playerIndex);
            return nClass;
        }

        private string SlugcatStats_getSlugcatName(On.SlugcatStats.orig_getSlugcatName orig, SlugcatStats.Name i)
        {
            return i == NPCFeature.NPCName ? "CPU" : orig(i);
        }

        private void AbstractCreature_Realize(On.AbstractCreature.orig_Realize orig, AbstractCreature self)
        {
            orig(self);
            if (ModManager.MSC && self.realizedCreature is Player player && player.SlugCatClass == NPCName)
            {
                player.SlugCatClass = MoreSlugcatsEnums.SlugcatStatsName.Artificer;
                player.slugcatStats.name = MoreSlugcatsEnums.SlugcatStatsName.Artificer;
                self.realizedCreature = new ArenaNPCPlayer(self, self.world);
            }
        }

        private static void Player_NPCForceGrab(On.Player.orig_NPCForceGrab orig, Player self, PhysicalObject obj)
        {
            if (self is ArenaNPCPlayer npc)
            {
                obj.AllGraspsLetGoOfThisObject(true);
            }
            orig(self, obj);
        }

        private static bool SlugNPCAI_CanGrabItem(On.MoreSlugcats.SlugNPCAI.orig_CanGrabItem orig, SlugNPCAI self, PhysicalObject obj)
        {
            return orig(self, obj) || (self.cat is ArenaNPCPlayer npc && npc.NPCLevel >= 3 && npc.CNPCGrabCheck(obj));
        }

        private static PhysicalObject SlugNPCAI_NearestLethalWeapon(On.MoreSlugcats.SlugNPCAI.orig_NearestLethalWeapon orig, SlugNPCAI self, Creature target)
        {
            if (self.cat is not ArenaNPCPlayer npc)
                return orig(self, target);

            float bestScore = 0f;
            PhysicalObject physicalObject = null;
            for (int i = 0; i < self.itemTracker.ItemCount; i++)
            {
                ItemTracker.ItemRepresentation rep = self.itemTracker.GetRep(i);
                WorldCoordinate pos = rep.representedItem.pos;
                if (rep.representedItem.realizedObject != null && !self.HoldingThis(rep.representedItem.realizedObject) && CoordinateRemotlyReachable(self.pathFinder, rep.representedItem.pos))
                {
                    //npc.LogNPC(rep.representedItem.realizedObject, "in reach");
                    float magnitude = (rep.representedItem.realizedObject.firstChunk.pos - self.cat.firstChunk.pos).magnitude;
                    float score = self.LethalWeaponScore(rep.representedItem.realizedObject, target) * Mathf.Clamp(1f - magnitude / 2000f, 0f, 1f);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        physicalObject = rep.representedItem.realizedObject;
                    }
                }
            }
            return physicalObject;
        }

        private static bool CoordinateRemotlyReachable(PathFinder pathFinder, WorldCoordinate pos)
        {
            for (int x = -2; x <= 2; x++)
            {
                for (int y = -2; y <= 1; y++)
                {
                    if (pathFinder.CoordinateReachable(new WorldCoordinate(pos.room, pos.x + x, pos.y + y, pos.abstractNode)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static float SlugNPCAI_LethalWeaponScore(On.MoreSlugcats.SlugNPCAI.orig_LethalWeaponScore orig, SlugNPCAI self, PhysicalObject obj, Creature target)
        {
            if (self.cat is ArenaNPCPlayer && obj is Spear spear && spear.stuckInWall != null)
            {
                return 3f;
            }
            return orig(self, obj, target);
        }

        private static void ArenaGameSession_Killing(On.ArenaGameSession.orig_Killing orig, ArenaGameSession self, Player player, Creature killedCrit)
        {
            if (player is not ArenaNPCPlayer npc)
            {
                orig(self, player, killedCrit);
                return;
            }

            SlugNPCAI ai = npc.AI;
            player.abstractCreature.abstractAI.RealAI = null;
            orig(self, player, killedCrit);
            player.abstractCreature.abstractAI.RealAI = ai;
        }

        private static bool ArenaGameSession_EndOfSessionLogPlayerAsAlive(On.ArenaGameSession.orig_EndOfSessionLogPlayerAsAlive orig, ArenaGameSession self, int playerNumber)
        {
            bool lastOutsidePlayersCountAsDead = self.outsidePlayersCountAsDead;
            for (int j = 0; j < self.Players.Count; j++)
            {
                if (self.Players[j].realizedCreature is ArenaNPCPlayer npc)
                {
                    self.outsidePlayersCountAsDead = false;
                    break;
                }
            }
            var val = orig(self, playerNumber);
            self.outsidePlayersCountAsDead = lastOutsidePlayersCountAsDead;
            return val;
        }


        private static void Player_Die(On.Player.orig_Die orig, Player self)
        {
            bool wasDead = self.dead;
            orig(self);
        }

        private static bool SlugNPCAI_GoodAttackPos(On.MoreSlugcats.SlugNPCAI.orig_GoodAttackPos orig, SlugNPCAI self, Tracker.CreatureRepresentation target, int chunk)
        {
            realVisualContact = true;
            var val = orig(self, target, chunk);
            realVisualContact = false;
            return val;
        }

        private static bool realVisualContact;
        private static bool ArtificialIntelligence_VisualContact_BodyChunk(On.ArtificialIntelligence.orig_VisualContact_BodyChunk orig, ArtificialIntelligence self, BodyChunk chunk)
        {
            if (self is SlugNPCAI npcAI && npcAI.cat is ArenaNPCPlayer npc && !realVisualContact && (chunk.owner is Weapon || chunk.owner is Player))
            {
                return true;
            }
            return orig(self, chunk);
        }

        private static void SlugNPCAI_Move(On.MoreSlugcats.SlugNPCAI.orig_Move orig, SlugNPCAI self)
        {
            orig(self);
            if (self.cat is not ArenaNPCPlayer npc)
                return;

            if (npc.enteringShortCut.HasValue && npc.room.shortcutData(npc.enteringShortCut.Value).shortCutType == ShortcutData.Type.RoomExit)
            {
                if (npc.abstractCreature.pos.Tile == npc.enteringShortCut.Value)
                {
                    npc.mainBodyChunk.vel += Custom.IntVector2ToVector2(npc.room.ShorcutEntranceHoleDirection(npc.enteringShortCut.Value)) * 25f;
                }
                npc.enteringShortCut = null;
            }
        }

        private static CreatureTemplate.Relationship SlugNPCAI_IUseARelationshipTracker_UpdateDynamicRelationship(On.MoreSlugcats.SlugNPCAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, SlugNPCAI self, RelationshipTracker.DynamicRelationship dRelation)
        {
            if (self.cat is not ArenaNPCPlayer npc)
                return orig(self, dRelation);

            if (dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Fly || dRelation.trackerRep.representedCreature.realizedCreature == null || dRelation.trackerRep.representedCreature.realizedCreature.dead)
            {
                return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 1f);
            }

            if (dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
            {
                return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 1f);
            }

            if (dRelation.trackerRep.representedCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC && (!NPCAttackPlayer || (!NPCAttackNpc && dRelation.trackerRep.representedCreature.realizedCreature is ArenaNPCPlayer)))
            {
                return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 1f);
            }

            return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 0.6f);
        }

        private static void ArenaOverlay_Update(On.Menu.ArenaOverlay.orig_Update orig, Menu.ArenaOverlay self)
        {
            orig(self);
            for (int i = 0; i < self.result.Count; i++)
            {
                if (self.allResultBoxesInPlaceCounter > 10 && !self.result[i].readyForNextRound)
                {
                    if (self.result[i].playerClass == NPCName && !self.playersContinueButtons[i])
                    {
                        self.result[i].readyForNextRound = true;
                        self.PlayerPressedContinue();
                    }
                }
            }
        }

        private static bool SandboxGameSession_ShouldSessionEnd(On.SandboxGameSession.orig_ShouldSessionEnd orig, SandboxGameSession self)
        {
            if (ShouldSessionEnd(self)) return self.winLoseGameOver || (self.PlayMode && self.initiated && self.sandboxInitiated);
            return orig(self);
        }

        private static bool CompetitiveGameSession_ShouldSessionEnd(On.CompetitiveGameSession.orig_ShouldSessionEnd orig, CompetitiveGameSession self)
        {
            if (ShouldSessionEnd(self)) return self.initiated;
            return orig(self);
        }

        private static bool ShouldSessionEnd(ArenaGameSession session)
        {
            if (!GameUtils.IsCompetitiveOrSandboxSession)
                return false;

            int playerCount = 0;
            int NPCCount = 0;
            int aliveNPC = 0;
            int alivePlayer = 0;
            foreach (var abstPlayer in session.Players)
            {
                if (abstPlayer == null)
                    continue;

                playerCount++;

                if (abstPlayer.realizedCreature is Player player && !player.dead)
                {
                    alivePlayer++;
                }

                if (abstPlayer.realizedCreature is ArenaNPCPlayer npc)
                {
                    NPCCount++;
                    if (!npc.dead)
                    {
                        aliveNPC++;
                    }
                }
            }

            bool hasNpc = NPCCount > 0;
            if (hasNpc && playerCount > 1 && alivePlayer == 1 && aliveNPC == 1)
            {
                return true;
            }

            return false;
        }

        private static void SlugNPCAI_PassingGrab(On.MoreSlugcats.SlugNPCAI.orig_PassingGrab orig, SlugNPCAI self)
        {
            orig(self);
            if (self.cat is not ArenaNPCPlayer npc)
                return;

            if (!npc.grasps.Any(g => g != null && g.grabbed is Weapon))
            {
                PassingGrabWeapon(npc, self);
            }


        }

        private static void PassingGrabWeapon(ArenaNPCPlayer npc, SlugNPCAI ai)
        {
            if (ai.itemTracker.ItemCount > 0)
            {
                for (int l = 0; l < ai.itemTracker.ItemCount; l++)
                {
                    PhysicalObject realizedObject = ai.itemTracker.GetRep(l).representedItem.realizedObject;
                    if (ai.CanGrabItem(realizedObject) && realizedObject is Weapon)
                    {
                        npc.NPCForceGrab(realizedObject);
                        return;
                    }
                }
            }

            if (npc.pickUpCandidate is Weapon w && npc.CanIPickThisUp(w))
            {
                npc.NPCForceGrab(w);
                return;
            }

            //foreach (var abstractCreature in npc.abstractCreature.Room.creatures)
            //{
            //    if (abstractCreature == null || abstractCreature.realizedCreature is not Creature creature || creature.grasps.Length == 0)
            //        continue;

            //    if (creature.grasps.FirstOrDefault(g => g != null && g.grabbed is Weapon)?.grabbed is Weapon weapon && npc.CNPCGrabCheck(weapon))
            //    {
            //        weapon.AllGraspsLetGoOfThisObject(true);
            //        npc.NPCForceGrab(weapon);
            //    }
            //}
        }

        private static void SlugNPCAI_DecideBehavior(On.MoreSlugcats.SlugNPCAI.orig_DecideBehavior orig, SlugNPCAI self)
        {
            orig(self);
            if (self.cat is not ArenaNPCPlayer npc)
                return;

            bool hasWeapon = npc.grasps.Any(g => g != null && g.grabbed is Weapon);

            if ((self.preyTracker.MostAttractivePrey != null || self.threatTracker.mostThreateningCreature != null)
                && (self.behaviorType == SlugNPCAI.BehaviorType.Idle
                || self.behaviorType == SlugNPCAI.BehaviorType.Fleeing
                || self.behaviorType == SlugNPCAI.BehaviorType.Following)
                || self.behaviorType == SlugNPCAI.BehaviorType.GrabItem)
            {
                self.behaviorType = SlugNPCAI.BehaviorType.Attacking;
            }

            if (!hasWeapon && self.behaviorType == SlugNPCAI.BehaviorType.Attacking && self.threatTracker.mostThreateningCreature != null)
            {
                self.behaviorType = SlugNPCAI.BehaviorType.Fleeing;
            }

            //if (self.behaviorType == SlugNPCAI.BehaviorType.GrabItem && self.threatTracker.mostThreateningCreature?.representedCreature.realizedCreature is Creature c)
            //{
            //    LogInfo("HasLethal", self.HasLethal(c, true));
            //}

            //if (self.behaviorType == SlugNPCAI.BehaviorType.GrabItem && self.threatTracker.mostThreateningCreature?.representedCreature.realizedCreature is Creature creature && self.HasLethal(creature, true))
            //{
            //    self.behaviorType = SlugNPCAI.BehaviorType.Attacking;
            //}
        }
    }
}
