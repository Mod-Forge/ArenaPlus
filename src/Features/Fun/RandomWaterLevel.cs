﻿using ArenaPlus.Utils;
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
        id: "randomWaterLevel",
        name: "Random water level (WIP)",
        category: BuiltInCategory.Fun,
        description: "Add water to every levels with a random initial height",
        enabledByDefault: false
    )]
    file class RandomWaterLevel(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
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
