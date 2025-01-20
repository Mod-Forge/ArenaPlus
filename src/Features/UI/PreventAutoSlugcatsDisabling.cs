using ArenaPlus.Lib;
using Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaPlus.Features.UI
{
    [ImmutableFeature]
    file class PreventAutoSlugcatsDisabling : ImmutableFeature
    {
        protected override void Register()
        {
            On.RainWorld.DeactivateAllPlayers += RainWorld_DeactivateAllPlayers; ;
        }

        private void RainWorld_DeactivateAllPlayers(On.RainWorld.orig_DeactivateAllPlayers orig, RainWorld self)
        {
            if (self.processManager.currentMainLoop is MultiplayerMenu) return;

            orig(self);
        }
    }
}
