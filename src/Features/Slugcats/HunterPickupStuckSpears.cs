using ArenaPlus.Lib;
using ArenaPlus.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaPlus.Features.Slugcats
{
    [SlugcatFeatureInfo(
        id: "canHunterPickupStuckSpear",
        name: "Hunter can pickup stuck spears",
        description: "Whether the Hunter can pickup a stuck spear",
        slugcat: "Hunter"
    )]
    file class HunterPickupStuckSpears(SlugcatFeatureInfoAttribute featureInfo) : SlugcatFeature(featureInfo)
    {
        protected override void Register()
        {
            On.Player.CanIPickThisUp += Player_CanIPickThisUp;
        }

        protected override void Unregister()
        {
            On.Player.CanIPickThisUp -= Player_CanIPickThisUp;
        }

        private bool Player_CanIPickThisUp(On.Player.orig_CanIPickThisUp orig, Player self, PhysicalObject obj)
        {
            if (GameUtils.IsCompetitiveOrSandboxSession && obj is Spear spear)
            {
                if (spear.mode == Weapon.Mode.StuckInWall && ModManager.MSC && self.SlugCatClass == SlugcatStats.Name.Red)
                {
                    return true;
                }
            }
            return orig(self, obj);
        }
    }
}
