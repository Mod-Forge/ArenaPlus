using ArenaPlus.Lib;
using ArenaPlus.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArenaPlus.Features
{
    [FeatureInfo(
        id: "arenaMultiplicationRandomizer",
        name: "Random multi-arenas",
        description: "Randomize the arenas list when multiply by 2x to 5x",
        enabledByDefault: false
    )]
    file class ArenaMultiplicationRandomizer(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
    {
        protected override void Unregister()
        {
            On.Menu.MultiplayerMenu.InitializeSitting -= MultiplayerMenu_InitializeSitting;
        }

        protected override void Register()
        {
            On.Menu.MultiplayerMenu.InitializeSitting += MultiplayerMenu_InitializeSitting;
        }

        private void MultiplayerMenu_InitializeSitting(On.Menu.MultiplayerMenu.orig_InitializeSitting orig, Menu.MultiplayerMenu self)
        {
            orig(self);
            if (self.GetGameTypeSetup.shufflePlaylist && self.GetGameTypeSetup.levelRepeats > 1)
            {
                self.manager.arenaSitting.levelPlaylist.Shuffle();
            }
        }
    }
}
