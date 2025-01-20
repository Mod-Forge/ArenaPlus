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

namespace ArenaPlus.Features.Reworks
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
        protected override void Register()
        {
            throw new NotImplementedException();
        }

        protected override void Unregister()
        {
            throw new NotImplementedException();
        }
    }
}
