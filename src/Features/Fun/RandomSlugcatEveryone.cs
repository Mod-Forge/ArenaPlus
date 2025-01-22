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
        id: "randomSlugcatEveryone",
        name: "Random Slugcat Everyone",
        category: BuiltInCategory.Fun,
        description: "Make \"random slugcat every round\" give the same random slugcat to everyone",
        require: ["randomSlugcatEveryRound"],
        enabledByDefault: false
    )]
    file class RandomSlugcatEveryone(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
    {
        protected override void Register()
        {
        }

        protected override void Unregister()
        {
        }
    }
}
