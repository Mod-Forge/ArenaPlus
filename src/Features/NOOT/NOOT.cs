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
using System.Timers;
using static ArenaPlus.Features.NOOT.NOOT.MusicNoot;
using ArenaPlus.Features.Fun;
using Unity.Burst.Intrinsics;

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

            musicNoot.noot = self;

            player.checkInput();
            musicNoot.lastPlayingInput = musicNoot.playingInput;
            musicNoot.playingInput = player.input[0];
            Vector2 vecInput = player.input[0].IntVec.ToVector2();

            var note = 0;
            var usingAnalogue = player.input[0].analogueDir.magnitude > 0f;
            if (player.input[0].analogueDir.magnitude > 0.6f || (usingAnalogue && vecInput.magnitude > 0f))
            {
                var angle = Custom.VecToDeg(usingAnalogue ? player.input[0].analogueDir.normalized : vecInput.normalized) - 90;

                // offset the angle for the middle to in the right pos
                if (usingAnalogue)
                    angle += 45f / 2f;

                while (angle < 0)
                    angle += 360;

                note = (int)(angle / 45) + 1;
            }

            // play note
            if (musicNoot.lastNote != note || (!musicNoot.lastPlayingInput.pckp && musicNoot.playingInput.pckp))
            {
                musicNoot.lastNote = note;



                if (musicNoot.playingInput.pckp && note > 0)
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

            public int lastNote = 0;
            public bool lockInput;
            public bool _playing;
            private bool wasPlaying;
            public bool IsPlaying => wasPlaying || _playing;
            public Player.InputPackage lastPlayingInput;
            public Player.InputPackage playingInput = new Player.InputPackage() { pckp = true };
            public Vector2 MusicInputVec => playingInput.analogueDir.magnitude > 0f ? playingInput.analogueDir.normalized : playingInput.IntVec.ToVector2().normalized;

            public SmallNeedleWorm noot;

            public override void Update(bool eu)
            {
                wasPlaying = _playing;
                base.Update(eu);
                DebugNoteUpdate();
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
                    smallNoot.room.PlaySound(SoundID.Small_Needle_Worm_Intense_Trumpet_Scream, smallNoot.mainBodyChunk.pos, 1f, Mathf.Lerp(0.475f, 0.95f, (float)(note - 1) / 7f) * 0.714285f);
                }
                else
                {
                    smallNoot.room.PlaySound(SoundID.Small_Needle_Worm_Intense_Trumpet_Scream, smallNoot.mainBodyChunk.pos, 1f, Mathf.Lerp(0.95f, 1.9f, (float)(note - 1) / 7f) * 0.714285f);
                }
                smallNoot.hasScreamed = true;
                if (smallNoot.Mother != null)
                {
                    smallNoot.Mother.BigAI.BigRespondCry();
                }

                smallNoot.room.AddObject(new MusicNote(player, smallNoot.mainBodyChunk.pos, Custom.DegToVec((note - 1) * 45f + 90f) * 5f, nootColor * 1.5f));

                PlayMusic(playingInput.mp ? -note : note, smallNoot);
            }

            #region sercet music

            private void ExplodePlayer()
            {
                for (global::System.Int32 i = 0; i < player.grasps.Length; i++)
                {
                    AbstractPhysicalObject abstBomb = new AbstractPhysicalObject(player.abstractCreature.world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, player.abstractCreature.pos, player.abstractCreature.world.game.GetNewID());
                    player.abstractCreature.Room.AddEntity(abstBomb);
                    abstBomb.RealizeInRoom();

                    var grabbed = player.grasps[i]?.grabbed;
                    player.ReleaseGrasp(i);
                    grabbed?.Destroy();

                    player.SlugcatGrab(abstBomb.realizedObject, i);
                    (abstBomb.realizedObject as ScavengerBomb).ignited = true;
                }

                for (global::System.Int32 i = 0; i < 3; i++)
                {
                    AbstractPhysicalObject abstBBomb = new AbstractPhysicalObject(player.abstractCreature.world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, player.abstractCreature.pos, player.abstractCreature.world.game.GetNewID());
                    player.abstractCreature.Room.AddEntity(abstBBomb);
                    abstBBomb.RealizeInRoom();
                }
            }

            // TODO: fix the crash
            [MyCommand("snow")]
            private void AddSnow()
            {
                if (!ModManager.DLCShared || room.roomSettings.DangerType == DLCSharedEnums.RoomRainDangerType.Blizzard)
                    return;

                room.roomSettings.DangerType = DLCSharedEnums.RoomRainDangerType.Blizzard;
                if (!room.updateList.Any(ud => ud is MoreSlugcats.ColdRoom))
                {
                    room.AddObject(new MoreSlugcats.ColdRoom(room));
                }

                if (room.roomRain != null)
                {
                    room.roomRain.Destroy();
                    room.RemoveObject(room.roomRain);
                    room.game.cameras.SelectMany(c => c.spriteLeasers).First(sl => sl.drawableObject == room.roomRain).CleanSpritesAndRemove();
                    room.roomRain = null;
                }

                room.AddSnow();

                MoreSlugcats.SnowSource snowSource = new MoreSlugcats.SnowSource(player.mainBodyChunk.pos);
                room.AddObject(snowSource);
                snowSource.rad = 1000000f;
                snowSource.intensity = 0.5f;
            }

            public void Nuke()
            {
                Custom.Log(new string[] { "SINGULARITY EXPLODE" });
                Vector2 vector = Vector2.Lerp(player.firstChunk.pos, player.firstChunk.lastPos, 0.35f);
                var explodeColor = new Color(0.2f, 0.2f, 1f);
                if (ModManager.MSC) room.AddObject(new MoreSlugcats.SingularityBomb.SparkFlash(player.firstChunk.pos, 300f, new Color(0f, 0f, 1f)));
                room.AddObject(new Explosion(room, player, vector, 7, 450f, 6.2f, 10f, 280f, 0.25f, null, 0.3f, 160f, 1f));
                room.AddObject(new Explosion(room, player, vector, 7, 2000f, 4f, 0f, 400f, 0.25f, null, 0.3f, 200f, 1f));
                room.AddObject(new Explosion.ExplosionLight(vector, 280f, 1f, 7, explodeColor));
                room.AddObject(new Explosion.ExplosionLight(vector, 230f, 1f, 3, new Color(1f, 1f, 1f)));
                room.AddObject(new Explosion.ExplosionLight(vector, 2000f, 2f, 60, explodeColor));
                room.AddObject(new ShockWave(vector, 750f, 1.485f, 300, true));
                room.AddObject(new ShockWave(vector, 3000f, 1.185f, 180, false));
                for (int i = 0; i < 25; i++)
                {
                    Vector2 vector2 = Custom.RNV();
                    if (room.GetTile(vector + vector2 * 20f).Solid)
                    {
                        if (!room.GetTile(vector - vector2 * 20f).Solid)
                        {
                            vector2 *= -1f;
                        }
                        else
                        {
                            vector2 = Custom.RNV();
                        }
                    }
                    for (int j = 0; j < 3; j++)
                    {
                        room.AddObject(new Spark(vector + vector2 * Mathf.Lerp(30f, 60f, global::UnityEngine.Random.value), vector2 * Mathf.Lerp(7f, 38f, global::UnityEngine.Random.value) + Custom.RNV() * 20f * global::UnityEngine.Random.value, Color.Lerp(explodeColor, new Color(1f, 1f, 1f), global::UnityEngine.Random.value), null, 11, 28));
                    }
                    room.AddObject(new Explosion.FlashingSmoke(vector + vector2 * 40f * global::UnityEngine.Random.value, vector2 * Mathf.Lerp(4f, 20f, Mathf.Pow(global::UnityEngine.Random.value, 2f)), 1f + 0.05f * global::UnityEngine.Random.value, new Color(1f, 1f, 1f), explodeColor, global::UnityEngine.Random.Range(3, 11)));
                }

                if (ModManager.MSC)
                {
                    for (int k = 0; k < 6; k++)
                    {
                        room.AddObject(new MoreSlugcats.SingularityBomb.BombFragment(vector, Custom.DegToVec(((float)k + global::UnityEngine.Random.value) / 6f * 360f) * Mathf.Lerp(18f, 38f, global::UnityEngine.Random.value)));
                    }
                }
                room.ScreenMovement(new Vector2?(vector), default(Vector2), 0.9f);

                this.room.PlaySound(SoundID.Bomb_Explode, player.firstChunk);
                this.room.InGameNoise(new Noise.InGameNoise(vector, 9000f, player, 1f));
                for (int m = 0; m < this.room.physicalObjects.Length; m++)
                {
                    for (int n = 0; n < this.room.physicalObjects[m].Count; n++)
                    {
                        if (this.room.physicalObjects[m][n] is Creature && (this.room.physicalObjects[m][n].abstractPhysicalObject.rippleLayer == player.abstractPhysicalObject.rippleLayer || this.room.physicalObjects[m][n].abstractPhysicalObject.rippleBothSides || player.abstractPhysicalObject.rippleBothSides) && Custom.Dist(this.room.physicalObjects[m][n].firstChunk.pos, player.firstChunk.pos) < 750f)
                        {
                            (this.room.physicalObjects[m][n] as Creature).SetKillTag(player.abstractCreature);
                            (this.room.physicalObjects[m][n] as Creature).Die();
                        }
                    }
                }
                FirecrackerPlant.ScareObject scareObject = new FirecrackerPlant.ScareObject(player.firstChunk.pos, player.abstractPhysicalObject.rippleLayer);
                scareObject.fearRange = 12000f;
                scareObject.fearScavs = true;
                scareObject.lifeTime = -600;
                this.room.AddObject(scareObject);
                this.room.InGameNoise(new Noise.InGameNoise(player.firstChunk.pos, 12000f, player, 1f));
                this.room.AddObject(new UnderwaterShock(this.room, null, player.firstChunk.pos, 10, 1200f, 2f, player, new Color(0.8f, 0.8f, 1f)));
            }

            static List<Music> musics = [
                new Music(
                    "warp",
                    [1, 1, 5, 5, 3, 3, 7, 7],
                    mn => {
                        mn.player.SuperHardSetPosition(mn.player.room.MiddleOfTile(mn.player.room.LocalCoordinateOfNode(Random.Range(0, mn.player.room.abstractRoom.nodes.Length)).Tile));
                        mn.player.room.PlaySound(Sounds.Noot_Warp, mn.player.mainBodyChunk.pos, 0.75f, 1f);
                        return false;
                    }
                ),
                new Music(
                    "sus",
                    [1, 3, 4, 5, 4, 3, 2, 1, 3, 2],
                    mn => {
                        mn.noot.Destroy();
                        mn.player.ObjectEaten(mn.noot);
                        return true;
                    }
                ),
                new Music(
                    "dead",
                    [1, 1, 8, 5],
                    mn => {
                        mn.ExplodePlayer();
                        return true;
                    }
                ),
                new Music(
                    "dead 2",
                    [-5, -5, 5, 2],
                    mn => {
                        mn.ExplodePlayer();
                        return true;
                    }
                ),
                new Music(
                    "blind song",
                    [2, 3, 4, 6, 4, 6, 4, 6],
                    mn => {
                        AbstractConsumable abstractConsumable = new AbstractConsumable(mn.player.abstractCreature.world, AbstractPhysicalObject.AbstractObjectType.FlareBomb, null, mn.player.abstractCreature.pos, mn.player.abstractCreature.world.game.GetNewID(), -1, -1, null);
                        mn.player.abstractCreature.Room.AddEntity(abstractConsumable);
                        abstractConsumable.RealizeInRoom();
                        (abstractConsumable.realizedObject as FlareBomb).StartBurn();
                        return false;
                    }
                ),
                new Music(
                    "bad noot",
                    [2, 5, 5, 4, 5, 2],
                    mn => {
                        mn.noot.Destroy();
                        for (global::System.Int32 i = 0; i < mn.player.grasps.Length; i++) { mn.player.ReleaseGrasp(i); }

                        var world = mn.player.abstractCreature.world; var pos = mn.player.abstractCreature.pos; var id = mn.noot.abstractCreature.ID; // var id = mn.player.abstractCreature.world.game.GetNewID();
                        AbstractCreature abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BigNeedleWorm), null, pos, id);
                        mn.player.abstractCreature.Room.AddEntity(abstractCreature);
                        abstractCreature.RealizeInRoom();
                        abstractCreature.realizedCreature.Die();

                        mn.player.SlugcatGrab(abstractCreature.realizedCreature, 0);
                        return true;
                    }
                ),
                new Music(
                    "water song",
                    [7, 5, 3, 1, 3, 5, 7, 5, 3, 1, 3, 5, 7, 5, 3, 1, 3, 5, 7, 5, 3, 1, 3, 5, 7, 4, 2],
                    mn => {
                        var room = mn.player.room;

                        int maxLevel = room.Height;
                        room.defaultWaterLevel = maxLevel;
                        room.floatWaterLevel = room.MiddleOfTile(new IntVector2(0, room.defaultWaterLevel)).y;
                        if (!room.water)
                        {
                            room.waterInFrontOfTerrain = true;
                        }
                        else
                        {
                            room.waterObject.Destroy();
                            room.waterObject = null;
                        }
                        room.AddWater();
                        room.waterObject.WaterIsLethal = false;
                        return true;
                    }
                ),
                new Music(
                    "picture of the past",
                    [6, 4, 6, 4, 6, 4, 6, 4, 5, 3, 5, 3, 5, 3, 5, 3],
                    mn => {
                        Creature.Grasp[] graspCopy = (Creature.Grasp[]) mn.player.grasps.Clone();
                        Player newPlayer = SlugcatsUtils.RecreatePlayerWithClass(mn.player, SlugcatsUtils.GetRandomSlugcat());
                        for (global::System.Int32 i = 0; i < graspCopy.Length; i++) {           
                            if (graspCopy[i] is Creature.Grasp grasp) {
                                newPlayer.SlugcatGrab(grasp.grabbed, grasp.graspUsed);
                            }
                        }
                        return true;
                    }
                ),
                new Music(
                    "monkey city",
                    [3, 4, 5, 7, 3, 4, 5, 7, 3, 4, 5, 7, 3, 4, 5, 7],
                    mn => {
                        var world = mn.player.abstractCreature.world; var pos = mn.player.abstractCreature.pos;

                        int amount = ModManager.MSC ? 1 :
                                     ModManager.DLCShared ? 2 :
                                     3;
                        for (global::System.Int32 i = 0; i < amount; i++)
                        {
                            var id = mn.player.abstractCreature.world.game.GetNewID();

                            AbstractCreature abstractScav = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(ModManager.MSC ? MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing
                                                                                                                      : ModManager.DLCShared ? DLCSharedEnums.CreatureTemplateType.ScavengerElite
                                                                                                                      : CreatureTemplate.Type.Scavenger), null, pos, id);
                            mn.room.abstractRoom.AddEntity(abstractScav);
                            (abstractScav.abstractAI as ScavengerAbstractAI).InitGearUp();
                            abstractScav.RealizeInRoom();

                            for (int j = 0; j < world.game.Players.Count; j++)
                            {
                                abstractScav.state.socialMemory.GetOrInitiateRelationship(world.game.Players[j].ID).like = -1f;
                                abstractScav.state.socialMemory.GetOrInitiateRelationship(world.game.Players[j].ID).tempLike = -1f;
                            }
                            abstractScav.state.socialMemory.GetOrInitiateRelationship(mn.player.abstractCreature.ID).like = 1f;
                            abstractScav.state.socialMemory.GetOrInitiateRelationship(mn.player.abstractCreature.ID).tempLike = 1f;

                            ScavengerMindControl.SetScavengerLover(abstractScav.realizedCreature as Scavenger, mn.player.abstractCreature);
                        }
                        return true;
                    }
                ),

                new Music(
                    "old memory",
                    [-1, -5, 1, 2, 3, 2, 1, -5, -1, -5, 1, 2, 3],
                    mn => {
                        mn.AddSnow();
                        return true;
                    },
                    octave: true
                ),
                new Music(
                    "lost moon",
                    [-3, -5, -6, 2, 6, 7, 4, 2, 5, -6, -7, 4, -6, -7],
                    mn => {
                        mn.AddSnow();
                        return true;
                    },
                    octave: true
                ),
                new Music(
                    "lack of drug",
                    [3, 3, 1, 1, -6, -6, -3, -3],
                    mn => {
                        mn.player.mushroomCounter += 320;
                        return true;
                    },
                    octave: true
                ),
                new Music(
                    "open sky",
                    [-1, 1, 2, 3, 5, -1, 1, 2, 3, -1, 1, 2, 3, 5, -1, 1, 2, 3],
                    mn => {
                        if (!mn.room.abstractRoom.nodes.Any(n => n.type == AbstractRoomNode.Type.SkyExit))
                            return true;

                        
                        var skyNode = mn.room.abstractRoom.nodes.FirstOrDefault(n => n.type == AbstractRoomNode.Type.SkyExit);
                        var world = mn.player.abstractCreature.world; var id = mn.player.abstractCreature.world.game.GetNewID();
                        var pos = WorldCoordinate.AddIntVector(mn.room.LocalCoordinateOfNode(Array.IndexOf(mn.room.abstractRoom.nodes, skyNode)), new IntVector2(0, 20));

                        AbstractCreature abstractVulture = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.KingVulture), null, pos, id);
                        mn.room.abstractRoom.AddEntity(abstractVulture);
                        abstractVulture.RealizeInRoom();
                        abstractVulture.realizedCreature.mainBodyChunk.vel.y -= 100f;

                        foreach (var abstractCreature in world.game.Players) {
                            if (abstractCreature?.realizedCreature is Player player) {
                                abstractVulture.abstractAI.RealAI.preyTracker.AddPrey(abstractVulture.abstractAI.RealAI.tracker.RepresentationForObject(player, true));
                            }
                        }
                        return true;
                    },
                    octave: true
                ){
                },
                new Music(
                    "last dream",
                    [1, -6, 3, -6, 1, -6, 1, 2, 3, -7, -5, 3, -5, -7, -5, -7, 1, 2, -6, -4, 1, -4, -6],
                    mn => {
                        int grasp = mn.noot.grabbedBy[0].graspUsed;
                        mn.player.ReleaseGrasp(grasp);


                        AbstractConsumable abstractConsumable = new AbstractConsumable(mn.player.abstractCreature.world, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, null, mn.player.abstractCreature.pos, mn.player.abstractCreature.world.game.GetNewID(), -1, -1, null);
                        mn.player.abstractCreature.Room.AddEntity(abstractConsumable);
                        abstractConsumable.RealizeInRoom();
                        mn.player.SlugcatGrab(abstractConsumable.realizedObject, grasp);

                        return true;
                    },
                    octave: true
                ),
                new Music(
                    "last minute",
                    [0],
                    mn => {
                        // TODO: end the round
                        return true;
                    },
                    octave: true
                ),
                new Music(
                    "travelers",
                    [-1, -5, -7, -5, 1, -7, -6, -5, -6, -7, -5, -2, -5, -7, -5, 1, -7, -6, -5, -6, -7, 2, -7, -3, -5, -7, -5, 1, -7, -6, -5, -6, -7, -5],
                    mn => {
                        mn.Nuke();
                        return true;
                    },
                    octave: true
                ),
            ];


            List<int> playedNotes = new List<int>();
            public void PlayMusic(int note, SmallNeedleWorm instrument)
            {
                //LogInfo("note played", note);

                playedNotes.Add(note);

                foreach (var music in musics)
                {
                    if (music.notes.Length > playedNotes.Count)
                        continue;

                    bool musicMatched = true;
                    //LogInfo("checking music", music.notes.FormatEnumarable(), playedNotes.FormatEnumarable());
                    for (global::System.Int32 i = 0; i < music.notes.Length; i++)
                    {
                        int musicNote = music.notes[music.notes.Length - 1 - i];
                        int playedNote = playedNotes[playedNotes.Count - 1 - i];
                        if (!music.octave)
                        {
                            musicNote = Mathf.Abs(musicNote);
                            playedNote = Mathf.Abs(playedNote);
                        }
                        else
                        {
                            if (playedNote == -8)
                                playedNote = 1;
                        }
                        //LogInfo("coparing", musicNote, playedNote);

                        musicMatched &= playedNote == musicNote;
     
                    }

                    if (musicMatched)
                    {
                        //LogInfo("music match for music", music.notes);
                        bool removeNoot = music.action(this);
                        LogUnity("Played", FormatObject(music.name));
                        if (removeNoot)
                        {
                            instrument.Destroy();
                            LogUnity("Noot removed");
                        }
                        playedNotes.Clear();
                        return;
                    }
                }

                var maxValue = 64;
                if (playedNotes.Count > maxValue)
                    playedNotes.RemoveRange(0, playedNotes.Count - maxValue);
            }

            #region debug player
            [MyCommand("log_notes")]
            private static void LogPlayedNotes(Player player)
            {
                if (player.GetAttachedFeatureType<MusicNoot>() is MusicNoot musicNoot)
                {
                    ConsoleWrite(musicNoot.playedNotes.FormatEnumarable());
                    return;
                }

                ConsoleWrite("Failed to find player music", Color.red);
            }


            private Music _debugPlayMusic;
            private List<int> _debugNotesList = null;

            [MyCommand("play_music")]
            private static void PlayMusic(Player player, [Values(typeof(MusicNoot), nameof(GetMusicNames))] string musicName)
            {
                if (player.GetAttachedFeatureType<MusicNoot>() is MusicNoot musicNoot)
                {
                    musicNoot._debugPlayMusic = musics.First(m => m.name.ToLower() == musicName.ToLower());
                    musicNoot._debugNotesList = musicNoot._debugPlayMusic.notes.ToList();
                    musicNoot._debugNoteCooldown = 40;
                    return;
                }

                ConsoleWrite("Failed to find player music", Color.red);
            }

            private static string[] GetMusicNames()
            {
                return musics.ConvertAll(m => m.name).ToArray();
            }

            private int _debugNoteCooldown;
            private void DebugNoteUpdate()
            {
                if (_debugNotesList == null)
                    return;

                _debugNoteCooldown--;
                if (_debugNoteCooldown > 0)
                    return;

                _debugNoteCooldown = 15;

                PlayNote(_debugNotesList[0]);
                _debugNotesList.RemoveAt(0);

                if (_debugNotesList.Count == 0)
                {
                    LogUnity("Played", FormatObject(_debugPlayMusic.name));
                    _debugPlayMusic.action(this);
                    _debugNotesList = null;
                }

            }
            #endregion

            

            private void PlayNote(int note)
            {
                LogInfo("playing note", note);
                if (note < 0)
                {
                    player.room.PlaySound(SoundID.Small_Needle_Worm_Intense_Trumpet_Scream, player.mainBodyChunk.pos, 1f, Mathf.Lerp(0.475f, 0.95f, (float)(Mathf.Abs(note) - 1) / 7f) * 0.714285f);
                }
                else
                {
                    player.room.PlaySound(SoundID.Small_Needle_Worm_Intense_Trumpet_Scream, player.mainBodyChunk.pos, 1f, Mathf.Lerp(0.95f, 1.9f, (float)(note - 1) / 7f) * 0.714285f);
                }
            }

            public struct Music(string name, int[] notes, Func<MusicNoot, bool> action, bool octave = false)
            {
                public string name = name;
                public int[] notes = notes;
                public Func<MusicNoot, bool> action = action;
                public bool octave = octave;
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
                var visible = IsPlaying && lastNote != 0;

                sLeaser.sprites[0].isVisible = visible;
                sLeaser.sprites[0].SetPosition((pPos + MusicInputVec.normalized * 40f) - camPos);
                sLeaser.sprites[0].color = playerColor;

                sLeaser.sprites[1].isVisible = visible;
                var notePos = pPos + Custom.DegToVec((lastNote - 1) * 45f + 90f) * 30f;
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
                    creature.mainBodyChunk.vel += vel * 0.25f;
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
