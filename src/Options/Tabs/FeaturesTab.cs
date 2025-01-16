using Menu.Remix.MixedUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArenaPlus.Utils;
using UnityEngine;

namespace ArenaPlus.Options.Tabs
{
    internal class FeaturesTab : OpTab
    {
        private static readonly Vector2 checkBoxSpace = new(0f, 20f + 10f);
        private static readonly Vector2 labelSpace = new(40f, 3f);
        private static readonly Vector2 initialPos = new Vector2(20f, 405f) - checkBoxSpace;

        public FeaturesTab(OptionInterface owner) : base(owner, "Features")
        {
            int height = 475 - 20;
            int contentSize = Feature.features.Count * 30 + 24;
            int index = 0;

            OpScrollBox moddedScrollBox = new(initialPos - new Vector2(0, height - 100), new Vector2(560, height), contentSize, false, false);

            AddItems(moddedScrollBox, new OpRect(initialPos - new Vector2(0, height - 100) - new Vector2(0, 5), new Vector2(560, height + 10)));

            foreach (var feature in Feature.features)
            {
                OpCheckBox checkbox = new(feature.configurable, new Vector2(20f, 0f) + new Vector2(0, Mathf.Max(height, contentSize) - 40 - index * checkBoxSpace.y));
                checkbox.description = feature.configurable.info.description;
                OpLabel label = new(checkbox.pos + labelSpace, default, feature.Name, FLabelAlignment.Left, false, null);
                index++;

                moddedScrollBox.AddItems(checkbox, label);
            }
        }
    }
}
