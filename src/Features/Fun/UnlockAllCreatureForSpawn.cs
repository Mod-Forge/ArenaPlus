using ArenaPlus.Utils;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MoreSlugcats;
using ArenaPlus.Lib;

namespace ArenaPlus.Features.Fun
{
    [FeatureInfo(
        id: "unlockAllCreatureForSpawn",
        name: "Unlock all creatures spawn (Cheat)",
        category: BuiltInCategory.Spoilers,
        description: "Unlock all creatures spawn in arena (Spoiler for evrything)",
        color: "FF0000",
        enabledByDefault: false
    )]
    internal class UnlockAllCreatureForSpawn(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
    {
        protected override void Unregister()
        {
            On.MultiplayerUnlocks.IsCreatureUnlockedForLevelSpawn -= MultiplayerUnlocks_IsCreatureUnlockedForLevelSpawn;
        }

        protected override void Register()
        {
            On.MultiplayerUnlocks.IsCreatureUnlockedForLevelSpawn += MultiplayerUnlocks_IsCreatureUnlockedForLevelSpawn;
        }

        private bool MultiplayerUnlocks_IsCreatureUnlockedForLevelSpawn(On.MultiplayerUnlocks.orig_IsCreatureUnlockedForLevelSpawn orig, MultiplayerUnlocks self, CreatureTemplate.Type tp)
        {
            orig(self, tp);
            return true;
        }
    }
}
