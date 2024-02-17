using MoreSlugcats;
using UnityEngine;
using Menu.Remix.MixedUI;
using System.Collections.Generic;
using System.Linq;

namespace ArenaSlugcatsConfigurator
{
    public class Options : OptionInterface
    {
        public override void Initialize()
        {
            base.Initialize();

            Vector2 checkBoxSpace = new Vector2(0f, 20f + 10f);
            Vector2 labelSpace = new Vector2(40f, 0f);
            Vector2 initialPos = new Vector2(20f, 405f) - checkBoxSpace;
            int vanillaButtonNum = 0;
            int moddedButtonNum = 0;
            int saintPos = 0;
            int artificerPos = 0;
            int hunterPos = 0;
            int gourmandPos = 0;

            Tabs = new OpTab[2];
            Tabs[0] = new OpTab(this, "Vanilla");
            OpLabel vanillaTitle = new OpLabel(new Vector2(20f, 520f), new Vector2(560f, 30f), "Arena Slugcats Configurator", FLabelAlignment.Center, true, null);
            Tabs[0].AddItems(vanillaTitle);

            if (GetModdedSlugcats().Count > 0)
            {
                Tabs[1] = new OpTab(this, "Modded");
                OpLabel moddedTitle = new OpLabel(new Vector2(20f, 520f), new Vector2(560f, 30f), "Arena Slugcats Configurator", FLabelAlignment.Center, true, null);
                Tabs[1].AddItems(moddedTitle);
            }

            OpCheckBox chkKeepSlugcatsSelectable = new OpCheckBox(Options.keepSlugcatsSelectable, initialPos + checkBoxSpace * 1.3f + checkBoxSpace);
            chkKeepSlugcatsSelectable.colorEdge = Color.grey;
            chkKeepSlugcatsSelectable.description = keepSlugcatsSelectable.info.description;
            OpLabel lbKeepSlugcatsSelectable = new OpLabel(chkKeepSlugcatsSelectable.pos + labelSpace, default(Vector2), "Keep slugcats selectable", FLabelAlignment.Left, false, null);
            lbKeepSlugcatsSelectable.color = chkKeepSlugcatsSelectable.colorEdge;

            Tabs[0].AddItems(chkKeepSlugcatsSelectable, lbKeepSlugcatsSelectable);

            OpCheckBox clkEnableRandomEveryRounds = new OpCheckBox(Options.enableRandomEveryRound, initialPos + checkBoxSpace * 1.3f);
            clkEnableRandomEveryRounds.colorEdge = Color.grey;
            clkEnableRandomEveryRounds.description = enableRandomEveryRound.info.description;
            OpLabel lbEnableRandomEveryRounds = new OpLabel(clkEnableRandomEveryRounds.pos + labelSpace, default(Vector2), "Random slugcat every rounds", FLabelAlignment.Left, false, null);
            lbEnableRandomEveryRounds.color = clkEnableRandomEveryRounds.colorEdge;

            Tabs[0].AddItems(clkEnableRandomEveryRounds, lbEnableRandomEveryRounds);

            OpCheckBox clkEnableMaskBlock = new OpCheckBox(Options.enableMaskBlock, initialPos + checkBoxSpace * 1.3f - checkBoxSpace);
            clkEnableMaskBlock.colorEdge = Color.grey;
            clkEnableMaskBlock.description = enableMaskBlock.info.description;
            OpLabel lbEnableMaskBlock = new OpLabel(clkEnableMaskBlock.pos + labelSpace, default(Vector2), "Masks blocking", FLabelAlignment.Left, false, null);
            lbEnableMaskBlock.color = clkEnableMaskBlock.colorEdge;

            Tabs[0].AddItems(clkEnableMaskBlock, lbEnableMaskBlock);

            OpCheckBox clkEnableRandomObjects = new OpCheckBox(Options.enableRandomObjects, initialPos + checkBoxSpace * 1.3f - checkBoxSpace * 2f);
            clkEnableRandomObjects.colorEdge = Color.grey;
            clkEnableRandomObjects.description = enableRandomObjects.info.description;
            OpLabel lbEnableRandomObjects = new OpLabel(clkEnableRandomObjects.pos + labelSpace, default(Vector2), "Random objects (SPOILER!)", FLabelAlignment.Left, false, null);
            lbEnableRandomObjects.color = clkEnableRandomObjects.colorEdge;

            Tabs[0].AddItems(clkEnableRandomObjects, lbEnableRandomObjects);

            OpCheckBox clkEnableSpearsRespawn = new OpCheckBox(Options.enableSpearsRespawn, initialPos + checkBoxSpace * 1.3f - checkBoxSpace * 3f);
            clkEnableSpearsRespawn.colorEdge = Color.grey;
            clkEnableSpearsRespawn.description = enableSpearsRespawn.info.description;
            OpLabel lbEnableSpearsRespawn = new OpLabel(clkEnableSpearsRespawn.pos + labelSpace, default(Vector2), "Spears respawn", FLabelAlignment.Left, false, null);
            lbEnableSpearsRespawn.color = clkEnableSpearsRespawn.colorEdge;

            Tabs[0].AddItems(clkEnableSpearsRespawn, lbEnableSpearsRespawn);

            OpUpdown udSpearRespawnTimer = new OpUpdown(Options.spearRespawnTimer, initialPos + checkBoxSpace * 1.3f - checkBoxSpace * 3f + new Vector2(180f, -5f), 60f)
            {
                colorEdge = Color.grey,
                description = spearRespawnTimer.info.description
            };
            OpLabel lbSpearRespawnTimer = new OpLabel(udSpearRespawnTimer.pos + labelSpace * 1.75f + new Vector2(0f, 5f), default(Vector2), "Spears respawn time", FLabelAlignment.Left, false, null);
            lbSpearRespawnTimer.color = udSpearRespawnTimer.colorEdge;

            Tabs[0].AddItems(udSpearRespawnTimer, lbSpearRespawnTimer);

            vanillaButtonNum += 3;

            OpCheckBox chkDisableSurvivor = new OpCheckBox(Options.disableSurvivor, initialPos - checkBoxSpace * vanillaButtonNum);
            chkDisableSurvivor.colorEdge = PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.White);
            chkDisableSurvivor.description = disableSurvivor.info.description;
            OpLabel lbDisableSurvivor = new OpLabel(chkDisableSurvivor.pos + labelSpace, default(Vector2), "Disable Survivor", FLabelAlignment.Left, false, null);
            lbDisableSurvivor.color = chkDisableSurvivor.colorEdge;

            Tabs[0].AddItems(chkDisableSurvivor, lbDisableSurvivor);
            vanillaButtonNum++;


            OpCheckBox chkDisableMonk = new OpCheckBox(Options.disableMonk, initialPos - checkBoxSpace * vanillaButtonNum);
            chkDisableMonk.colorEdge = PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.Yellow);
            chkDisableMonk.description = disableMonk.info.description;
            OpLabel lbDisableMonk = new OpLabel(chkDisableMonk.pos + labelSpace, default(Vector2), "Disable Monk", FLabelAlignment.Left, false, null);
            lbDisableMonk.color = chkDisableMonk.colorEdge;

            Tabs[0].AddItems(chkDisableMonk, lbDisableMonk);
            vanillaButtonNum++;

            if (IsSlugcatUnlocked(SlugcatStats.Name.Red))
            {
                OpCheckBox chkDisableHunter = new OpCheckBox(Options.disableHunter, initialPos - checkBoxSpace * vanillaButtonNum);
                chkDisableHunter.colorEdge = PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.Red);
                chkDisableHunter.description = disableHunter.info.description;
                OpLabel lbDisableHunter = new OpLabel(chkDisableHunter.pos + labelSpace, default(Vector2), "Disable Hunter", FLabelAlignment.Left, false, null);
                lbDisableHunter.color = chkDisableHunter.colorEdge;

                Tabs[0].AddItems(chkDisableHunter, lbDisableHunter);
                hunterPos = vanillaButtonNum;
                vanillaButtonNum++;
            }

            if (IsSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Rivulet))
            {
                OpCheckBox chkDisableRivulet = new OpCheckBox(Options.disableRivulet, initialPos - checkBoxSpace * vanillaButtonNum);
                chkDisableRivulet.colorEdge = PlayerGraphics.DefaultSlugcatColor(MoreSlugcatsEnums.SlugcatStatsName.Rivulet);
                chkDisableRivulet.description = disableRivulet.info.description;
                OpLabel lbDisableRivulet = new OpLabel(chkDisableRivulet.pos + labelSpace, default(Vector2), "Disable Rivulet", FLabelAlignment.Left, false, null);
                lbDisableRivulet.color = chkDisableRivulet.colorEdge;

                Tabs[0].AddItems(chkDisableRivulet, lbDisableRivulet);
                vanillaButtonNum++;
            }

            if (IsSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Artificer))
            {
                OpCheckBox chkDisableArtificer = new OpCheckBox(Options.disableArtificer, initialPos - checkBoxSpace * vanillaButtonNum);
                chkDisableArtificer.colorEdge = PlayerGraphics.DefaultSlugcatColor(MoreSlugcatsEnums.SlugcatStatsName.Artificer);
                chkDisableArtificer.description = disableArtificer.info.description;
                OpLabel lbDisableArtificer = new OpLabel(chkDisableArtificer.pos + labelSpace, default(Vector2), "Disable Artificer", FLabelAlignment.Left, false, null);
                lbDisableArtificer.color = chkDisableArtificer.colorEdge;

                Tabs[0].AddItems(chkDisableArtificer, lbDisableArtificer);
                artificerPos = vanillaButtonNum;
                vanillaButtonNum++;
            }

            if (IsSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Saint))
            {
                OpCheckBox chkDisableSaint = new OpCheckBox(Options.disableSaint, initialPos - checkBoxSpace * vanillaButtonNum);
                chkDisableSaint.colorEdge = PlayerGraphics.DefaultSlugcatColor(MoreSlugcatsEnums.SlugcatStatsName.Saint);
                chkDisableSaint.description = disableSaint.info.description;
                OpLabel lbDisableSaint = new OpLabel(chkDisableSaint.pos + labelSpace, default(Vector2), "Disable Saint", FLabelAlignment.Left, false, null);
                lbDisableSaint.color = chkDisableSaint.colorEdge;

                Tabs[0].AddItems(chkDisableSaint, lbDisableSaint);
                saintPos = vanillaButtonNum;
                vanillaButtonNum++;
            }

            if (IsSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Spear))
            {
                OpCheckBox chkDisableSpearmaster = new OpCheckBox(Options.disableSpearmaster, initialPos - checkBoxSpace * vanillaButtonNum);
                chkDisableSpearmaster.colorEdge = PlayerGraphics.DefaultSlugcatColor(MoreSlugcatsEnums.SlugcatStatsName.Spear);
                chkDisableSpearmaster.description = disableSpearmaster.info.description;
                OpLabel lbDisableSpearmaster = new OpLabel(chkDisableSpearmaster.pos + labelSpace, default(Vector2), "Disable Spearmaster", FLabelAlignment.Left, false, null);
                lbDisableSpearmaster.color = chkDisableSpearmaster.colorEdge;

                Tabs[0].AddItems(chkDisableSpearmaster, lbDisableSpearmaster);
                vanillaButtonNum++;
            }

            if (IsSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Gourmand))
            {
                OpCheckBox chkDisableGourmand = new OpCheckBox(Options.disableGourmand, initialPos - checkBoxSpace * vanillaButtonNum);
                chkDisableGourmand.colorEdge = PlayerGraphics.DefaultSlugcatColor(MoreSlugcatsEnums.SlugcatStatsName.Gourmand);
                chkDisableGourmand.description = disableGourmand.info.description;
                OpLabel lbDisableGourmand = new OpLabel(chkDisableGourmand.pos + labelSpace, default(Vector2), "Disable Gourmand", FLabelAlignment.Left, false, null);
                lbDisableGourmand.color = chkDisableGourmand.colorEdge;

                Tabs[0].AddItems(chkDisableGourmand, lbDisableGourmand);
                gourmandPos = vanillaButtonNum;
                vanillaButtonNum++;
            }


            //special
            if (IsSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Artificer))
            {
                OpCheckBox chkNerfArtificer = new OpCheckBox(Options.nerfArtificer, initialPos - checkBoxSpace * artificerPos + new Vector2(180f, 0f));
                chkNerfArtificer.colorEdge = PlayerGraphics.DefaultSlugcatColor(MoreSlugcatsEnums.SlugcatStatsName.Artificer);
                chkNerfArtificer.description = nerfArtificer.info.description;
                OpLabel lbNerfArtificer = new OpLabel(chkNerfArtificer.pos + labelSpace, default(Vector2), "Enable Artificer nerf", FLabelAlignment.Left, false, null);
                lbNerfArtificer.color = chkNerfArtificer.colorEdge;

                Tabs[0].AddItems(chkNerfArtificer, lbNerfArtificer);
            }

            if (IsSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Saint))
            {
                OpCheckBox chkEnableSaintSpear = new OpCheckBox(Options.enableSaintSpear, initialPos - checkBoxSpace * saintPos + new Vector2(180f, 0f));
                chkEnableSaintSpear.colorEdge = PlayerGraphics.DefaultSlugcatColor(MoreSlugcatsEnums.SlugcatStatsName.Saint);
                chkEnableSaintSpear.description = enableSaintSpear.info.description;
                OpLabel lbEnableSaintSpear = new OpLabel(chkEnableSaintSpear.pos + labelSpace, default(Vector2), "Enable Saint spear", FLabelAlignment.Left, false, null);
                lbEnableSaintSpear.color = chkEnableSaintSpear.colorEdge;

                Tabs[0].AddItems(chkEnableSaintSpear, lbEnableSaintSpear);
            }

            if (IsSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Gourmand))
            {
                OpCheckBox chkCustomGourmandItem = new OpCheckBox(Options.enableGourmandCustomItem, initialPos - checkBoxSpace * gourmandPos + new Vector2(180f, 0f));
                chkCustomGourmandItem.colorEdge = PlayerGraphics.DefaultSlugcatColor(MoreSlugcatsEnums.SlugcatStatsName.Gourmand);
                chkCustomGourmandItem.description = enableGourmandCustomItem.info.description;
                OpLabel lbCustomGourmandItem = new OpLabel(chkCustomGourmandItem.pos + labelSpace, default(Vector2), "Enable Gourmand better item generation", FLabelAlignment.Left, false, null);
                lbCustomGourmandItem.color = chkCustomGourmandItem.colorEdge;

                Tabs[0].AddItems(chkCustomGourmandItem, lbCustomGourmandItem);
            }


            if (IsSlugcatUnlocked(SlugcatStats.Name.Red))
            {
                OpCheckBox chkHunterCanPickupStuckSpear = new OpCheckBox(Options.canHunterPickupStuckSpear, initialPos - checkBoxSpace * hunterPos + new Vector2(180f, 0f));
                chkHunterCanPickupStuckSpear.colorEdge = PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.Red);
                chkHunterCanPickupStuckSpear.description = canHunterPickupStuckSpear.info.description;
                OpLabel lbHunterCanPickupStuckSpear = new OpLabel(chkHunterCanPickupStuckSpear.pos + labelSpace, default(Vector2), "Enable Hunter to pickup stuck spear", FLabelAlignment.Left, false, null);
                lbHunterCanPickupStuckSpear.color = chkHunterCanPickupStuckSpear.colorEdge;

                Tabs[0].AddItems(chkHunterCanPickupStuckSpear, lbHunterCanPickupStuckSpear);
            }

            var slugcats = GetModdedSlugcats();

            if (slugcats.Count > 0)
            {
                int height = 475 - 20;

                int contentSize = slugcats.Count * 30 + 24;

                OpScrollBox moddedScrollBox = new(initialPos - new Vector2(0, height - 100), new Vector2(560, height), contentSize, false, false);

                Tabs[1].AddItems(moddedScrollBox, new OpRect(initialPos - new Vector2(0, height - 100) - new Vector2(0, 5), new Vector2(560, height + 10)));

                foreach (var slugcat in slugcats)
                {
                    OpCheckBox checkbox = new OpCheckBox(slugcat.configurable, new Vector2(20f, 0f) + new Vector2(0, Mathf.Max(height, contentSize) - 40 - moddedButtonNum * checkBoxSpace.y));
                    checkbox.colorEdge = slugcat.color;
                    checkbox.description = slugcat.configurable.info.description;
                    OpLabel label = new OpLabel(checkbox.pos + labelSpace, default(Vector2), $"Disable {slugcat.name}", FLabelAlignment.Left, false, null);
                    label.color = slugcat.color;
                    moddedButtonNum++;

                    moddedScrollBox.AddItems(checkbox, label);
                }
            }
        }

        public static List<SlugcatObject> GetModdedSlugcats()
        {
            List<string> vanillaSlugcats = new()
            {
                "White",
                "Yellow",
                "Red",
                "Night",
                "Rivulet",
                "Artificer",
                "Saint",
                "Spear",
                "Gourmand",
                "Slugpup",
                "Inv",
                "JollyPlayer1",
                "JollyPlayer2",
                "JollyPlayer3",
                "JollyPlayer4"
            };
            return ExtEnumBase.GetNames(typeof(SlugcatStats.Name)).ToList().Where(name => !vanillaSlugcats.Contains(name)).ToList().ConvertAll(name => SlugcatObject.slugcats.Find(slugcat => slugcat.codeName == name) ?? new SlugcatObject(name));
        }

        public class SlugcatObject
        {
            public static List<SlugcatObject> slugcats = new();

            public string codeName;
            public SlugcatStats.Name nameObject;
            public string name;
            public Color color;
            public Configurable<bool> configurable;

            public SlugcatObject(string name)
            {
                this.codeName = name;
                this.nameObject = new SlugcatStats.Name(name, register: false);
                this.name = SlugcatStats.getSlugcatName(nameObject);
                this.color = PlayerGraphics.DefaultSlugcatColor(nameObject);
                this.configurable = instance.config.Bind($"disable{this.codeName}", false, new ConfigurableInfo($"Whether the {this.name} appears in arena", null, "", new object[]
                {
                    $"{this.name} disabled?"
                }));
                slugcats.Add(this);
            }
        }

        public static Options instance = new();

        public static Configurable<bool> keepSlugcatsSelectable = Options.instance.config.Bind<bool>("keepSlugcatsSelectable", false, new ConfigurableInfo("Whether disabled slugcats are disabled only in random (not in the select menu)", null, "", new object[]
        {
            "Keep slugcats selectable?"
        }));

        public static Configurable<bool> enableRandomEveryRound = Options.instance.config.Bind<bool>("enableRandomEveryRound", false, new ConfigurableInfo("Whether the random slugcat is random every rounds", null, "", new object[]
        {
            "Keep slugcats selectable?"
        }));

        public static Configurable<bool> enableMaskBlock = Options.instance.config.Bind<bool>("enableMaskBlock", true, new ConfigurableInfo("Whether masks block projectiles before breaking", null, "", new object[]
        {
            "Enable masks block?"
        }));

        public static Configurable<bool> enableRandomObjects = Options.instance.config.Bind<bool>("enableRandomObjects", false, new ConfigurableInfo("Whether objects in arena are random", null, "", new object[]
        {
            "Enable random objects?"
        }));

        public static Configurable<bool> enableSpearsRespawn = Options.instance.config.Bind<bool>("enableSpearsRespawn", false, new ConfigurableInfo("Whether spears reappear when they are all lost", null, "", new object[]
        {
            "Enable spears respawn?"
        }));

        public static Configurable<int> spearRespawnTimer = Options.instance.config.Bind<int>("spearRespawnTimer", 30, new ConfigurableInfo("The time in seconds before the spears respawn", new ConfigAcceptableRange<int>(0, 100), "", new object[]
        {
            "Spears respawn timer"
        }));

        public static Configurable<bool> nerfArtificer = Options.instance.config.Bind<bool>("nerfArtificer", false, new ConfigurableInfo("Whether the Artificer is nerfed in arena", null, "", new object[]
        {
            "Artificer nerf?"
        }));

        public static Configurable<bool> enableSaintSpear = Options.instance.config.Bind<bool>("enableSaintSpear", false, new ConfigurableInfo("Whether the Saint can use spear", null, "", new object[]
        {
            "Saint spear?"
        }));

        public static Configurable<bool> canHunterPickupStuckSpear = Options.instance.config.Bind<bool>("canHunterPickupStuckSpear", false, new ConfigurableInfo("Whether the Hunter can pickup a stuck spear", null, "", new object[]
        {
            "Can hunter pickup spear in wall?"
        }));

        public static Configurable<bool> enableGourmandCustomItem = Options.instance.config.Bind<bool>("enableGourmandCustomItem", false, new ConfigurableInfo("Whether the Gourmand generate better item for crafting in arena", null, "", new object[]
        {
            "Can the gourmand generate custom item??"
        }));


        public static Configurable<bool> disableMonk = Options.instance.config.Bind<bool>("disableMonk", false, new ConfigurableInfo("Whether the Monk appears in arena", null, "", new object[]
        {
            "Monk disable?"
        }));

        public static Configurable<bool> disableSurvivor = Options.instance.config.Bind<bool>("disableSurvivor", false, new ConfigurableInfo("Whether the Survivor appears in arena", null, "", new object[]
        {
            "Survivor disable?"
        }));

        public static Configurable<bool> disableHunter = Options.instance.config.Bind<bool>("disableHunter", false, new ConfigurableInfo("Whether the Hunter appears in arena", null, "", new object[]
        {
            "Hunter disable?"
        }));

        public static Configurable<bool> disableRivulet = Options.instance.config.Bind<bool>("disableRivulet", false, new ConfigurableInfo("Whether the Rivulet appears in arena", null, "", new object[]
        {
            "Rivulet disable?"
        }));

        public static Configurable<bool> disableArtificer = Options.instance.config.Bind<bool>("disableArtificer", false, new ConfigurableInfo("Whether the Artificer appears in arena", null, "", new object[]
        {
            "Artificer disable?"
        }));

        public static Configurable<bool> disableSaint = Options.instance.config.Bind<bool>("disableSaint", false, new ConfigurableInfo("Whether the Saint appears in arena", null, "", new object[]
        {
            "Saint disable?"
        }));

        public static Configurable<bool> disableSpearmaster = Options.instance.config.Bind<bool>("disableSpearmaster", false, new ConfigurableInfo("Whether the Spearmaster appears in arena", null, "", new object[]
        {
            "Spearmaster disable?"
        }));

        public static Configurable<bool> disableGourmand = Options.instance.config.Bind<bool>("disableGourmand", false, new ConfigurableInfo("Whether the Gourmand appears in arena", null, "", new object[]
        {
            "Gourmand disable?"
        }));

        public static bool IsSlugcatUnlocked(SlugcatStats.Name id)
        {
            if (SlugcatStats.HiddenOrUnplayableSlugcat(id))
            {
                return false;
            }
            if (ModManager.MSC && MoreSlugcats.MoreSlugcats.chtUnlockClasses.Value)
            {
                return true;
            }
            if (id == SlugcatStats.Name.Red)
            {
                return Plugin.ProgressionData.redUnlocked;
            }
            if (id == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
            {
                return Plugin.ProgressionData.GetTokenCollected(MultiplayerUnlocks.SlugcatUnlockID.Gourmand);
            }
            if (id == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
            {
                return Plugin.ProgressionData.GetTokenCollected(MultiplayerUnlocks.SlugcatUnlockID.Rivulet);
            }
            if (id == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
            {
                return Plugin.ProgressionData.GetTokenCollected(MultiplayerUnlocks.SlugcatUnlockID.Artificer);
            }
            if (id == MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                return Plugin.ProgressionData.GetTokenCollected(MultiplayerUnlocks.SlugcatUnlockID.Saint);
            }
            return !(id == MoreSlugcatsEnums.SlugcatStatsName.Spear) || Plugin.ProgressionData.GetTokenCollected(MultiplayerUnlocks.SlugcatUnlockID.Spearmaster);
        }

        //public override void Update()
        //{
        //    Debug.Log(Options.nerfArtificer.Value.ToString()+", "+ MoreSlugcats.MoreSlugcats.cfgArtificerExplosionCapacity.Value.ToString());
        //    base.Update();
        //    if (Options.nerfArtificer.Value == true && MoreSlugcats.MoreSlugcats.cfgArtificerExplosionCapacity.Value != 5)
        //    {
        //        Plugin.ConsoleWrite("Nerf Artificer");
        //        MoreSlugcats.MoreSlugcats.cfgArtificerExplosionCapacity.Value = 5;
        //    }
        //    else if (Options.nerfArtificer.Value == false && MoreSlugcats.MoreSlugcats.cfgArtificerExplosionCapacity.Value == 5)
        //    {
        //        Plugin.ConsoleWrite("Unerf Artificer");
        //        MoreSlugcats.MoreSlugcats.cfgArtificerExplosionCapacity.Value = 10;
        //    }
        //}
    }
}
