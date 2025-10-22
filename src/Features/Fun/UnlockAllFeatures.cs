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
        id: "unlockAllFeatures",
        name: "Unlock all locked features (Cheat)",
        category: BuiltInCategory.Spoilers,
        description: "Unlock all locked features (Require reloading the remix menu)",
        color: "FF0000",
        enabledByDefault: false
    )]
    internal class UnlockAllFeatures(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
    {
        protected override void Unregister()
        {
        }
        protected override void Register()
        {
        }
    }
}
