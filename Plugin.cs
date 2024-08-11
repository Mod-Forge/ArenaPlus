global using static ArenaSlugcatsConfigurator.Plugin;
using BepInEx;
using BepInEx.Logging;
using MoreSlugcats;
using RWCustom;
using Menu;
using System.Security.Permissions;
using UnityEngine;
using DevConsole.Commands;
using static PlayerProgression;
using System.Collections.Generic;
using System.Threading;
using System;
using Random = UnityEngine.Random;
using System.IO;
using System.Linq;


#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618
namespace ArenaSlugcatsConfigurator
{
    [BepInPlugin("ddemile.arenaslugcatsconfigurator", "Arena Slugcats Configurator", "1.3.3")] // (GUID, mod name, mod version)
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource logSource = BepInEx.Logging.Logger.CreateLogSource("ArenaSlugcatsConfigurator");
        public static MultiplayerUnlocks multiplayerUnlocks;
        public static List<AbstractPhysicalObject> arenaEggs = new();
        public SymbolButton presetButton;
        public MenuLabel presetLabel;
        public int currentPreset = 0;
        private static string separator = Path.DirectorySeparatorChar.ToString();
        public static string configFolerPath = Application.persistentDataPath + Path.DirectorySeparatorChar.ToString() + "ModConfigs" + separator + "arenaslugcatsconfigurator";
        public string presetsFilePath = configFolerPath + separator + "arena_presets.txt";
        public Timer spearRespawnTimer;
        public int spearsCheckCoolDown = 0;
        public int SlugcatCount
        {
            get
            {
                return RainWorld.PlayerObjectBodyColors.Length;
            }
        }

        public void OnEnable()
        {
            Log("Mod enabled", "info");
            try { RegisterCommands(); }
            catch { }


            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            Freatures.FeaturesManager.Register();

            On.Menu.MultiplayerMenu.InitializeSitting += MultiplayerMenu_InitializeSitting;
            //On.Menu.MultiplayerMenu.Update += MultiplayerMenu_Update;
            On.Menu.MultiplayerMenu.NextClass += MultiplayerMenu_NextClass;

            On.Menu.LevelSelector.LevelsList.ctor += LevelsList_ctor;
            On.Menu.LevelSelector.LevelsPlaylist.ctor += LevelsPlaylist_ctor;
            On.Menu.LevelSelector.LevelsPlaylist.Singal += LevelsPlaylist_Singal;
            On.Menu.LevelSelector.LevelsPlaylist.Update += LevelsPlaylist_Update;
            On.Menu.MultiplayerMenu.UpdateInfoText += MultiplayerMenu_UpdateInfoText;

            On.Player.ThrowObject += Player_ThrowObject;
            On.Player.CanIPickThisUp += Player_CanIPickThisUp;
            On.Player.Regurgitate += Player_Regurgitate;
            On.Player.ClassMechanicsArtificer += Player_ClassMechanicsArtificer;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;

            On.Room.Loaded += Room_Loaded;

            On.ArenaSitting.SessionEnded += ArenaSitting_SessionEnded;
            On.ArenaGameSession.SpawnPlayers += ArenaGameSession_SpawnPlayers;
            On.ArenaGameSession.Update += ArenaGameSession_Update;
            On.Menu.PlayerResultBox.ctor += PlayerResultBox_ctor;


            On.MoreSlugcats.SingularityBomb.ctor += SingularityBomb_ctor;
            On.JokeRifle.Use += JokeRifle_Use;
            On.MoreSlugcats.AbstractBullet.ctor += AbstractBullet_ctor;

            On.Spear.HitSomething += Spear_HitSomething;
            On.PuffBall.Explode += PuffBall_Explode;

            //On.FirecrackerPlant.Explode += FirecrackerPlant_Explode;
        }

        //private void FirecrackerPlant_Explode(On.FirecrackerPlant.orig_Explode orig, FirecrackerPlant self)
        //{
        //    if (self.room.game.IsArenaSession && !IsChallengeGameSession(self.room.game))
        //    {
        //        Explosion obj = new Explosion(self.room, self, self.firstChunk.pos, 6, 60f, 5f, 0.0f, 100f, 0.5f, null, 1f, 0f, 1f);
        //        self.room.AddObject(obj);
        //    }
        //    orig(self);
        //}

        private void PuffBall_Explode(On.PuffBall.orig_Explode orig, PuffBall self)
        {
            //ConsoleWrite("puff explode " + self.slatedForDeletetion);
            if (self.slatedForDeletetion)
            {
                orig(self);
                return;
            }

            //ConsoleWrite("creatures count: " + self.room.abstractRoom.creatures.Count);
            //ConsoleWrite();
            for (int i = 0; i < self.room.abstractRoom.creatures.Count; i++)
            {
                if (self.room.abstractRoom.creatures[i].realizedCreature != null)
                {
                    //ConsoleWrite($"try stun slugcat: {self.room.abstractRoom.creatures[i].realizedCreature.Template.type == CreatureTemplate.Type.Slugcat}, {Vector2.Distance(self.firstChunk.pos, self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos)}");
                    if (self.room.abstractRoom.creatures[i].realizedCreature.Template.type == CreatureTemplate.Type.Slugcat && Custom.DistLess(self.firstChunk.pos, self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos, (1f + 20f) * 3))
                    {
                        int stun = Mathf.RoundToInt(20f * Random.value * 3f / Mathf.Lerp(self.room.abstractRoom.creatures[i].realizedCreature.TotalMass, 1f, 0.15f));
                        self.room.abstractRoom.creatures[i].realizedCreature.Stun(stun);
                        //ConsoleWrite("stun slugcat for " + stun);
                    }
                }
            }

            orig(self);
        }

        private void Player_ClassMechanicsArtificer(On.Player.orig_ClassMechanicsArtificer orig, Player self)
        {
            float parryCooldown = self.pyroParryCooldown;
            orig(self);
            if (self.pyroParryCooldown > parryCooldown)
            {
                if (ModManager.MSC && Options.nerfArtificer.Value && !self.room.game.IsStorySession && !IsChallengeGameSession(self.room.game))
                {
                    ConsoleWrite("nerf Artificer");
                    self.pyroJumpCounter += 5;
                    self.Stun(60 * (self.pyroJumpCounter - (Mathf.Max(1, MoreSlugcats.MoreSlugcats.cfgArtificerExplosionCapacity.Value - 3) - 1)));
                    //orig(self);
                }
            }

        }

        private void AbstractBullet_ctor(On.MoreSlugcats.AbstractBullet.orig_ctor orig, AbstractBullet self, World world, Bullet realizedObject, WorldCoordinate pos, EntityID ID, JokeRifle.AbstractRifle.AmmoType type, int timeToLive)
        {
            orig(self, world, realizedObject, pos, ID, type, timeToLive);

            if ((bool)world?.game?.IsArenaSession && !IsChallengeGameSession(world?.game))
            {
                self.timeToLive = 40;
            }
        }

        private void JokeRifle_Use(On.JokeRifle.orig_Use orig, JokeRifle self, bool eu)
        {
            if (self.room.game.IsArenaSession && !IsChallengeGameSession(self.room.game) && self.counter < 1 && self.abstractRifle.currentAmmo() > 0 && self.abstractRifle.ammoStyle == JokeRifle.AbstractRifle.AmmoType.Pearl)
            {
                Vector2 pos = self.firstChunk.pos + self.aimDir;
                Vector2 dir = pos + self.aimDir * 200f;
                SharedPhysics.CollisionResult collisionResult = SharedPhysics.TraceProjectileAgainstBodyChunks(null, self.room, pos, ref dir, 1f, 1, (self.grabbedBy[0].grabber as PhysicalObject), false);
                if (collisionResult.chunk != null)
                {
                    AbstractPhysicalObject abtractObject = collisionResult.chunk.owner.abstractPhysicalObject;
                    bool shoot = true;

                    string text = "result: " + abtractObject.type;

                    if (abtractObject is AbstractCreature)
                    {
                        text += " " + (abtractObject as AbstractCreature).creatureTemplate.name;
                        if (abtractObject == self.abstractPhysicalObject && ((abtractObject as AbstractCreature).creatureTemplate.type == CreatureTemplate.Type.Slugcat && self.grabbedBy[0].grabber == (abtractObject.realizedObject as Creature)))
                        {
                            shoot = false;
                            text += " (shoot ower/self)";
                        }
                    }

                    //ConsoleWrite(text);

                    if (shoot)
                    {
                        float falloff = Vector2.Distance(pos, collisionResult.chunk.pos) / 20;
                        abtractObject.realizedObject.firstChunk.vel = self.aimDir * 100f / falloff;
                    }
                }
            }
            orig(self, eu);
        }

        private string MultiplayerMenu_UpdateInfoText(On.Menu.MultiplayerMenu.orig_UpdateInfoText orig, MultiplayerMenu self)
        {
            if (self.selectedObject is SymbolButton)
            {
                string text = (self.selectedObject as SymbolButton).signalText;

                if (text == "CHANGE_PRESET")
                {
                    switch (currentPreset)
                    {
                        case 0: return "Preset A levels selected";
                        case 1: return "Preset B levels selected";
                        case 2: return "Preset C levels selected";
                    }
                }
            }

            return orig(self);
        }

        private void LevelsPlaylist_Update(On.Menu.LevelSelector.LevelsPlaylist.orig_Update orig, LevelSelector.LevelsPlaylist self)
        {
            orig(self);

            switch (currentPreset)
            {
                case 0:
                    presetLabel.text = "Preset A";
                    break;
                case 1:
                    presetLabel.text = "Preset B";
                    break;
                case 2:
                    presetLabel.text = "Preset C";
                    break;
            }
        }

        private void LevelsPlaylist_Singal(On.Menu.LevelSelector.LevelsPlaylist.orig_Singal orig, LevelSelector.LevelsPlaylist self, MenuObject sender, string message)
        {
            if (message != null)
            {
                if (message == "CHANGE_PRESET")
                {
                    UpdatePresetsFile(currentPreset, string.Join(",", self.PlayList));
                    currentPreset = (currentPreset + 1) % 3;

                    (self.owner as LevelSelector).GetMultiplayerMenu.GetGameTypeSetup.playList = (from level in ReadPresetsFile()[currentPreset].Split(',') where level != "" select level).ToList();

                    UpdatePresetsFile(3, currentPreset.ToString());

                    self.ResolveMismatch();

                    //ConsoleWrite("PlayList : " + string.Join(", ", self.PlayList));

                    presetButton.UpdateSymbol(GetPresetIcon());

                    self.menu.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
                    return;
                }
            }

            orig(self, sender, message);
        }

        string GetPresetIcon()
        {
            return currentPreset switch
            {
                0 => "Sandbox_A",
                1 => "Sandbox_B",
                2 => "Sandbox_C",
                _ => throw new Exception("Invalid preset"),
            };
        }

        string[] ReadPresetsFile()
        {
            if (!File.Exists(presetsFilePath))
            {
                string[] lines = new string[4];
                lines[3] = currentPreset.ToString();
                File.WriteAllLines(presetsFilePath, lines);
                return lines;
            }

            return File.ReadAllLines(presetsFilePath);
        }

        void UpdatePresetsFile(int line, string content)
        {
            string[] lines = ReadPresetsFile();

            lines[line] = content;

            File.WriteAllLines(presetsFilePath, lines);
        }

        private void LevelsPlaylist_ctor(On.Menu.LevelSelector.LevelsPlaylist.orig_ctor orig, LevelSelector.LevelsPlaylist self, Menu.Menu menu, LevelSelector owner, Vector2 pos)
        {
            orig(self, menu, owner, pos);

            currentPreset = int.Parse(ReadPresetsFile()[3]);

            // Right hand line
            FSprite[] rightHandLines = new FSprite[self.rightHandLines.Length + 1];

            Array.Copy(self.rightHandLines, rightHandLines, self.rightHandLines.Length);

            FSprite rightHandLine = new FSprite("pixel", true);
            rightHandLine.anchorX = 0f;
            rightHandLine.anchorY = 0f;
            rightHandLine.scaleX = 2f;
            self.Container.AddChild(rightHandLine);

            rightHandLines[rightHandLines.Length - 1] = rightHandLine;

            self.rightHandLines = rightHandLines;

            // Button
            presetButton = new SymbolButton(menu, self, GetPresetIcon(), "CHANGE_PRESET", self.sideButtons[self.sideButtons.Length - 2].pos + new Vector2(0f, 30f));
            presetButton.symbolSprite.scale = 0.7f;

            self.sideButtons[self.sideButtons.Length - 1] = presetButton;

            self.subObjects.Add(presetButton);

            MenuLabel[] labels = new MenuLabel[self.labels.Length + 1];

            Array.Copy(self.labels, labels, self.labels.Length);

            // Label
            presetLabel = new MenuLabel(menu, self, "", presetButton.pos + new Vector2(10f, -3f), new Vector2(50f, 30f), false, null);
            presetLabel.text = menu.Translate("Clear playlist");
            presetLabel.label.alignment = FLabelAlignment.Left;

            labels[labels.Length - 1] = presetLabel;

            self.labels = labels;

            self.subObjects.Add(presetLabel);

            float[,] labelsFade = new float[self.labelsFade.Length + 1, 2];

            Array.Copy(self.labelsFade, labelsFade, self.labelsFade.Length);

            self.labelsFade = labelsFade;
        }

        private void LevelsList_ctor(On.Menu.LevelSelector.LevelsList.orig_ctor orig, LevelSelector.LevelsList self, Menu.Menu menu, MenuObject owner, Vector2 pos, int extraSideButtons, bool shortList)
        {
            orig(self, menu, owner, pos, extraSideButtons, shortList);

            SymbolButton[] sideButtons = new SymbolButton[self.sideButtons.Length + 1];

            Array.Copy(self.sideButtons, sideButtons, self.sideButtons.Length);

            self.sideButtons = sideButtons;

            Directory.CreateDirectory(configFolerPath);
        }

        private void SingularityBomb_ctor(On.MoreSlugcats.SingularityBomb.orig_ctor orig, SingularityBomb self, AbstractPhysicalObject abstractPhysicalObject, World world)
        {
            orig(self, abstractPhysicalObject, world);

            //ConsoleWrite($"SingularityEgg {arenaEggs.Contains(abstractPhysicalObject)}");

            if (arenaEggs.Contains(abstractPhysicalObject))
            {
                self.zeroMode = true;
                self.explodeColor = new Color(1f, 0.2f, 0.2f);
                self.connections = new CoralBrain.Mycelium[0];
                self.holoShape = null;
            }
        }

        private bool Spear_HitSomething(On.Spear.orig_HitSomething orig, Spear self, SharedPhysics.CollisionResult result, bool eu)
        {
            if (!Options.enableMaskBlock.Value || self.room.game.IsStorySession)
            {
                return orig(self, result, eu);
            }
            if (result.obj != null && result.obj is Creature && result.obj is Player)
            {
                Player player = (Player)result.obj;
                for (int i = 0; i < player.grasps.Length; i++)
                {
                    if (player.grasps[i] != null)
                    {
                        VultureMask vultureMask = player.grasps[i].grabbed as VultureMask;
                        if (vultureMask != null && vultureMask.donned > 0)
                        {
                            if (!vultureMask.King || Random.value > 0.33333)
                            {
                                for (int n = 17; n > 0; n--)
                                {
                                    vultureMask.room.AddObject(new Spark(vultureMask.firstChunk.pos, Custom.RNV() * 2, Color.white, null, 10, 20));
                                }
                                vultureMask.Destroy();
                            }
                            self.room.PlaySound(SoundID.Spear_Bounce_Off_Creauture_Shell, self.firstChunk);
                            self.vibrate = 20;
                            self.ChangeMode(Weapon.Mode.Free);
                            self.firstChunk.vel = self.firstChunk.vel * -0.5f + Custom.DegToVec(Random.value * 360f) * Mathf.Lerp(0.1f, 0.4f, Random.value) * self.firstChunk.vel.magnitude;
                            self.SetRandomSpin();
                            return false;
                        }
                    }
                }
            }
            return orig(self, result, eu);
        }

        public class RandomObject
        {
            public RandomObject(AbstractPhysicalObject.AbstractObjectType type, int chance)
            {
                this.type = type;
                this.chance = chance;
            }
            public AbstractPhysicalObject.AbstractObjectType type;
            public int chance;
        }
        public AbstractPhysicalObject.AbstractObjectType getRandomObject(List<RandomObject> list)
        {
            int sum = 0;
            for (int i = 0; i < list.Count; i++)
            {
                sum += list[i].chance;
            }

            int rand = Random.Range(0, sum);

            sum = 0;
            for (int i = 0; i < list.Count; i++)
            {
                sum += list[i].chance;
                if (sum > rand)
                {
                    //ConsoleWrite("Type Generated");
                    return list[i].type;
                }
            }
            return list[Random.Range(0, list.Count)].type;
        }
        public AbstractPhysicalObject getAbstractPhysicalObject(AbstractPhysicalObject.AbstractObjectType type, Room room, WorldCoordinate pos)
        {
            EntityID entityID = room.game.GetNewID();
            if (type == AbstractPhysicalObject.AbstractObjectType.VultureMask)
            {
                return new VultureMask.AbstractVultureMask(room.world, null, pos, entityID, entityID.RandomSeed, false);
            }
            if (type == AbstractPhysicalObject.AbstractObjectType.DataPearl)
            {
                return new DataPearl.AbstractDataPearl(room.world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, pos, entityID, -1, -1, null, new DataPearl.AbstractDataPearl.DataPearlType(ExtEnum<DataPearl.AbstractDataPearl.DataPearlType>.values.entries[Random.Range(0, ExtEnum<DataPearl.AbstractDataPearl.DataPearlType>.values.entries.Count)], false));
            }
            if (type == MoreSlugcatsEnums.AbstractObjectType.FireEgg)
            {
                return new FireEgg.AbstractBugEgg(room.world, null, pos, entityID, Mathf.Lerp(0.35f, 0.6f, Custom.ClampedRandomVariation(0.5f, 0.5f, 2f)));
            }
            if (type == MoreSlugcatsEnums.AbstractObjectType.MoonCloak)
            {
                return new AbstractConsumable(room.world, MoreSlugcatsEnums.AbstractObjectType.MoonCloak, null, pos, entityID, -1, -1, null);
            }
            if (AbstractConsumable.IsTypeConsumable(type))
            {
                return new AbstractConsumable(room.world, type, null, pos, entityID, -1, -1, null);
            }

            return new AbstractPhysicalObject(room.world, type, null, pos, entityID);
        }
        private void Room_Loaded(On.Room.orig_Loaded orig, Room self)
        {
            orig(self);
            if (Options.enableRandomObjects.Value && self.game != null && self.game.IsArenaSession && !IsChallengeGameSession(self.game) && self.game.session is not SandboxGameSession)
            {
                List<AbstractPhysicalObject> addObjects = new List<AbstractPhysicalObject>();
                for (int i = 0; i < self.abstractRoom.entities.Count; i++)
                {
                    AbstractPhysicalObject obj = (AbstractPhysicalObject)self.abstractRoom.entities[i];
                    if (obj != null)
                    {
                        AbstractPhysicalObject newObject = null;
                        bool destroy = false;

                        if (obj.type == AbstractPhysicalObject.AbstractObjectType.Spear)
                        {
                            float random = Random.value;
                            //ConsoleWrite("Random value : " + random);
                            if (Random.value < 0.25 || (obj as AbstractSpear).explosive || (obj as AbstractSpear).electric || (obj as AbstractSpear).hue != 0f)
                            {
                                if (random < 0.25f)
                                {
                                    newObject = new AbstractSpear(self.world, null, self.abstractRoom.entities[i].pos, self.game.GetNewID(), true, false);
                                }
                                else if (random < 0.5f)
                                {
                                    newObject = new AbstractSpear(self.world, null, self.abstractRoom.entities[i].pos, self.game.GetNewID(), false, true);
                                }
                                else if (random < 0.75f)
                                {
                                    newObject = new AbstractSpear(self.world, null, self.abstractRoom.entities[i].pos, self.game.GetNewID(), false, Mathf.Lerp(0.35f, 0.6f, Custom.ClampedRandomVariation(0.5f, 0.5f, 2f)));
                                }
                                else
                                {
                                    //ConsoleWrite("new rifle");
                                    JokeRifle.AbstractRifle.AmmoType ammo = new JokeRifle.AbstractRifle.AmmoType(ExtEnum<JokeRifle.AbstractRifle.AmmoType>.values.entries[Random.Range(0, ExtEnum<JokeRifle.AbstractRifle.AmmoType>.values.entries.Count)]);
                                    newObject = new JokeRifle.AbstractRifle(self.world, null, self.abstractRoom.entities[i].pos, self.game.GetNewID(), ammo);
                                    (newObject as JokeRifle.AbstractRifle).setCurrentAmmo((int)Random.Range(5, 40));
                                }
                            }
                            else
                            {
                                if (random < 0.25f)
                                {
                                    newObject = new AbstractSpear(self.world, null, self.abstractRoom.entities[i].pos, self.game.GetNewID(), true, false);
                                }
                                else if (random < 0.5f)
                                {
                                    newObject = new AbstractSpear(self.world, null, self.abstractRoom.entities[i].pos, self.game.GetNewID(), false, true);
                                }
                                else
                                {
                                    newObject = new AbstractSpear(self.world, null, self.abstractRoom.entities[i].pos, self.game.GetNewID(), false, false);
                                }
                            }

                            //newObject = new AbstractSpear(self.world, null, self.abstractRoom.entities[i].pos, self.game.GetNewID(), false, false);

                            if (Random.value < 0.5f) // 1/2
                            {
                                destroy = true;
                            }
                        }
                        else if (obj.type == AbstractPhysicalObject.AbstractObjectType.Rock || obj.type == AbstractPhysicalObject.AbstractObjectType.DataPearl)
                        {
                            List<RandomObject> objectsList = new List<RandomObject>
                            {
                                // TubeWorm 20
                                new RandomObject(AbstractPhysicalObject.AbstractObjectType.Rock, 20),
                                new RandomObject(AbstractPhysicalObject.AbstractObjectType.Rock, 20),
                                new RandomObject(AbstractPhysicalObject.AbstractObjectType.FlareBomb, 20),
                                new RandomObject(AbstractPhysicalObject.AbstractObjectType.Mushroom, 20),
                                new RandomObject(AbstractPhysicalObject.AbstractObjectType.JellyFish, 20),
                                new RandomObject(AbstractPhysicalObject.AbstractObjectType.PuffBall, 20),
                                new RandomObject(AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, 20),
                                // VultureGrub 10
                                new RandomObject(AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, 10),
                                new RandomObject(AbstractPhysicalObject.AbstractObjectType.VultureMask, 10),
                                new RandomObject(MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType.FireEgg, 5),
                                //new randomObject(MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType.MoonCloak, 5), // don't work
                                new RandomObject(MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType.EnergyCell, 5),
                                new RandomObject(AbstractPhysicalObject.AbstractObjectType.KarmaFlower, 5),
                                new RandomObject(MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType.SingularityBomb, 1),
                            };

                            newObject = getAbstractPhysicalObject(getRandomObject(objectsList), self, obj.pos);

                            if (Random.value < 0.6f) // 3/5
                            {
                                destroy = true;
                            }
                        }

                        if (newObject != null)
                        {
                            if (destroy)
                            {
                                ConsoleWrite($"Object {obj.type} destroyed");
                                obj.Destroy();
                                self.abstractRoom.entities[i] = newObject;
                            }
                            else
                            {
                                addObjects.Add(newObject);
                            }
                            ConsoleWrite($"Replace object {obj.type} {i} by {newObject.type}");
                            ConsoleWrite("======================\n");
                        }
                    }
                }
                self.abstractRoom.entities.AddRange(addObjects);
            }
        }

        private void ArenaGameSession_Update(On.ArenaGameSession.orig_Update orig, ArenaGameSession self)
        {
            spearsCheckCoolDown++;
            if (!self.initiated && spearRespawnTimer != null)
            {
                ConsoleWrite("Stop timer: INIT", Color.red);
                spearRespawnTimer.Dispose();
                spearRespawnTimer = null;
            }
            orig(self);

            if (Options.enableSpearsRespawn.Value && self.game.session is not SandboxGameSession && self.room != null && self.playersSpawned && spearsCheckCoolDown > 30)
            {
                spearsCheckCoolDown = 0;
                int spearCount = 0;

                //List<PhysicalObject> Spear =
                if (self.room.physicalObjects[2] != null)
                {
                    for (int i = 0; i < self.room.physicalObjects[2].Count; i++)
                    {
                        PhysicalObject obj = self.room.physicalObjects[2][i];
                        if (obj != null && obj is Spear)
                        {
                            //ConsoleWrite($"Visible spear {i} : " + (self.game.cameras[0] as RoomCamera).IsViewedByCameraPosition((self.game.cameras[0] as RoomCamera).currentCameraPosition, obj.firstChunk.pos));
                            if ((self.game.cameras[0] as RoomCamera).IsViewedByCameraPosition((self.game.cameras[0] as RoomCamera).currentCameraPosition, obj.firstChunk.pos))
                            {
                                spearCount++;
                            }
                        }
                    }
                }

                if (spearCount <= 0 && spearRespawnTimer == null)
                {
                    ConsoleWrite("Start timer", Color.green);
                    spearRespawnTimer = new Timer(x => RespawnTimerEnd(self.room), null, SecondsToMilliseconds(Options.spearRespawnTimer.Value), 0);
                }
                else if (spearCount > 0 && spearRespawnTimer != null)
                {
                    ConsoleWrite($"Stop timer: {spearCount} > 0", Color.red);
                    spearRespawnTimer.Dispose();
                    spearRespawnTimer = null;
                }

                ConsoleWrite("spearCount : " + spearCount);
            }
        }

        public void RespawnTimerEnd(Room room)
        {
            ConsoleWrite("RespawnTimerEnd", Color.green);
            if (room != null)
            {
                //ConsoleWrite($"Start processe of {room.roomSettings.placedObjects.Count} items");

                for (int i = 0; i < room.roomSettings.placedObjects.Count; i++)
                {
                    PlacedObject placedObj = room.roomSettings.placedObjects[i];
                    if (placedObj.data is PlacedObject.MultiplayerItemData && ((placedObj.data as PlacedObject.MultiplayerItemData).type == PlacedObject.MultiplayerItemData.Type.Spear || (placedObj.data as PlacedObject.MultiplayerItemData).type == PlacedObject.MultiplayerItemData.Type.ExplosiveSpear))
                    {
                        //ConsoleWrite("Spawn spear");
                        AbstractSpear spear = new AbstractSpear(room.world, null, room.GetWorldCoordinate(placedObj.pos), room.game.GetNewID(), false);
                        spear.RealizeInRoom();
                    }
                }
                ConsoleWrite("Respawn spears");

                //ConsoleWrite("Respawn Objects" + string.Join(", ", room.roomSettings.placedObjects.ConvertAll(x => x.pos)));
            }
        }

        private void ArenaGameSession_SpawnPlayers(On.ArenaGameSession.orig_SpawnPlayers orig, ArenaGameSession self, Room room, List<int> suggestedDens)
        {
            orig(self, room, suggestedDens);
            List<AbstractPhysicalObject> eggs = new();
            //ConsoleWrite("PLAYER COUNT : " + self.Players.Count);
            for (int i = 0; i < self.Players.Count; i++)
            {
                //ConsoleWrite($"Player [{i}] : {self.arenaSitting.players[i].playerClass}");
                Player player = (Player)self.Players[i].realizedCreature;
                if (player.SlugCatClass.ToString() == "Inv")
                {
                    AbstractPhysicalObject singularityBomb = new AbstractPhysicalObject(room.world, MoreSlugcatsEnums.AbstractObjectType.SingularityBomb, null, self.Players[i].pos, room.game.GetNewID());
                    eggs.Add(singularityBomb);

                    player.objectInStomach = singularityBomb;
                    player.objectInStomach.Abstractize(player.abstractCreature.pos);

                    //ConsoleWrite($"Add player{i} a egg");
                }
            }
            arenaEggs = eggs;
        }

        private void LoadResources(RainWorld rainWorld)
        {
            Futile.atlasManager.LoadAtlas("atlases/huntersprites");
        }

        private void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (self.player.slugcatStats?.name.ToString() == "Red")
            {

                FSprite fspriteFace = sLeaser.sprites[9];
                // fspriteFace.scaleX = Math.Abs(fspriteFace.scaleX);
                if (fspriteFace.element.name.StartsWith("Face"))
                {
                    bool keepFace = fspriteFace.element.name.Contains("A0") || fspriteFace.element.name.Contains("A8");

                    if (fspriteFace.scaleX > 0 || keepFace)
                    {
                        fspriteFace.scaleX = Math.Abs(fspriteFace.scaleX);
                        fspriteFace.SetElementByName("Hunter" + fspriteFace.element.name);
                    }
                }
            }
        }
        private void PlayerResultBox_ctor(On.Menu.PlayerResultBox.orig_ctor orig, PlayerResultBox self, Menu.Menu menu, MenuObject owner, Vector2 pos, Vector2 size, ArenaSitting.ArenaPlayer player, int index)
        {
            orig(self, menu, owner, pos, size, player, index);
            if (Options.enableRandomEveryRound.Value && self is FinalResultbox && RainWorldInstance.processManager.arenaSetup.playerClass[player.playerNumber] == null)
            {
                self.subObjects.Remove(self.portrait);
                self.portrait = new MenuIllustration(menu, self, "", string.Concat(new string[]
                {
                    "MultiplayerPortrait",
                    self.player.playerNumber.ToString(),
                    "2"
                }), new Vector2(size.y / 2f, size.y / 2f), true, true);
                self.subObjects.Add(self.portrait);
            }
        }

        private void ArenaSitting_SessionEnded(On.ArenaSitting.orig_SessionEnded orig, ArenaSitting self, ArenaGameSession session)
        {
            orig(self, session);
            if (Options.enableRandomEveryRound.Value)
            {
                ArenaSetup arenaSetup = session.room.game.rainWorld.processManager.arenaSetup;
                for (int i = 0; i < SlugcatCount; i++)
                {
                    if (arenaSetup.playersJoined[i] && arenaSetup.playerClass[i] == null)
                    {
                        self.players.Find(player => player.playerNumber == i).playerClass = GetRandomSlugcat();
                    }
                }
            }
        }

        private void Player_Regurgitate(On.Player.orig_Regurgitate orig, Player self)
        {
            if (self.objectInStomach == null && self.isGourmand && Options.enableGourmandCustomItem.Value && self.room.game.IsArenaSession && !IsChallengeGameSession(self.room.game))
            {
                if (Random.value < 0.67)
                {
                    self.objectInStomach = RandomStomachItem(self);
                }
                else
                {
                    self.objectInStomach = GourmandCombos.RandomStomachItem(self);
                }
            }
            orig(self);
        }

        public static AbstractPhysicalObject RandomStomachItem(PhysicalObject caller)
        {
            float value = Random.value;
            AbstractPhysicalObject abstractPhysicalObject;
            if (value <= 0.65f)
            {
                abstractPhysicalObject = new AbstractConsumable(caller.room.world, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID(), -1, -1, null);
            }
            else
            {
                abstractPhysicalObject = new AbstractConsumable(caller.room.world, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID(), -1, -1, null);
            }
            if (AbstractConsumable.IsTypeConsumable(abstractPhysicalObject.type))
            {
                (abstractPhysicalObject as AbstractConsumable).isFresh = false;
                (abstractPhysicalObject as AbstractConsumable).isConsumed = true;
            }
            return abstractPhysicalObject;
        }

        private bool Player_CanIPickThisUp(On.Player.orig_CanIPickThisUp orig, Player self, PhysicalObject obj)
        {
            if (Options.canHunterPickupStuckSpear.Value && obj is Weapon)
            {
                if ((obj as Weapon).mode == Weapon.Mode.StuckInWall && ModManager.MSC && self.SlugCatClass == SlugcatStats.Name.Red && (obj is Spear))
                {
                    return true;
                }
            }
            return orig(self, obj);
        }

        private void Player_ThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
        {
            if (self.slugcatStats.name != MoreSlugcatsEnums.SlugcatStatsName.Saint || self.room.game.IsStorySession || !Options.enableSaintSpear.Value) // self.room.game.IsStorySession
            {
                orig(self, grasp, eu);
            }
            else
            {
                //ConsoleWrite("Custom");

                self.slugcatStats.throwingSkill = 2;
                self.slugcatStats.runspeedFac = 1.3f;
                self.slugcatStats.bodyWeightFac = 1.2f;
                self.slugcatStats.generalVisibilityBonus = 0.1f;
                self.slugcatStats.visualStealthInSneakMode = 0.3f;
                self.slugcatStats.loudnessFac = 1.35f;
                self.slugcatStats.poleClimbSpeedFac = 1.35f;
                self.slugcatStats.corridorClimbSpeedFac = 1.4f;

                if (self.grasps[grasp] == null || self.grasps[grasp].grabbed is JokeRifle)
                {
                    return;
                }
                self.AerobicIncrease(0.75f);
                if (ModManager.MMF && self.room != null && MMF.cfgOldTongue.Value && self.grasps[grasp].grabbed is TubeWorm)
                {
                    (self.grasps[grasp].grabbed as TubeWorm).Use();
                    return;
                }
                if (self.grasps[grasp].grabbed is Weapon)
                {
                    IntVector2 intVector = new IntVector2(self.ThrowDirection, 0);
                    bool flag = self.input[0].y < 0;
                    if (ModManager.MMF && MMF.cfgUpwardsSpearThrow.Value)
                    {
                        flag = (self.input[0].y != 0);
                    }
                    if (self.animation == global::Player.AnimationIndex.Flip && flag && self.input[0].x == 0)
                    {
                        intVector = new IntVector2(0, (ModManager.MMF && MMF.cfgUpwardsSpearThrow.Value) ? self.input[0].y : -1);
                    }
                    if (ModManager.MMF && self.bodyMode == global::Player.BodyModeIndex.ZeroG && MMF.cfgUpwardsSpearThrow.Value)
                    {
                        int y = self.input[0].y;
                        if (y != 0)
                        {
                            intVector = new IntVector2(0, y);
                        }
                        else
                        {
                            intVector = new IntVector2(self.ThrowDirection, 0);
                        }
                    }
                    Vector2 vector = self.firstChunk.pos + intVector.ToVector2() * 10f + new Vector2(0f, 4f);
                    if (self.room.GetTile(vector).Solid)
                    {
                        vector = self.mainBodyChunk.pos;
                    }
                    if (ModManager.MSC && self.grasps[grasp].grabbed is Spear && (self.grasps[grasp].grabbed as Spear).bugSpear)
                    {
                        (self.grasps[grasp].grabbed as Weapon).Thrown(self, vector, new Vector2?(self.mainBodyChunk.pos - intVector.ToVector2() * 10f), intVector, Mathf.Lerp(1f, 1.5f, self.Adrenaline), eu);
                    }
                    else
                    {
                        // ConsoleWrite("throw");
                        (self.grasps[grasp].grabbed as Weapon).Thrown(self, vector, new Vector2?(self.mainBodyChunk.pos - intVector.ToVector2() * 10f), intVector, Mathf.Lerp(1f, 1.5f, self.Adrenaline), eu);
                    }
                    if (ModManager.MMF && MMF.cfgUpwardsSpearThrow.Value && self.grasps[grasp].grabbed is ScavengerBomb && intVector.y == 1 && self.bodyMode != global::Player.BodyModeIndex.ZeroG)
                    {
                        (self.grasps[grasp].grabbed as ScavengerBomb).doNotTumbleAtLowSpeed = true;
                        (self.grasps[grasp].grabbed as ScavengerBomb).throwModeFrames = 90;
                        (self.grasps[grasp].grabbed as ScavengerBomb).firstChunk.vel *= 0.75f;
                    }
                    if (self.grasps[grasp].grabbed is Spear)
                    {
                        // ConsoleWrite("throw spear");
                        self.ThrownSpear(self.grasps[grasp].grabbed as Spear);
                    }
                    if (self.animation == global::Player.AnimationIndex.BellySlide && self.rollCounter > 8 && self.rollCounter < 15)
                    {
                        if (intVector.x == self.rollDirection && self.slugcatStats.throwingSkill > 0)
                        {
                            BodyChunk firstChunk = self.grasps[grasp].grabbed.firstChunk;
                            firstChunk.vel.x = firstChunk.vel.x + (float)intVector.x * 15f;
                            if ((self.grasps[grasp].grabbed as Weapon).HeavyWeapon)
                            {
                                (self.grasps[grasp].grabbed as Weapon).floorBounceFrames = 30;
                                if (self.grasps[grasp].grabbed is Spear)
                                {
                                    (self.grasps[grasp].grabbed as Spear).alwaysStickInWalls = true;
                                }
                                self.grasps[grasp].grabbed.firstChunk.goThroughFloors = false;
                                BodyChunk firstChunk2 = self.grasps[grasp].grabbed.firstChunk;
                                firstChunk2.vel.y = firstChunk2.vel.y - 5f;
                            }
                            (self.grasps[grasp].grabbed as Weapon).changeDirCounter = 0;
                        }
                        else if (intVector.x == -self.rollDirection && !self.longBellySlide)
                        {
                            BodyChunk firstChunk3 = self.grasps[grasp].grabbed.firstChunk;
                            firstChunk3.vel.y = firstChunk3.vel.y + ((self.grasps[grasp].grabbed is Spear) ? 3f : 5f);
                            (self.grasps[grasp].grabbed as Weapon).changeDirCounter = 0;
                            self.rollCounter = 8;
                            BodyChunk mainBodyChunk = self.mainBodyChunk;
                            mainBodyChunk.pos.x = mainBodyChunk.pos.x + (float)self.rollDirection * 6f;
                            self.room.AddObject(new ExplosionSpikes(self.room, self.bodyChunks[1].pos + new Vector2((float)self.rollDirection * -40f, 0f), 6, 5.5f, 4f, 4.5f, 21f, new Color(1f, 1f, 1f, 0.25f)));
                            BodyChunk bodyChunk = self.bodyChunks[1];
                            bodyChunk.pos.x = bodyChunk.pos.x + (float)self.rollDirection * 6f;
                            BodyChunk bodyChunk2 = self.bodyChunks[1];
                            bodyChunk2.pos.y = bodyChunk2.pos.y + 17f;
                            BodyChunk mainBodyChunk2 = self.mainBodyChunk;
                            mainBodyChunk2.vel.x = mainBodyChunk2.vel.x + (float)self.rollDirection * 16f;
                            BodyChunk bodyChunk3 = self.bodyChunks[1];
                            bodyChunk3.vel.x = bodyChunk3.vel.x + (float)self.rollDirection * 16f;
                            self.room.PlaySound(SoundID.Slugcat_Rocket_Jump, self.mainBodyChunk, false, 1f, 1f);
                            self.exitBellySlideCounter = 0;
                            self.longBellySlide = true;
                        }
                    }
                    if (self.animation == global::Player.AnimationIndex.ClimbOnBeam && ModManager.MMF && MMF.cfgClimbingGrip.Value)
                    {
                        self.bodyChunks[0].vel += intVector.ToVector2() * 2f;
                        self.bodyChunks[1].vel -= intVector.ToVector2() * 8f;
                    }
                    else
                    {
                        self.bodyChunks[0].vel += intVector.ToVector2() * 8f;
                        self.bodyChunks[1].vel -= intVector.ToVector2() * 4f;
                    }
                    if (self.graphicsModule != null)
                    {
                        (self.graphicsModule as PlayerGraphics).ThrowObject(grasp, self.grasps[grasp].grabbed);
                    }
                    self.Blink(15);
                }
                else
                {
                    self.TossObject(grasp, eu);
                }
                self.dontGrabStuff = (self.isNPC ? 45 : 15);
                if (self.graphicsModule != null)
                {
                    (self.graphicsModule as PlayerGraphics).LookAtObject(self.grasps[grasp].grabbed);
                }
                if (self.grasps[grasp].grabbed is PlayerCarryableItem)
                {
                    (self.grasps[grasp].grabbed as PlayerCarryableItem).Forbid();
                }
                self.ReleaseGrasp(grasp);
            }
        }

        //private void MultiplayerMenu_Update(On.Menu.MultiplayerMenu.orig_Update orig, Menu.MultiplayerMenu self)
        //{
        //    if (self.exiting)
        //    {
        //        //if (ModManager.MSC && self.currentGameType != MoreSlugcatsEnums.GameTypeID.Challenge)
        //        //{
        //        //    //ConsoleWrite("Unerf Artificer");
        //        //    MoreSlugcats.MoreSlugcats.cfgArtificerExplosionCapacity.Value = 10;
        //        //}
        //    }
        //    orig(self);
        //}

        private SlugcatStats.Name MultiplayerMenu_NextClass(On.Menu.MultiplayerMenu.orig_NextClass orig, Menu.MultiplayerMenu self, SlugcatStats.Name curClass)
        {
            SlugcatStats.Name name = orig(self, curClass);
            if (IsSlucatDisabled(name) && !Options.keepSlugcatsSelectable.Value) return MultiplayerMenu_NextClass(orig, self, name);
            return name;
        }

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig.Invoke(self);
            MachineConnector.SetRegisteredOI("arenaslugcatsconfigurator", Options.instance);
            multiplayerUnlocks = new MultiplayerUnlocks(RainWorldInstance.progression, new List<string>());
        }

        private void MultiplayerMenu_InitializeSitting(On.Menu.MultiplayerMenu.orig_InitializeSitting orig, Menu.MultiplayerMenu self)
        {
            //if (Options.nerfArtificer.Value == true && MoreSlugcats.MoreSlugcats.cfgArtificerExplosionCapacity.Value != 5)
            //{
            //    // Plugin.ConsoleWrite("Nerf Artificer");
            //    MoreSlugcats.MoreSlugcats.cfgArtificerExplosionCapacity.Value = 5;
            //}
            //else if (Options.nerfArtificer.Value == false && MoreSlugcats.MoreSlugcats.cfgArtificerExplosionCapacity.Value == 5)
            //{
            //    // Plugin.ConsoleWrite("Unerf Artificer");
            //    MoreSlugcats.MoreSlugcats.cfgArtificerExplosionCapacity.Value = 10;
            //}

            Random.InitState((int)System.DateTime.Now.Ticks);
            self.manager.arenaSitting = new ArenaSitting(self.GetGameTypeSetup, self.multiplayerUnlocks);
            if (ModManager.MSC && self.currentGameType == MoreSlugcatsEnums.GameTypeID.Challenge)
            {
                self.manager.arenaSitting.AddPlayerWithClass(0, self.challengeInfo.meta.slugcatClass);
            }
            else
            {
                for (int i = 0; i < SlugcatCount; i++)
                {
                    if (self.GetArenaSetup.playersJoined[i])
                    {
                        if (ModManager.MSC)
                        {
                            if (self.GetArenaSetup.playerClass[i] == null)
                            {
                                if (i == 0)
                                {
                                    GetRandomSlugcat(true);
                                }

                                SlugcatStats.Name RandomSlugcat = GetRandomSlugcat();

                                ConsoleWrite($"player {i + 1} : {RandomSlugcat}");
                                self.manager.arenaSitting.AddPlayerWithClass(i, RandomSlugcat);
                            }
                            else
                            {
                                self.manager.arenaSitting.AddPlayerWithClass(i, self.GetArenaSetup.playerClass[i]);
                            }
                        }
                        else
                        {
                            self.manager.arenaSitting.AddPlayer(i);
                        }
                    }
                }
            }
            self.manager.arenaSitting.levelPlaylist = new List<string>();
            if (ModManager.MSC && self.GetGameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge)
            {
                self.manager.arenaSitting.levelPlaylist.Add(self.challengeInfo.meta.arena);
            }
            if (self.GetGameTypeSetup.shufflePlaylist)
            {
                List<string> list2 = new List<string>();
                for (int l = 0; l < self.GetGameTypeSetup.playList.Count; l++)
                {
                    list2.Add(self.GetGameTypeSetup.playList[l]);
                }
                while (list2.Count > 0)
                {
                    int index2 = Random.Range(0, list2.Count);
                    for (int m = 0; m < self.GetGameTypeSetup.levelRepeats; m++)
                    {
                        self.manager.arenaSitting.levelPlaylist.Add(list2[index2]);
                    }
                    list2.RemoveAt(index2);
                }
            }
            else
            {
                for (int n = 0; n < self.GetGameTypeSetup.playList.Count; n++)
                {
                    for (int num = 0; num < self.GetGameTypeSetup.levelRepeats; num++)
                    {
                        self.manager.arenaSitting.levelPlaylist.Add(self.GetGameTypeSetup.playList[n]);
                    }
                }
            }
            if (self.GetGameTypeSetup.savingAndLoadingSession && self.manager.rainWorld.options.ContainsArenaSitting())
            {
                self.manager.arenaSitting.LoadFromFile(null, null, self.manager.rainWorld);
                self.manager.arenaSitting.attempLoadInGame = true;
            }
        }

        private void RegisterCommands()
        {
            new CommandBuilder("debug_command_2")
            .Run(args =>
            {
                try
                {
                    DebugCommand(args);
                }
                catch { ConsoleWrite("Error in command", Color.red); }
            })
            .AutoComplete(new string[][] {
                new string[] { "1", "2"}
            })
            .Register();

            Log("Custom command registered");
        }

        private void DebugCommand(string[] args)
        {
            if (args.Length == 0)
            {
                ConsoleWrite("Error: invalid parameter", Color.red);
                return;
            }
            if (args[0] == "1")
            {
                ConsoleWrite("DebugCommand[1] : " + "hello world", Color.green);
            }
            else if (args[0] == "2")
            {
                ConsoleWrite("DebugCommand[2] : " + MoreSlugcats.MoreSlugcats.cfgArtificerExplosionCapacity, Color.green);
            }
        }

        private void Log(string message, string logType = "debug")
        {
            if (logType == "info")
            {
                logSource.LogInfo(message);
            }
            else if (logType == "error")
            {
                logSource.LogError(message);
            }
            else if (logType == "warning")
            {
                logSource.LogWarning(message);
            }
            else if (logType == "message")
            {
                logSource.LogMessage(message);
            }
            else if (logType == "debug")
            {
                logSource.LogDebug(message);
            }
        }

        public static void ConsoleWrite(string message, Color color)
        {
            try
            {
                GameConsoleWriteLine(message, color);
            }
            catch { }
        }
        public static void ConsoleWrite(string message = "")
        {
            try
            {
                GameConsoleWriteLine(message, Color.white);
            }
            catch { }
        }

        private static void GameConsoleWriteLine(string message, Color color)
        {
            DevConsole.GameConsole.WriteLine(message, color);
            Debug.Log(message);
        }

        private void ShowMessage(RainWorldGame game, string message, int wait = 0, int time = 50, bool darken = false)
        {
            game.cameras[0].hud.textPrompt.AddMessage(message, wait, time, darken, true);
        }

        public static MiscProgressionData ProgressionData
        {
            get => FindObjectOfType<RainWorld>().progression.miscProgressionData;
        }

        public static RainWorld RainWorldInstance
        {
            get => FindObjectOfType<RainWorld>();
        }

        public static bool IsSlucatDisabled(SlugcatStats.Name slugcat)
        {
            if (Options.GetModdedSlugcats().ConvertAll(x => x.nameObject).Contains(slugcat))
            {
                return Options.SlugcatObject.slugcats.Find(x => x.codeName == slugcat.value).configurable.Value;
            }
            else if (slugcat == SlugcatStats.Name.Yellow)
            {
                return Options.disableMonk.Value;
            }
            else if (slugcat == SlugcatStats.Name.White)
            {
                return Options.disableSurvivor.Value;
            }
            else if (slugcat == SlugcatStats.Name.Red)
            {
                return Options.disableHunter.Value;
            }
            else if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
            {
                return Options.disableRivulet.Value;
            }
            else if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
            {
                return Options.disableArtificer.Value;
            }
            else if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                return Options.disableSaint.Value;
            }
            else if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Spear)
            {
                return Options.disableSpearmaster.Value;
            }
            else if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
            {
                return Options.disableGourmand.Value;
            }
            else
            {
                return false;
            }
        }

        public static List<SlugcatStats.Name> GetSlugcatsList()
        {
            List<SlugcatStats.Name> list = new();

            foreach (var slugcat in Options.GetModdedSlugcats())
            {
                if (!slugcat.configurable.Value)
                {
                    list.Add(slugcat.nameObject);
                }
            }
            if (!Options.disableSurvivor.Value)
            {
                list.Add(SlugcatStats.Name.White);
            }
            if (!Options.disableMonk.Value)
            {
                list.Add(SlugcatStats.Name.Yellow);
            }
            if (!Options.disableHunter.Value && Options.IsSlugcatUnlocked(SlugcatStats.Name.Red))
            {
                list.Add(SlugcatStats.Name.Red);
            }
            if (!Options.disableRivulet.Value && Options.IsSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Rivulet))
            {
                list.Add(MoreSlugcatsEnums.SlugcatStatsName.Rivulet);
            }
            if (!Options.disableArtificer.Value && Options.IsSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Artificer))
            {
                list.Add(MoreSlugcatsEnums.SlugcatStatsName.Artificer);
            }
            if (!Options.disableSaint.Value && Options.IsSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Saint))
            {
                list.Add(MoreSlugcatsEnums.SlugcatStatsName.Saint);
            }
            if (!Options.disableSpearmaster.Value && Options.IsSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Spear))
            {
                list.Add(MoreSlugcatsEnums.SlugcatStatsName.Spear);
            }
            if (!Options.disableGourmand.Value && Options.IsSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Gourmand))
            {
                list.Add(MoreSlugcatsEnums.SlugcatStatsName.Gourmand);
            }
            if (list.Count < 1)
            {
                list.Add(SlugcatStats.Name.White);
                list.Add(SlugcatStats.Name.Yellow);
            }

            if (Mathf.Round(Random.Range(0, 40 / Mathf.Ceil(list.Count / 2))) == 0)
            {
                list.Add(MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel);
            }


            return list;
        }

        public static SlugcatStats.Name GetRandomSlugcat(bool showList = false)
        {
            List<SlugcatStats.Name> list = new();

            foreach (var slugcat in Options.GetModdedSlugcats())
            {
                if (!slugcat.configurable.Value)
                {
                    list.Add(slugcat.nameObject);
                }
            }
            if (!Options.disableSurvivor.Value)
            {
                list.Add(SlugcatStats.Name.White);
            }
            if (!Options.disableMonk.Value)
            {
                list.Add(SlugcatStats.Name.Yellow);
            }
            if (!Options.disableHunter.Value && Options.IsSlugcatUnlocked(SlugcatStats.Name.Red))
            {
                list.Add(SlugcatStats.Name.Red);
            }
            if (!Options.disableRivulet.Value && Options.IsSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Rivulet))
            {
                list.Add(MoreSlugcatsEnums.SlugcatStatsName.Rivulet);
            }
            if (!Options.disableArtificer.Value && Options.IsSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Artificer))
            {
                list.Add(MoreSlugcatsEnums.SlugcatStatsName.Artificer);
            }
            if (!Options.disableSaint.Value && Options.IsSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Saint))
            {
                list.Add(MoreSlugcatsEnums.SlugcatStatsName.Saint);
            }
            if (!Options.disableSpearmaster.Value && Options.IsSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Spear))
            {
                list.Add(MoreSlugcatsEnums.SlugcatStatsName.Spear);
            }
            if (!Options.disableGourmand.Value && Options.IsSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Gourmand))
            {
                list.Add(MoreSlugcatsEnums.SlugcatStatsName.Gourmand);
            }
            if (list.Count < 1)
            {
                list.Add(SlugcatStats.Name.White);
                list.Add(SlugcatStats.Name.Yellow);
            }

            if (Mathf.Round(Random.Range(0, 40 / Mathf.Ceil(list.Count / 2))) == 0)
            {
                list.Add(MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel);
            }

            if (showList)
            {
                ConsoleWrite("List: " + string.Join(",", list.ConvertAll(x => x.value)), Color.white);
                return null;
            }

            return list[Random.Range(0, list.Count)];
        }
        public static int SecondsToMilliseconds(int seconds)
        {
            int milliseconds = seconds * 1000;
            return milliseconds;
        }

        public static bool IsChallengeGameSession(RainWorldGame game)
        {
            if (game.IsArenaSession && ModManager.MSC && game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge)
            {
                return true;
            }
            return false;
        }
    }
}