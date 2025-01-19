using Menu.Remix.MixedUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ArenaPlus.Utils;
using ArenaPlus.Lib;
using ArenaPlus.Options.Elements;

namespace ArenaPlus.Options.Tabs
{
    internal class SlugcatsTab : OpCustomTab
    {
        private const int HEIGHT = 475 - 20;
        private static readonly Vector2 checkBoxSpace = new(0f, 20f + 10f);
        private static readonly Vector2 labelSpace = new(40f, 0f);
        private static readonly Vector2 initialPos = new Vector2(20f, 405f) - checkBoxSpace;

        private readonly OpScrollBox scrollBox;

        public SlugcatsTab(OptionInterface owner) : base(owner, "Slugcats")
        {
            var slugcats = SlugcatsUtils.GetUnlockedSlugcats();

            int contentSize = (slugcats.Count() + FeaturesManager.slugcatFeatures.Count) * 30 + 24;

            scrollBox = new(initialPos - new Vector2(0, HEIGHT - 100), new Vector2(560, HEIGHT), contentSize, false, false);

            AddItems(scrollBox, new OpRect(initialPos - new Vector2(0, HEIGHT - 100) - new Vector2(0, 5), new Vector2(560, HEIGHT + 10)));

            OnPreActivate += DrawItems;
            OnPreUnload += SlugcatsTab_OnPreUnload;
        }

        private void SlugcatsTab_OnPreUnload()
        {
            OnPreActivate -= DrawItems;
            OnPreUnload -= SlugcatsTab_OnPreUnload;
        }

        private void DrawItems()
        {
            foreach (var item in scrollBox.items)
            {
                item._RemoveFromScrollBox();
                RemoveItems(item);
                item.Unload();
            }

            int featureOffset = 24;
            int index = 0;

            var slugcats = SlugcatsUtils.GetUnlockedSlugcats();

            foreach (var slugcat in slugcats)
            {
                OpCheckBox checkbox = new(slugcat.configurable, new Vector2(20f, 0f) + new Vector2(0, Mathf.Max(HEIGHT, scrollBox.contentSize) - 40 - index * checkBoxSpace.y));
                checkbox.colorEdge = slugcat.color;
                checkbox.description = slugcat.configurable.info.description;
                OpLabel label = new(checkbox.pos + labelSpace, default, $"Enable {slugcat.name}", FLabelAlignment.Left, false, null);
                label.color = slugcat.color;

                index++;

                SlugcatFeature[] features = FeaturesManager.slugcatFeatures.Where(feature => feature.Slugcat == slugcat.name).ToArray();

                foreach (var feature in features)
                {
                    OpCheckBox featureCheckbox = new(feature.configurable, new Vector2(20f + featureOffset, 0f) + new Vector2(0, Mathf.Max(HEIGHT, scrollBox.contentSize) - 40 - index * checkBoxSpace.y));
                    featureCheckbox.colorEdge = slugcat.color;
                    featureCheckbox.description = feature.configurable.info.description;
                    OpLabel featureLabel = new(featureCheckbox.pos + labelSpace, default, feature.Name, FLabelAlignment.Left, false, null);
                    featureLabel.color = slugcat.color;

                    scrollBox.AddItems(featureCheckbox, featureLabel);
                    index++;
                }

                scrollBox.AddItems(checkbox, label);
            }
        }
    }
}
