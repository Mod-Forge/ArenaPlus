using ArenaPlus.Utils;
using Menu.Remix.MixedUI;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArenaPlus.Options.Tabs;
using ArenaPlus.Options.Elements;

namespace ArenaPlus.Options
{
    internal class OptionsInterface : OptionInterface
    {
        public override void Initialize()
        {
            base.Initialize();

            Tabs = [
               new FeaturesTab(this),
               new SlugcatsTab(this)
            ];

            foreach (var tab in Tabs)
            {
                OpLabel modTitle = new(new Vector2(20f, 520f), new Vector2(560f, 30f), "Arena+", FLabelAlignment.Center, true, null);

                tab.AddItems(modTitle);
            }
        }

        public override void Update()
        {
            base.Update();
            foreach (OpCustomTab tab in Tabs.Where(tab => tab is OpCustomTab).Select(tab => tab as OpCustomTab))
            {
                tab.Update();
            }
        }

        public static OptionsInterface instance = new();
    }
}