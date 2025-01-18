﻿using Menu.Remix.MixedUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ArenaPlus.Utils;
using ArenaPlus.Lib;

namespace ArenaPlus.Options.Tabs
{
    internal class SlugcatsTab : OpTab
    {
        private static readonly Vector2 checkBoxSpace = new(0f, 20f + 10f);
        private static readonly Vector2 labelSpace = new(40f, 0f);
        private static readonly Vector2 initialPos = new Vector2(20f, 405f) - checkBoxSpace;

        public SlugcatsTab(OptionInterface owner) : base(owner, "Slugcats")
        {
            var slugcats = SlugcatsUtils.GetSlugcats();

            int featureOffset = 24;
            int height = 475 - 20;
            int contentSize = (slugcats.Count + FeaturesManager.slugcatFeatures.Count) * 30 + 24;
            int index = 0;

            OpScrollBox moddedScrollBox = new(initialPos - new Vector2(0, height - 100), new Vector2(560, height), contentSize, false, false);

            AddItems(moddedScrollBox, new OpRect(initialPos - new Vector2(0, height - 100) - new Vector2(0, 5), new Vector2(560, height + 10)));


            foreach (var slugcat in slugcats)
            {
                OpCheckBox checkbox = new(slugcat.configurable, new Vector2(20f, 0f) + new Vector2(0, Mathf.Max(height, contentSize) - 40 - index * checkBoxSpace.y));
                checkbox.colorEdge = slugcat.color;
                checkbox.description = slugcat.configurable.info.description;
                OpLabel label = new(checkbox.pos + labelSpace, default, $"Enable {slugcat.name}", FLabelAlignment.Left, false, null);
                label.color = slugcat.color;

                index++;

                SlugcatFeature[] features = FeaturesManager.slugcatFeatures.Where(feature => feature.Slugcat == slugcat.name).ToArray();

                foreach (var feature in features)
                {
                    OpCheckBox featureCheckbox = new(feature.configurable, new Vector2(20f + featureOffset, 0f) + new Vector2(0, Mathf.Max(height, contentSize) - 40 - index * checkBoxSpace.y));
                    featureCheckbox.colorEdge = slugcat.color;
                    featureCheckbox.description = feature.configurable.info.description;
                    OpLabel featureLabel = new(featureCheckbox.pos + labelSpace, default, feature.Name, FLabelAlignment.Left, false, null);
                    featureLabel.color = slugcat.color;

                    moddedScrollBox.AddItems(featureCheckbox, featureLabel);
                    index++;
                }

                moddedScrollBox.AddItems(checkbox, label);
            }
        }
    }
}
