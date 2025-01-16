using Menu.Remix.MixedUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ArenaPlus.Utils;

namespace ArenaPlus.Options.Tabs
{
    internal class ModdedSlugcatsTab : OpTab
    {
        private static readonly Vector2 checkBoxSpace = new(0f, 20f + 10f);
        private static readonly Vector2 labelSpace = new(40f, 0f);
        private static readonly Vector2 initialPos = new Vector2(20f, 405f) - checkBoxSpace;

        public ModdedSlugcatsTab(OptionInterface owner) : base(owner, "Modded")
        {
            int height = 475 - 20;
            int contentSize = Feature.features.Count * 30 + 24;
            int index = 0;

            OpScrollBox moddedScrollBox = new(initialPos - new Vector2(0, height - 100), new Vector2(560, height), contentSize, false, false);

            AddItems(moddedScrollBox, new OpRect(initialPos - new Vector2(0, height - 100) - new Vector2(0, 5), new Vector2(560, height + 10)));

            var slugcats = SlugcatsUtils.GetModdedSlugcats();

            foreach (var slugcat in slugcats)
            {
                OpCheckBox checkbox = new(slugcat.configurable, new Vector2(20f, 0f) + new Vector2(0, Mathf.Max(height, contentSize) - 40 - index * checkBoxSpace.y));
                checkbox.colorEdge = slugcat.color;
                checkbox.description = slugcat.configurable.info.description;
                OpLabel label = new(checkbox.pos + labelSpace, default(Vector2), $"Disable {slugcat.name}", FLabelAlignment.Left, false, null);
                label.color = slugcat.color;
                index++;

                moddedScrollBox.AddItems(checkbox, label);
            }
        }
    }
}
