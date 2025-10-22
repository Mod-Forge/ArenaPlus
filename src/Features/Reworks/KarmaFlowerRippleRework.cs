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
        public override bool IsLocked(out string reason)
        {
            reason = $"Require: Beten Watcher echo quest";
            return !GameUtils.RainWorldInstance.progression.miscProgressionData.beaten_Watcher_SpinningTop;
        }

        protected override void Unregister()
        {
        }

        protected override void Register()
        {
            //GameUtils.RainWorldInstance.progression.miscProgressionData.beaten_Watcher_SpinningTop
        }
    }
}
