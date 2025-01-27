using ArenaPlus.Lib;
using ArenaPlus.Utils;
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
        id: "firecrackerPlantRework",
        name: "Cherrybomb rework",
        category: BuiltInCategory.Reworks,
        description: "Make Cherrybombs stun",
        enabledByDefault: true
    )]
    file class FirecrackerPlantRework(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
    {
        private const float MAX_STUN = 120f;
        protected override void Unregister()
        {
            On.FirecrackerPlant.PopLump -= FirecrackerPlant_PopLump;
        }
        protected override void Register()
        {
            On.FirecrackerPlant.PopLump += FirecrackerPlant_PopLump;
        }

        private void FirecrackerPlant_PopLump(On.FirecrackerPlant.orig_PopLump orig, FirecrackerPlant self, int lmp)
        {
            orig(self, lmp);
            if (GameUtils.IsCompetitiveOrSandboxSession)
            {
                for (int l = 0; l < self.room.abstractRoom.creatures.Count; l++)
                {
                    if (self.room.abstractRoom.creatures[l].realizedCreature != null && self.room.abstractRoom.creatures[l].realizedCreature.room == self.room && !self.room.abstractRoom.creatures[l].realizedCreature.dead)
                    {
                        self.room.abstractRoom.creatures[l].realizedCreature.Stun((int)Custom.LerpMap(Vector2.Distance(self.lumps[lmp].pos, self.room.abstractRoom.creatures[l].realizedCreature.mainBodyChunk.pos), 40f, 80f, MAX_STUN, 0f));
                    }
                }
            }
        }
    }
}
