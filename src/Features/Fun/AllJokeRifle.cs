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
        id: "allJokeRifle",
        name: "All Joke Rifle (WIP)",
        category: BuiltInCategory.Spoilers,
        description: "Replacer every spears with Joke Rifles",
        incompatible: ["objectsRandomizer"],
        enabledByDefault: false
    )]
    file class AllJokeRifle(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
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
