using ArenaPlus.Utils;
using Menu.Remix.MixedUI;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArenaPlus.Options.Tabs;

namespace ArenaPlus.Options
{
    internal class OptionsInterface : OptionInterface
    {
        public override void Initialize()
        {
            base.Initialize();

            List<OpTab> tabs = [
                new FeaturesTab(this)
            ];

            if (SlugcatsUtils.GetModdedSlugcats().Count > 0)
            {
                tabs.Add(new ModdedSlugcatsTab(this));
            }

            foreach (var tab in tabs)
            {
                OpLabel modTitle = new(new Vector2(20f, 520f), new Vector2(560f, 30f), "Arena+", FLabelAlignment.Center, true, null);

                tab.AddItems(modTitle);
            }
            
            Tabs = [.. tabs];
        }

        public static OptionsInterface instance = new();
    }
}