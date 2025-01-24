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
        id: "rockRain",
        name: "Rock rain (WIP)",
        category: BuiltInCategory.Fun,
        description: "Make rock rain instead of water",
        enabledByDefault: false
    )]
    file class RockRain(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
    {
        protected override void Unregister()
        {
            throw new NotImplementedException();
        }

        protected override void Register()
        {
            On.ArenaGameSession.ctor += ArenaGameSession_ctor;
        }

        private void ArenaGameSession_ctor(On.ArenaGameSession.orig_ctor orig, ArenaGameSession self, RainWorldGame game)
        {
            orig(self, game);
            if (self is not SandboxGameSession || self.arenaSitting.sandboxPlayMode)
            {
                self.AddBehavior(new RockRainBehavior(self));
            }
        }
    }

    file class RockRainBehavior : ArenaBehaviors.ArenaGameBehavior
    {
        public RockRainBehavior(ArenaGameSession gameSession) : base(gameSession)
        {
        }
    }
}
