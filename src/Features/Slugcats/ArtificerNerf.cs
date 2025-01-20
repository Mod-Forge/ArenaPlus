using ArenaPlus.Lib;
using ArenaPlus.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArenaPlus.Features.Slugcats
{
    [SlugcatFeatureInfo(
        id: "artificerNerf",
        name: "Artificer nerf",
        description: "Whether the Artificer is nerfed in arena",
        slugcat: "Artificer"
    )]
    file class ArtificerNerf(SlugcatFeatureInfoAttribute featureInfo) : SlugcatFeature(featureInfo)
    {
        protected override void Register()
        {
            On.Player.ClassMechanicsArtificer += Player_ClassMechanicsArtificer;
        }

        protected override void Unregister()
        {
            On.Player.ClassMechanicsArtificer -= Player_ClassMechanicsArtificer;
        }

        private void Player_ClassMechanicsArtificer(On.Player.orig_ClassMechanicsArtificer orig, Player self)
        {
            float parryCooldown = self.pyroParryCooldown;
            orig(self);
            if (self.pyroParryCooldown > parryCooldown)
            {
                if (ModManager.MSC && GameUtils.IsCompetitiveOrSandboxSession)
                {
                    ConsoleWrite("nerf Artificer");
                    self.pyroJumpCounter += 5;
                    self.Stun(60 * (self.pyroJumpCounter - (Mathf.Max(1, MoreSlugcats.MoreSlugcats.cfgArtificerExplosionCapacity.Value - 3) - 1)));
                    //orig(self);
                }
            }
        }
    }
}