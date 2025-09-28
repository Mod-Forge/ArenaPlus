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
using static ArenaPlus.Lib.PlayerAttachedFeature;

namespace ArenaPlus.Features.Reworks
{
    [FeatureInfo(
        id: "karmaFlowerRippleRework",
        name: "Watcher karma flower (Spoiler) (WIP)",
        category: BuiltInCategory.Spoilers,
        description: "The Watcher allow beings from the ripples to feast on your enemies (Watcher spoiler)",
        requireDLC: [DLCIdentifiers.Watcher],
        enabledByDefault: false
    )]
    file class KarmaFlowerRippleRework(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
    {
        protected override void Unregister()
        {
        }

        protected override void Register()
        {
        }
    }
}
