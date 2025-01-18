using ArenaPlus.Options;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArenaPlus.Utils
{
    internal static class SlugcatsUtils
    {
        public static List<SlugcatObject> GetSlugcats()
        {
            List<string> exceptions = [
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
                .Where(slugcat => IsSlugcatUnlocked(slugcat.nameObject))
                .ToList();
        }

        public static List<SlugcatObject> GetModdedSlugcats()
        {
            List<string> vanillaSlugcats = [
                "White",
                "Yellow",
                "Red",
                "Rivulet",
                "Artificer",
                "Saint",
                "Spear",
                "Gourmand"
            ];
            return GetSlugcats().Where(slugcat => !vanillaSlugcats.Contains(slugcat.name)).ToList();
        }

        public static List<SlugcatStats.Name> GetActiveSlugcats()
        {
            List<SlugcatStats.Name> list = [];

            foreach (var slugcat in GetSlugcats())
            {
                if (!slugcat.configurable.Value)
                {
                    list.Add(slugcat.nameObject);
                }
            }
            //if (!Options.disableSurvivor.Value)
            //{
            //    list.Add(SlugcatStats.Name.White);
            //}
            //if (!Options.disableMonk.Value)
            //{
            //    list.Add(SlugcatStats.Name.Yellow);
            //}
            //if (!Options.disableHunter.Value && Options.IsSlugcatUnlocked(SlugcatStats.Name.Red))
            //{
            //    list.Add(SlugcatStats.Name.Red);
            //}
            //if (!Options.disableRivulet.Value && Options.IsSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Rivulet))
            //{
            //    list.Add(MoreSlugcatsEnums.SlugcatStatsName.Rivulet);
            //}
            //if (!Options.disableArtificer.Value && Options.IsSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Artificer))
            //{
            //    list.Add(MoreSlugcatsEnums.SlugcatStatsName.Artificer);
            //}
            //if (!Options.disableSaint.Value && Options.IsSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Saint))
            //{
            //    list.Add(MoreSlugcatsEnums.SlugcatStatsName.Saint);
            //}
            //if (!Options.disableSpearmaster.Value && Options.IsSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Spear))
            //{
            //    list.Add(MoreSlugcatsEnums.SlugcatStatsName.Spear);
            //}
            //if (!Options.disableGourmand.Value && Options.IsSlugcatUnlocked(MoreSlugcatsEnums.SlugcatStatsName.Gourmand))
            //{
            //    list.Add(MoreSlugcatsEnums.SlugcatStatsName.Gourmand);
            //}
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
            List<SlugcatStats.Name> list = GetActiveSlugcats();

            if (showList)
            {
                ConsoleWrite("List: " + string.Join(",", list.ConvertAll(x => x.value)), Color.white);
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
            if (id == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
            {
                return GameUtils.ProgressionData.GetTokenCollected(MultiplayerUnlocks.SlugcatUnlockID.Gourmand);
            }
            if (id == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
            {
                return GameUtils.ProgressionData.GetTokenCollected(MultiplayerUnlocks.SlugcatUnlockID.Rivulet);
            }
            if (id == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
            {
                return GameUtils.ProgressionData.GetTokenCollected(MultiplayerUnlocks.SlugcatUnlockID.Artificer);
            }
            if (id == MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                return GameUtils.ProgressionData.GetTokenCollected(MultiplayerUnlocks.SlugcatUnlockID.Saint);
            }
            return !(id == MoreSlugcatsEnums.SlugcatStatsName.Spear) || GameUtils.ProgressionData.GetTokenCollected(MultiplayerUnlocks.SlugcatUnlockID.Spearmaster);
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
            this.configurable = OptionsInterface.instance.config.Bind($"enable{this.codeName}", true, new ConfigurableInfo($"Whether the {this.name} appears in arena", null, "", []));
            slugcats.Add(this);
        }
    }
}
