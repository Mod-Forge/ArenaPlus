using ArenaPlus.Lib;
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
    internal class HunterPickupStuckSpears(SlugcatFeatureInfoAttribute featureInfo) : SlugcatFeature(featureInfo)
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
            if (obj is Weapon weapon)
            {
                if (weapon.mode == Weapon.Mode.StuckInWall && ModManager.MSC && self.SlugCatClass == SlugcatStats.Name.Red && obj is Spear)
                {
                    return true;
                }
            }
            return orig(self, obj);
        }
    }
}
