using ArenaPlus.Lib;
using ArenaPlus.Utils;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArenaPlus.Features.Slugcats
{
    [SlugcatFeatureInfo(
        id: "watcherPowers",
        name: "Watcher powers (WIP)",
        description: "Give the watcher some powers (NightCat spoiler) (do nothing for now).",
        slugcat: "Watcher"
    )]
    file class SpearMasterCustomSpears(SlugcatFeatureInfoAttribute featureInfo) : SlugcatFeature(featureInfo)
    {
        protected override void Unregister()
        {
        }
        protected override void Register()
        {
        }

    }
}