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
        category: BuiltInCategory.Fun
    )]


    // TODO: NOOT
    file class NOOT(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
    {
        private bool skipMovement;
        protected override void Unregister()
        {
            On.SmallNeedleWorm.BitByPlayer -= SmallNeedleWorm_BitByPlayer;
            On.NeedleWormGraphics.DrawSprites -= NeedleWormGraphics_DrawSprites;
            On.Player.checkInput -= Player_checkInput;
            On.Player.ThrowObject -= Player_ThrowObject;
        }

        protected override void Register()
        {
            On.SmallNeedleWorm.BitByPlayer += SmallNeedleWorm_BitByPlayer;
            On.NeedleWormGraphics.DrawSprites += NeedleWormGraphics_DrawSprites;
            On.Player.checkInput += Player_checkInput;
            On.Player.ThrowObject += Player_ThrowObject;

            On.PlayerGraphics.RippleTrailUpdate += PlayerGraphics_RippleTrailUpdate;
        }

        private void PlayerGraphics_RippleTrailUpdate(On.PlayerGraphics.orig_RippleTrailUpdate orig, PlayerGraphics self)
        {
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
            //if (!GameUtils.IsCompetitiveOrSandboxSession)
                //orig(self, grasp, eu);

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
                    player.RemoveAttachedFeature(this);
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

                smallNoot.room.AddObject(new MusicNote(player, smallNoot.mainBodyChunk.pos, Custom.DegToVec(note * 45f + 90f) * 1f));

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
                LogInfo("note played", note);

                playedNote.Add(note);

                foreach (var music in musics)
                {
                    if (music.notes.Length > playedNote.Count)
                        continue;

                    bool musicMatched = true;
                    LogInfo("checking music", music.notes.FormatEnumarable(), playedNote.FormatEnumarable());
                    for (global::System.Int32 i = 0; i < music.notes.Length; i++)
                    {
                        //LogInfo("coparing", music.notes[music.notes.Length - 1 - i], playedNote[playedNote.Count - 1 - i]);
                        musicMatched &= playedNote[playedNote.Count - 1 - i] == music.notes[music.notes.Length - 1 - i];
     
                    }

                    if (musicMatched)
                    {
                        LogInfo("music match for music", music.notes);
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
                    color = playerColor
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
            public int maxLife = 40 * 10;
            public int life;

            public MusicNote(Player noteOwner, Vector2 pos, Vector2 vel)
            {
                player = noteOwner;
                this.pos = pos;
                this.lastPos = pos;
                this.vel = vel;
                life = maxLife;
            }

            public override void Update(bool eu)
            {
                base.Update(eu);
                UpdateMask();
                life--;
                if (life <= 0)
                {
                    Destroy();
                }
            }

            public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                sLeaser.sprites = new FSprite[1];
                sLeaser.sprites[0] = new FSprite("Futile_White", true)
                {
                    scale = 100f,
                    shader = Custom.rainWorld.Shaders["WarpTear"]
                };


                sLeaser.maskSources = new MaskSource[1];
                mesh ??= this.CreateMesh();
                mask ??= MaskMaker.MakeSource("WarpTearGrab", "WarpTearMask", false, this.mesh, this.pos, Vector3.back, Vector2.one * 40f);
                sLeaser.maskSources[0] = mask;
                mask.SetProperty(0, 1f);
                mask.SetProperty(2, 1f);
                mask.SetPos(this.pos, true);
                mask.SetScale(Vector2.one * 40f, true);
                mask.SetRotation(Vector3.back, true);


                AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("WarpPoint"));
            }

            public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                sLeaser.sprites[0].SetPosition(Vector2.Lerp(lastPos, pos, timeStacker) - camPos);

                if (life <= 10)
                {
                    sLeaser.sprites[0].alpha = Mathf.InverseLerp(0, 10, life);
                }

                mask?.SetProperty(1, Random.value);
                mask?.DrawUpdate(timeStacker, rCam, camPos);

                base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            }

            private Mesh CreateMesh()
            {
                Mesh quad = MaskSource.Quad;
                quad.vertices = quad.vertices.Select((Vector3 v) => v + Vector3.up * 0.3f).ToArray<Vector3>();
                quad.RecalculateBounds();
                return quad;
            }

            private Mesh mesh;
            private MaskSource mask;
            private void UpdateMask()
            {
                if (mask == null || mask.beingDeleted)
                    return;

                mask.SetPos(this.pos, false);
                mask.SetScale(Vector2.one * 40f, false);
                mask.SetRotation(Vector3.zero, false);

                //tail.SetPos(pos, false);
                //tail.SetScale(Vector3.one * 40f, false);

                //tail.SetPos(this.player.mainBodyChunk.pos, false);
                //tail.SetScale(Vector3.one * 70f, false);
            }

        }
    }
}
