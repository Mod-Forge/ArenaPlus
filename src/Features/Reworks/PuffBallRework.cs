using ArenaPlus.Lib;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArenaPlus.Features.Reworks
{
    [FeatureInfo(
        id: "puffBallRework",
        name: "Puff ball stun",
        category: BuiltInCategory.Reworks,
        description: "Whether puff balls stun players",
        enabledByDefault: true
    )]
    file class PuffBallRework(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
    {
        protected override void Register()
        {
            On.PuffBall.Explode += PuffBall_Explode;
        }

        protected override void Unregister()
        {
            On.PuffBall.Explode -= PuffBall_Explode;
        }

        private void PuffBall_Explode(On.PuffBall.orig_Explode orig, PuffBall self)
        {
            if (self.slatedForDeletetion)
            {
                orig(self);
                return;
            }

            for (int i = 0; i < self.room.abstractRoom.creatures.Count; i++)
            {
                if (self.room.abstractRoom.creatures[i].realizedCreature != null)
                {
                    if (self.room.abstractRoom.creatures[i].realizedCreature.Template.type == CreatureTemplate.Type.Slugcat && Custom.DistLess(self.firstChunk.pos, self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos, (1f + 20f) * 3))
                    {
                        int stun = Mathf.RoundToInt(20f * Random.value * 3f / Mathf.Lerp(self.room.abstractRoom.creatures[i].realizedCreature.TotalMass, 1f, 0.15f));
                        self.room.abstractRoom.creatures[i].realizedCreature.Stun(stun);
                    }
                }
            }

            orig(self);
        }
    }
}
