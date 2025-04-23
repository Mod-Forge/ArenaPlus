using ArenaPlus.Lib;
using ArenaPlus.Utils;
using Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaPlus.Features
{
    // Remove if the watcher get in arena
    //[ImmutableFeature]
    file class SelectableWatcher : ImmutableFeature
    {
        internal bool WatcherUnlocked => GameUtils.ProgressionData.beaten_Watcher_SpinningTop;
        protected override void Register()
        {
            On.SlugcatStats.SlugcatUnlocked += SlugcatStats_SlugcatUnlocked;
            On.MultiplayerUnlocks.ClassUnlocked += MultiplayerUnlocks_ClassUnlocked;
        }

        private bool MultiplayerUnlocks_ClassUnlocked(On.MultiplayerUnlocks.orig_ClassUnlocked orig, MultiplayerUnlocks self, SlugcatStats.Name classID)
        {
            if (ModManager.Watcher && classID == Watcher.WatcherEnums.SlugcatStatsName.Watcher)
            {
                return WatcherUnlocked;
            }
            return orig(self, classID);
        }

        private bool SlugcatStats_SlugcatUnlocked(On.SlugcatStats.orig_SlugcatUnlocked orig, SlugcatStats.Name i, RainWorld rainWorld)
        {
            if (ModManager.Watcher && i == Watcher.WatcherEnums.SlugcatStatsName.Watcher)
            {
                return WatcherUnlocked;
            }

            return orig(i, rainWorld);
        }
    }
}
