using ArenaPlus.Lib;
using ArenaPlus.Options;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Watcher;

namespace ArenaPlus.Utils
{
    internal static class SlugcatsUtils
    {
        internal static SlugcatStats.Name _overrideName;
        public static Player RecreatePlayerWithClass(Player player, SlugcatStats.Name newClass)
        {
            _overrideName = newClass;
            player.abstractCreature.realizedCreature?.Abstractize();
            player.abstractCreature.realizedCreature?.room?.RemoveObject(player.abstractCreature.realizedCreature);
            player.abstractCreature.realizedObject = null;
            player.abstractCreature.RealizeInRoom();
            return player.abstractCreature.realizedCreature as Player;
        }

        public static IntVector2 ThrowDirection(this Player player)
        {
            IntVector2 intVector = new IntVector2(player.ThrowDirection, 0);
            bool flag = player.input[0].y < 0;
            if (ModManager.MMF && MoreSlugcats.MMF.cfgUpwardsSpearThrow.Value)
            {
                flag = player.input[0].y != 0;
            }
            if (player.animation == Player.AnimationIndex.Flip && flag && player.input[0].x == 0)
            {
                intVector = new IntVector2(0, (ModManager.MMF && MoreSlugcats.MMF.cfgUpwardsSpearThrow.Value) ? player.input[0].y : (-1));
            }
            if (ModManager.MMF && player.bodyMode == Player.BodyModeIndex.ZeroG && MoreSlugcats.MMF.cfgUpwardsSpearThrow.Value)
            {
                int y = player.input[0].y;
                if (y != 0)
                {
                    intVector = new IntVector2(0, y);
                }
                else
                {
                    intVector = new IntVector2(player.ThrowDirection, 0);
                }
            }
            return intVector;
        }

        public static SlugcatObject[] GetSlugcats()
        {
            string[] exceptions = [
                "Night",
                "Slugpup",
                "Inv",
                "JollyPlayer1",
                "JollyPlayer2",
                "JollyPlayer3",
                "JollyPlayer4"
            ];

            return ExtEnumBase.GetNames(typeof(SlugcatStats.Name))
                .Where(name => !exceptions.Contains(name))
                .ToList()
                .ConvertAll(name => SlugcatObject.slugcats.Find(slugcat => slugcat.codeName == name) ?? new SlugcatObject(name))
                .ToArray();
        }

        public static SlugcatObject[] GetModdedSlugcats()
        {
            string[] vanillaSlugcats = [
                "White",
                "Yellow",
                "Red",
                "Rivulet",
                "Artificer",
                "Saint",
                "Spear",
                "Gourmand"
            ];
            return GetSlugcats().Where(slugcat => !vanillaSlugcats.Contains(slugcat.name)).ToArray();
        }

        public static SlugcatObject[] GetUnlockedSlugcats()
        {
            return GetSlugcats().Where(slugcat => IsSlugcatUnlocked(slugcat.nameObject)).ToArray();
        }

        public static bool IsSlugcatEnabled(SlugcatStats.Name id)
        {
            foreach (var slugcat in GetSlugcats())
            {
                if (slugcat.nameObject.value == id.value) return slugcat.configurable.Value;
            }

            return true;
        }

        public static List<SlugcatStats.Name> GetActiveSlugcats()
        {
            List<SlugcatStats.Name> list = [];

            foreach (var slugcat in GetUnlockedSlugcats())
            {
                if (slugcat.configurable.Value)
                {
                    list.Add(slugcat.nameObject);
                }
            }

            if (list.Count < 1)
            {
                list.Add(SlugcatStats.Name.White);
                list.Add(SlugcatStats.Name.Yellow);
            }

            if (ModManager.MSC && Mathf.Round(Random.Range(0, 40 / Mathf.Ceil(list.Count / 2))) == 0)
            {
                list.Add(MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel);
            }

            return list;
        }

        public static SlugcatStats.Name GetRandomSlugcat(bool showList = false)
        {
            List<SlugcatStats.Name> list = GetActiveSlugcats();

            if (showList)
            {
                LogDebug("List: " + string.Join(",", list.ConvertAll(x => x.value)));
                return null;
            }

            return list[Random.Range(0, list.Count)];
        }

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
                return GameUtils.ProgressionData.redUnlocked;
            }

            if (ModManager.MSC)
            {
                if (id == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
                {
                    return GameUtils.ProgressionData.GetTokenCollected(MultiplayerUnlocks.SlugcatUnlockID.Gourmand);
                }
                if (id == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
                {
                    return GameUtils.ProgressionData.GetTokenCollected(MultiplayerUnlocks.SlugcatUnlockID.Rivulet);
                }
                if (id == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Artificer)
                {
                    return GameUtils.ProgressionData.GetTokenCollected(MultiplayerUnlocks.SlugcatUnlockID.Artificer);
                }
                if (id == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Saint)
                {
                    return GameUtils.ProgressionData.GetTokenCollected(MultiplayerUnlocks.SlugcatUnlockID.Saint);
                }
                if (id == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Spear)
                {
                    return GameUtils.ProgressionData.GetTokenCollected(MultiplayerUnlocks.SlugcatUnlockID.Spearmaster);
                }
            }

            if (ModManager.Watcher)
            {
                //if (id == Watcher.WatcherEnums.SlugcatStatsName.Watcher)
                //{
                //    return SlugcatStats.SlugcatUnlocked(id, GameUtils.RainWorldInstance);
                //}
            }

            return SlugcatStats.SlugcatUnlocked(id, GameUtils.RainWorldInstance);
        }
    }
    public class SlugcatObject
    {
        public static List<SlugcatObject> slugcats = [];

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
            this.configurable = OptionsInterface.instance.config.Bind($"enable_{GetValidSlugcatName()}", true, new ConfigurableInfo($"Whether the {this.name} appears in arena", null, "", []));

            if (name == "Watcher")
                color = new Color(0.22f, 0.192f, 0.929f);


            slugcats.Add(this);
        }

        public string GetValidSlugcatName()
        {
            return GetValidSlugcatName(codeName);
        }

        public static string GetValidSlugcatName(string slugcatName)
        {
            return slugcatName.Replace(' ', '_');
        }
    }
    internal static class SlugcatsUtilsHooks
    {
        [HookRegister]
        private static void Register()
        {
            On.Player.GetInitialSlugcatClass += Player_GetInitialSlugcatClass;
        }

        private static void Player_GetInitialSlugcatClass(On.Player.orig_GetInitialSlugcatClass orig, Player self)
        {
            orig(self);
            if (SlugcatsUtils._overrideName != null)
            {
                self.SlugCatClass = SlugcatsUtils._overrideName;
                SlugcatsUtils._overrideName = null;

            }

        }
    }
}
