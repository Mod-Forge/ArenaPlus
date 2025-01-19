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
    [ImmutableFeature]
    file class SlugcatMenuSkip : ImmutableFeature
    {
        protected override void Register()
        {
            On.Menu.MultiplayerMenu.NextClass += MultiplayerMenu_NextClass;
        }

        private SlugcatStats.Name MultiplayerMenu_NextClass(On.Menu.MultiplayerMenu.orig_NextClass orig, Menu.MultiplayerMenu self, SlugcatStats.Name curClass)
        {
            SlugcatStats.Name name = orig(self, curClass);
            if (name != null && Input.GetKey(KeyCode.LeftControl)) return null;
            if (name != null && !FeaturesManager.GetFeature("keepSlugcatsSelectable").Enabled && !SlugcatsUtils.IsSlugcatEnabled(name)) return MultiplayerMenu_NextClass(orig, self, name);
            return name;
        }
    }
}
