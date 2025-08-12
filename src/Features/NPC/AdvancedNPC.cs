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

namespace ArenaPlus.Features.NPC
{
    [FeatureInfo(
        id: nameof(AdvancedNPC),
        name: "Advanced NPC",
        category: BuiltInCategory.General,
        description: "Allow arena NPC (CPU) to jump",
        enabledByDefault: true
    )]
    internal class AdvancedNPC(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
    {
        protected override void Unregister()
        {
        }

        protected override void Register()
        {
        }

    }
}
