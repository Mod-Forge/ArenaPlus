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
        id: "rifleMonkey",
        name: "Rifle Monkes (WIP)",
        category: BuiltInCategory.Spoilers,
        description: "Arm scavengers with joke rifles (Challenge spoiler)",
        incompatibilities: ["objectsRandomizer"],
        enabledByDefault: false
    )]
    public class RifleMonkey(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
    {
        protected override void Unregister()
        {
        }
        protected override void Register()
        {
        }
    }
}
