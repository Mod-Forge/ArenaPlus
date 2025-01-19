using ArenaPlus.Lib;
using ArenaPlus.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArenaPlus.Features.UI
{
    [FeatureInfo(
        id: "keepSlugcatsSelectable",
        name: "Keep slugcats selectable",
        description: "Whether disabled slugcats are disabled only in random (not in the select menu)",
        enabledByDefault: false
    )]
    internal class KeepSlugcatsSelectable(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
    {
        protected override void Register()
        {
        }

        protected override void Unregister()
        {
        }
    }
}
