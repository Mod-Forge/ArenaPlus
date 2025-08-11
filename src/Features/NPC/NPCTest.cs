using ArenaPlus.Lib;
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
    public class NPCTestPlayer : Player, Weapon.INotifyOfFlyingWeapons
    {
        public NPCTestPlayer(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
        {
        }

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

        public bool CanPyro()
        {
            int max = Mathf.Max(1, MoreSlugcats.MoreSlugcats.cfgArtificerExplosionCapacity.Value - 5);
            return Consious && this.pyroJumpCounter < max;
        }

        public void PyroJump(int dir = 0)
        {
            standing = true;
            wantToJump = 1;
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

        public override void Update(bool eu)
        {
            base.Update(eu);
            RoomDebugDraw.SetupWithColor(Color.yellow);


            for (int i = 0; i < bodyChunks.Length; i++)
            {
                RoomDebugDraw.DrawText(bodyChunks[i].pos, $"{i}", 1f);
            }

            RoomDebugDraw.color = new Color(1f, 1f, 0f, 0.02f);
            RoomDebugDraw.DrawCircle(mainBodyChunk.pos, 350f);

        }

        public void FlyingWeapon(Weapon weapon)
        {
            RoomDebugDraw.SetupWithColor(Color.red);
            RoomDebugDraw.DrawArrow(weapon.firstChunk.lastPos, weapon.firstChunk.pos);

            RoomDebugDraw.color = new Color(1, 0f, 0f, 0.05f);
            bool hitChunk = false;

            bool[] hitChunks = new bool[bodyChunks.Length];
            if (Custom.DistLess(weapon.firstChunk.pos, mainBodyChunk.pos, 350f))
            {
                for (int i = 0; i < bodyChunks.Length; i++)
                {
                    hitChunks[i] = false;
                    if (this.BallisticCollision(bodyChunks[i].pos, weapon.firstChunk.lastPos, weapon.firstChunk.pos, bodyChunks[i].rad + weapon.firstChunk.rad, (weapon is Spear) ? 0.45f : 0.9f))
                    {
                        hitChunk = true;
                        hitChunks[i] = true;
                        RoomDebugDraw.DrawCircle(bodyChunks[i].pos, bodyChunks[i].rad);
                        //break;
                    }
                }
            }

            if (!hitChunk)
            {
                RoomDebugDraw.DrawText(mainBodyChunk.pos + Vector2.up * 20f, $"will hit chunk {hitChunk}");
                return;
            }

            RoomDebugDraw.color = Color.red;
            var text = $"will hit chunk {hitChunk}";
            text += "\naction to take: " + CheckForAction(weapon, hitChunks);
            RoomDebugDraw.DrawText(mainBodyChunk.pos + Vector2.up * 20f, text);
        }

        public string CheckForAction(Weapon weapon, bool[] hitChunks)
        {
            int weaponDir = (int)Mathf.Sign(weapon.firstChunk.vel.x);
            float weaponDist = Custom.Dist(weapon.firstChunk.pos, mainBodyChunk.pos);

            if (standing && hitChunks[0] && !hitChunks[1] && weaponDist > 60f)
            {
                Vector2[] dodgePos = [
                    bodyChunks[0].pos,
                    bodyChunks[1].pos
                ];

                var dist = Custom.Dist(bodyChunks[0].pos, bodyChunks[1].pos);
                dodgePos[0].y = bodyChunks[1].pos.y;
                dodgePos[0].x = bodyChunks[1].pos.x + dist * ThrowDirection;

                if (OutOfDanger(weapon.firstChunk.lastPos, weapon.firstChunk.pos, dodgePos, weapon.firstChunk.rad + 5f, (weapon is Spear) ? 0.45f : 0.9f))
                {
                    Crouch();
                    return "crouch";
                }
            }

            if (!room.GetTile(bodyChunks[1].pos + Vector2.down * 20f).Solid)
            {
                Vector2[] dodgePos = [
                    bodyChunks[0].pos + Vector2.down * 20f,
                    bodyChunks[1].pos + Vector2.down * 20f
                ];

                if (OutOfDanger(weapon.firstChunk.lastPos, weapon.firstChunk.pos, dodgePos, weapon.firstChunk.rad, (weapon is Spear) ? 0.45f : 0.9f))
                {
                    Drop();
                    return "drop";
                }
            }

            if (!room.GetTile(bodyChunks[0].pos + Vector2.up * 20f).Solid && weaponDist > 60f)
            {
                Vector2[] dodgePos = [
                    bodyChunks[0].pos + Vector2.up * 20f,
                    bodyChunks[1].pos + Vector2.up * 20f
                ];

                if (OutOfDanger(weapon.firstChunk.lastPos, weapon.firstChunk.pos, dodgePos, weapon.firstChunk.rad, (weapon is Spear) ? 0.45f : 0.9f))
                {
                    if (CanPyro()) // can pyrro jump
                    {
                        mainBodyChunk.pos.y += 10f;
                        PyroJump(-weaponDir);
                        return "pyro jump";
                    }
                    else if (lowerBodyFramesOnGround >= 5 && CanFlip())
                    {
                        Flip();
                        return "flip";
                    }
                    else if (canJump > 0)
                    {
                        canJump = 0;
                        wantToJump = 0;
                        mainBodyChunk.pos.y += 10f;
                        Jump();
                        return "jump";
                    }
                }
            }

            // check for throw parry
            if (weaponDist > 60f && grasps.Any(g => g != null && g.grabbed is Weapon) && BallisticCollision(bodyChunks[0].pos, weapon.firstChunk.lastPos, weapon.firstChunk.pos, weapon.firstChunk.rad * 2f, (weapon is Spear) ? 0.45f : 0.9f))
            {
                flipDirection = -weaponDir;
                mainBodyChunk.vel.x = Mathf.Abs(mainBodyChunk.vel.x) * flipDirection;
                wantToThrow = 5;
                return "projectile block";
            }

            if (CanPyro() && Custom.DistLess(weapon.firstChunk.pos, mainBodyChunk.pos, 60f))
            {
                PyroParry();
                return "parry";
            }


            return "None";
        }

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
    }

    [ImmutableFeature]
    internal class NPCTest : ImmutableFeature
    {
        protected override void Register()
        {
            //On.AbstractCreature.Realize += AbstractCreature_Realize;
        }

        private void AbstractCreature_Realize(On.AbstractCreature.orig_Realize orig, AbstractCreature self)
        {
            orig(self);
            if (ModManager.MSC && self.realizedCreature is Player player && player.SlugCatClass == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Artificer)
            {
                self.realizedCreature = new NPCTestPlayer(self, self.world);
            }
        }


    }
}
