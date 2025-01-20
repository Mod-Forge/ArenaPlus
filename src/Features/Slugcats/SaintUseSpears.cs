using ArenaPlus.Lib;
using ArenaPlus.Utils;
using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArenaPlus.Features.Slugcats
{
    [SlugcatFeatureInfo(
        id: "saintSpear",
        name: "Saint can use spears",
        description: "Whether the Saint can use spears",
        slugcat: "Saint"
    )]
    file class SaintUseSpears(SlugcatFeatureInfoAttribute featureInfo) : SlugcatFeature(featureInfo)
    {
        protected override void Register()
        {
            On.Player.TossObject += Player_TossObject;
        }

        protected override void Unregister()
        {
            On.Player.TossObject -= Player_TossObject;
        }

        private void Player_TossObject(On.Player.orig_TossObject orig, Player self, int grasp, bool eu)
        {
            if (GameUtils.IsCompetitiveOrSandboxSession && self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint && self.grasps[grasp] != null && self.grasps[grasp].grabbed is Spear)
            {
                IntVector2 intVector = new IntVector2(self.ThrowDirection, 0);
                if (self.animation == Player.AnimationIndex.Flip && (self.input[0].y < 0 || (MMF.cfgUpwardsSpearThrow.Value && self.input[0].y > 0)))
                {
                    intVector = new IntVector2(0, MMF.cfgUpwardsSpearThrow.Value ? self.input[0].y : -1);
                }
                else
                {
                    intVector = new IntVector2(self.ThrowDirection, 0);
                }


                self.slugcatStats.throwingSkill = 2;
                (self.grasps[grasp].grabbed as Weapon).Thrown(self, self.firstChunk.pos + intVector.ToVector2() * 10f + new Vector2(0f, 4f), new Vector2?(self.mainBodyChunk.pos - intVector.ToVector2() * 10f), intVector, Mathf.Lerp(1f, 1.5f, self.Adrenaline), eu);
                return;
            }
            orig(self, grasp, eu);
        }
    }
}