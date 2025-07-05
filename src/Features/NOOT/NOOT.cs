using ArenaPlus.Lib;
using ArenaPlus.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using SlugBase.DataTypes;
using Watcher;

namespace ArenaPlus.Features.NOOT
{

    [FeatureInfo(
        id: nameof(NOOT),
        name: "N O O T",
        description: "NOOT NOOT",
        enabledByDefault: false,
        category: BuiltInCategory.Reworks
    )]


    file class NOOT(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
    {
        private bool skipMovement;
        protected override void Unregister()
        {
            JavlinNoot.Unregister();
            On.SmallNeedleWorm.BitByPlayer -= SmallNeedleWorm_BitByPlayer;
            On.NeedleWormGraphics.DrawSprites -= NeedleWormGraphics_DrawSprites;
            On.Player.checkInput -= Player_checkInput;
            On.Player.ThrowObject -= Player_ThrowObject;
            On.PlayerGraphics.Update -= PlayerGraphics_Update;
        }

        protected override void Register()
        {
            JavlinNoot.Register();
            On.SmallNeedleWorm.BitByPlayer += SmallNeedleWorm_BitByPlayer;
            On.NeedleWormGraphics.DrawSprites += NeedleWormGraphics_DrawSprites;
            On.Player.checkInput += Player_checkInput;
            On.Player.ThrowObject += Player_ThrowObject;
            On.PlayerGraphics.Update += PlayerGraphics_Update;

        }

        private void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            if (self.player.GetAttachedFeatureType<MusicNoot>() is MusicNoot musicNoot)
            {
                musicNoot.playerColor = PlayerGraphics.SlugcatColor(self.CharacterForColor);
            }
            orig(self);
        }

        private void Player_ThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
        {
            if (self.grasps[grasp]?.grabbed is SmallNeedleWorm smallNoot && self.GetAttachedFeatureType<MusicNoot>() is MusicNoot musicNoot && musicNoot.IsPlaying)
            {
                // stop playing
                self.eatCounter = 30;
                return;
            }
            orig(self, grasp, eu);
        }

        private void Player_checkInput(On.Player.orig_checkInput orig, Player self)
        {
            orig(self);
            if (self.GetAttachedFeatureType<MusicNoot>() is MusicNoot musicNoot && musicNoot.lockInput)
            {
                self.input[0].x = 0;
                self.input[0].y = 0;
                self.input[0].pckp = false;
                musicNoot.lockInput = false;
            }
        }

        private void NeedleWormGraphics_DrawSprites(On.NeedleWormGraphics.orig_DrawSprites orig, NeedleWormGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (self.worm.grabbedBy.Count > 0 && self.worm.grabbedBy[0].grabber is Player player && player.GetAttachedFeatureType<MusicNoot>() is MusicNoot musicNoot)
            {
                musicNoot.nootColor = self.bodyColor;
            }
            orig(self, sLeaser, rCam, timeStacker, camPos);
        }

        private void SmallNeedleWorm_BitByPlayer(On.SmallNeedleWorm.orig_BitByPlayer orig, SmallNeedleWorm self, Creature.Grasp grasp, bool eu)
        {
            if (!GameUtils.IsCompetitiveOrSandboxSession)
                orig(self, grasp, eu);

            Player player = grasp.grabber as Player;
            if (player.GetAttachedFeatureType<MusicNoot>() is not MusicNoot musicNoot)
            {
                musicNoot = new MusicNoot();
                player.AddAttachedFeature(musicNoot);
            }

            player.checkInput();
            musicNoot.lastPlayingInput = musicNoot.playingInput;
            musicNoot.playingInput = player.input[0];
            Vector2 vecInput = player.input[0].IntVec.ToVector2();

            var note = -1;
            var usingAnalogue = player.input[0].analogueDir.magnitude > 0f;
            if (player.input[0].analogueDir.magnitude > 0.6f || (usingAnalogue && vecInput.magnitude > 0f))
            {
                var angle = Custom.VecToDeg(usingAnalogue ? player.input[0].analogueDir.normalized : vecInput.normalized) - 90;

                // offset the angle for the middle to in the right pos
                if (usingAnalogue)
                    angle += 45f / 2f;

                while (angle < 0)
                    angle += 360;

                note = (int)(angle / 45);
            }

            // play note
            if (musicNoot.lastNote != note || (!musicNoot.lastPlayingInput.pckp && musicNoot.playingInput.pckp))
            {
                musicNoot.lastNote = note;



                if (musicNoot.playingInput.pckp && note > -1)
                {
                    musicNoot.ScreemMusic(self, note);
                }
            }


                //player.eatCounter = player.input[0].pckp ? 0 : 30;
            player.eatCounter = 0;

            musicNoot.lockInput = true;
            player.input[0].pckp = false;
            musicNoot._playing = true;

        }

        public class MusicNoot : PlayerCosmeticFeature
        {
            public Color playerColor = Color.white;
            public Color nootColor = Color.magenta;

            public int lastNote = -1;
            public bool lockInput;
            public bool _playing;
            private bool wasPlaying;
            public bool IsPlaying => wasPlaying || _playing;
            public Player.InputPackage lastPlayingInput;
            public Player.InputPackage playingInput = new Player.InputPackage() { pckp = true };
            public Vector2 MusicInputVec => playingInput.analogueDir.magnitude > 0f ? playingInput.analogueDir.normalized : playingInput.IntVec.ToVector2().normalized;

            public override void Update(bool eu)
            {
                wasPlaying = _playing;
                base.Update(eu);
                _playing = false;

                if (!IsPlaying)
                {
                    Destroy();
                }
            }

            internal void ScreemMusic(SmallNeedleWorm smallNoot, int note)
            {
                //LogInfo("playing note", note);

                smallNoot.screaming = 1f;
                // 0.475
                if (playingInput.mp)
                {
                    smallNoot.room.PlaySound(SoundID.Small_Needle_Worm_Intense_Trumpet_Scream, smallNoot.mainBodyChunk.pos, 1f, Mathf.Lerp(0.475f, 0.95f, (float)note / 7f) * 0.714285f);
                }
                else
                {
                    smallNoot.room.PlaySound(SoundID.Small_Needle_Worm_Intense_Trumpet_Scream, smallNoot.mainBodyChunk.pos, 1f, Mathf.Lerp(0.95f, 1.9f, (float)note / 7f) * 0.714285f);
                }
                smallNoot.hasScreamed = true;
                if (smallNoot.Mother != null)
                {
                    smallNoot.Mother.BigAI.BigRespondCry();
                }

                smallNoot.room.AddObject(new MusicNote(player, smallNoot.mainBodyChunk.pos, Custom.DegToVec(note * 45f + 90f) * 5f, nootColor * 1.5f));

                PlayMusic(note);
            }

            #region sercet music
            List<Music> musics = [
                new Music( // den warp
                    [0, 0, 4, 4, 2, 2, 6, 6],
                    mn => {
                        mn.player.SuperHardSetPosition(mn.player.room.MiddleOfTile(mn.player.room.LocalCoordinateOfNode(Random.Range(0, mn.player.room.abstractRoom.nodes.Length)).Tile));
                        mn.player.room.PlaySound(Sounds.Noot_Warp, mn.player.mainBodyChunk.pos, 0.75f, 1f);

                    }
                ),
                new Music( // MEGALOVANIA
                    [0, 0, 7, 4],
                    mn => {

                        for (global::System.Int32 i = 0; i < mn.player.grasps.Length; i++)
                        {
                            AbstractPhysicalObject abstBomb = new AbstractPhysicalObject(mn.player.abstractCreature.world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, mn.player.abstractCreature.pos, mn.player.abstractCreature.world.game.GetNewID());
                            mn.player.abstractCreature.Room.AddEntity(abstBomb);
                            abstBomb.RealizeInRoom();

                            var grabbed = mn.player.grasps[i]?.grabbed;
                            mn.player.ReleaseGrasp(i);
                            grabbed?.Destroy();

                            mn.player.SlugcatGrab(abstBomb.realizedObject, i);
                            (abstBomb.realizedObject as ScavengerBomb).ignited = true;
                        }

                        for (global::System.Int32 i = 0; i < 3; i++)
                        {
                            AbstractPhysicalObject abstBBomb = new AbstractPhysicalObject(mn.player.abstractCreature.world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, mn.player.abstractCreature.pos, mn.player.abstractCreature.world.game.GetNewID());
                            mn.player.abstractCreature.Room.AddEntity(abstBBomb);
                            abstBBomb.RealizeInRoom();
                        }
                    }
                )

            ];
            List<int> playedNote = new List<int>();
            public void PlayMusic(int note)
            {
                //LogInfo("note played", note);

                playedNote.Add(note);

                foreach (var music in musics)
                {
                    if (music.notes.Length > playedNote.Count)
                        continue;

                    bool musicMatched = true;
                    //LogInfo("checking music", music.notes.FormatEnumarable(), playedNote.FormatEnumarable());
                    for (global::System.Int32 i = 0; i < music.notes.Length; i++)
                    {
                        //LogInfo("coparing", music.notes[music.notes.Length - 1 - i], playedNote[playedNote.Count - 1 - i]);
                        musicMatched &= playedNote[playedNote.Count - 1 - i] == music.notes[music.notes.Length - 1 - i];
     
                    }

                    if (musicMatched)
                    {
                        //LogInfo("music match for music", music.notes);
                        music.action(this);
                        playedNote.Clear();
                        return;
                    }
                }

                var maxValue = 64;
                if (playedNote.Count > maxValue)
                    playedNote.RemoveRange(0, playedNote.Count - maxValue);
            }

            public struct Music(int[] notes, Action<MusicNoot> action)
            {
                public int[] notes = notes;
                public Action<MusicNoot> action = action;
            }
            #endregion

            public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                sLeaser.sprites = new FSprite[2];
                sLeaser.sprites[0] = new FSprite("Futile_White", true)
                {
                    scale = 0.5f,
                };

                sLeaser.sprites[1] = new FSprite("Futile_White", true)
                {
                    scale = 0.5f,
                };

                AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("HUD"));
            }

            public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                var pPos = player.mainBodyChunk.pos;
                var visible = IsPlaying && lastNote > -1;

                sLeaser.sprites[0].isVisible = visible;
                sLeaser.sprites[0].SetPosition((pPos + MusicInputVec.normalized * 40f) - camPos);
                sLeaser.sprites[0].color = playerColor;

                sLeaser.sprites[1].isVisible = visible;
                var notePos = pPos + Custom.DegToVec(lastNote * 45f + 90f) * 30f;
                sLeaser.sprites[1].SetPosition(notePos - camPos);
                sLeaser.sprites[1].color = nootColor;

                base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            }
        }

        public class MusicNote : CosmeticSprite
        {
            public Player player;
            public Color color;
            public int maxLife = 40;
            public int life;

            public MusicNote(Player noteOwner, Vector2 pos, Vector2 vel, Color color)
            {
                player = noteOwner;
                this.color = color;
                this.pos = pos;
                this.lastPos = pos;
                this.vel = vel;
                life = maxLife;
            }

            public override void Update(bool eu)
            {
                base.Update(eu);

                var result = SharedPhysics.TraceProjectileAgainstBodyChunks(null, room, lastPos - vel, ref pos, 20f, 1, player, false);
                if (result.obj is Creature creature)
                {
                    creature.Stun(40);
                    room.AddObject(new CreatureSpasmer(creature, false, 40));
                }
                else if (result.obj is PhysicalObject obj)
                {
                    obj.firstChunk.vel += vel / obj.firstChunk.mass;
                }

                life--;
                if (life <= 0)
                {
                    Destroy();
                }
            }

            public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                sLeaser.sprites = new FSprite[1];
                sLeaser.sprites[0] = new FSprite("musicSymbol", true)
                {
                    scale = 1f,
                    color = color
                };

                AddToContainer(sLeaser, rCam, rCam.ReturnFContainer(FContainerLayer.HUD));
            }

            public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                var positon = Vector2.Lerp(lastPos, pos, timeStacker);
                positon += Custom.PerpendicularVector(vel.normalized) * (vel.magnitude / 2f) * (life % 5f / 4f) * (1f + Random.value);

                sLeaser.sprites[0].SetPosition(positon - camPos);

                if (life <= 10)
                {
                    sLeaser.sprites[0].alpha = Mathf.InverseLerp(0, 10, life);
                }
                base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            }


        }
    }

    file static class JavlinNoot
    {
        internal static void Unregister()
        {
            On.Player.GrabUpdate -= Player_GrabUpdate;
            On.Player.SlugcatGrab -= Player_SlugcatGrab;
            On.Player.ThrowObject -= Player_ThrowObject;
            On.BigNeedleWorm.Update -= BigNeedleWorm_Update;

        }

        internal static void Register()
        {
            On.Player.GrabUpdate += Player_GrabUpdate;
            On.Player.SlugcatGrab += Player_SlugcatGrab;
            On.Player.ThrowObject += Player_ThrowObject;
            On.BigNeedleWorm.Update += BigNeedleWorm_Update;
        }

        private static void BigNeedleWorm_Update(On.BigNeedleWorm.orig_Update orig, BigNeedleWorm self, bool eu)
        {
            orig(self, eu);

            if (!GameUtils.IsCompetitiveOrSandboxSession)
                return;



            if (self.grabbedBy.Count > 0 && self.grabbedBy[0].grabber is Player)
            {
                if (self.attackReady > 0f && !self.attackRefresh)
                {
                    self.attackReady = Custom.LerpAndTick(self.attackReady, 1f, 0f, 0.0375f);
                    if (self.chargingAttack < 0.1f)
                    {
                        self.chargingAttack = 0.1f;
                    }
                }
            }
        }

        private static void Player_ThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
        {
            BigNeedleWorm bigNoot = self.grasps[grasp]?.grabbed as BigNeedleWorm;

            orig(self, grasp, eu);

            if (bigNoot != null && bigNoot.grabbedBy.Count == 0 && GameUtils.IsCompetitiveOrSandboxSession)
            {
                bigNoot.chargingAttack = 0f;
                bigNoot.swishDir = self.ThrowDirection().ToVector2().normalized;
                bigNoot.swishCounter = 6;
                bigNoot.room.PlaySound(SoundID.Big_Needle_Worm_Attack, bigNoot.mainBodyChunk);
                bigNoot.attackRefresh = true;
            }
        }

        private static void Player_SlugcatGrab(On.Player.orig_SlugcatGrab orig, Player self, PhysicalObject obj, int graspUsed)
        {
            orig(self, obj, graspUsed);
            if (self.grasps[graspUsed]?.grabbed == obj && obj is BigNeedleWorm bigNoot && GameUtils.IsCompetitiveOrSandboxSession)
            {
                if (bigNoot.attackReady < 0.05f)
                {
                    bigNoot.attackReady = 0.05f;
                    bigNoot.controlledCharge = Vector2.zero;
                }
            }
        }

        private static void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            orig(self, eu);

            if (self.grasps[0]?.grabbed is not BigNeedleWorm bigNoot || !GameUtils.IsCompetitiveOrSandboxSession)
                return;

            if (bigNoot.attackReady > 0f && !bigNoot.attackRefresh)
            {
                bigNoot.attackReady = Custom.LerpAndTick(bigNoot.attackReady, 1f, 0f, 0.0375f);
                if (bigNoot.chargingAttack < 0.1f)
                {
                    bigNoot.chargingAttack = 0.1f;
                }
            }
        }
    }
}
