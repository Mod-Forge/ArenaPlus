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
    [ImmutableFeature]
    file class RandomEnabledSlugcatAtStart : ImmutableFeature
    {
        private ArenaSetup arenaSetup;

        protected override void Register()
        {
            On.ArenaSitting.AddPlayerWithClass += ArenaSitting_AddPlayerWithClass;
            On.Menu.MultiplayerMenu.InitializeSitting += MultiplayerMenu_InitializeSitting;
        }

        private void MultiplayerMenu_InitializeSitting(On.Menu.MultiplayerMenu.orig_InitializeSitting orig, MultiplayerMenu self)
        {
            RandomSlugcatEveryRound.randomSlugcat = SlugcatsUtils.GetRandomSlugcat();
            arenaSetup = self.GetArenaSetup;
            orig(self);
            arenaSetup = null;
        }

        private void ArenaSitting_AddPlayerWithClass(On.ArenaSitting.orig_AddPlayerWithClass orig, ArenaSitting self, int playerNumber, SlugcatStats.Name playerClass)
        {
            if (self.gameTypeSetup.challengeMeta == null && arenaSetup.playerClass[playerNumber] == null)
            {
                playerClass = FeaturesManager.GetFeature("randomSlugcatEveryone").configurable.Value ? RandomSlugcatEveryRound.randomSlugcat : SlugcatsUtils.GetRandomSlugcat();
            }

            orig(self, playerNumber, playerClass);
        }
    }
}
