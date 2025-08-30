using ArenaPlus.Lib;
using ArenaPlus.Options;
using ArenaPlus.Options.Tabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArenaPlus.Features.NPC;

[FeatureInfo(
    id: "NormalNPC",
    name: "Normal CPU",
    description: "Make CPU stop using artificer powers",
    category: BuiltInCategory.Fun,
    enabledByDefault: false
)]
internal class NormalNPC(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
{
    protected override void Register()
    {
    }

    protected override void Unregister()
    {
    }
}