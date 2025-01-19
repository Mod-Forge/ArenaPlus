using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ArenaPlus.Lib;
using ArenaPlus.Options.Elements;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using UnityEngine;

namespace ArenaPlus.Options.Tabs
{
    internal class FeaturesTab : OpTab
    {
        private const int MARGIN = 10;
        private const int HEIGHT = 475 - 20;
        private const int CHECKBOX_SIZE = 24;
        private readonly Vector2 INITIAL_POS = new(20, 375);

        private readonly Dictionary<Feature, OpCheckBox> checkBoxes = [];
        internal OpScrollBox scrollBox;

        public FeaturesTab(OptionInterface owner) : base(owner, "Features")
        {
            bool updating = false;
            int size = 48;
            int lineYOffset = 2;

            scrollBox = new(INITIAL_POS - new Vector2(0, HEIGHT - 100), new Vector2(560, HEIGHT), HEIGHT, false, false);
            AddItems(scrollBox, new OpRect(INITIAL_POS - new Vector2(0, HEIGHT - 100) - new Vector2(0, 5), new Vector2(560, HEIGHT + 10)));

            float lastYPos = HEIGHT;
            foreach (var category in FeaturesManager.categories)
            {
                OpExpandable expandable = new(new Vector2(20, lastYPos - 20), new Vector2(500, size), 1);

                expandables.Add(expandable);
                scrollBox.AddItems(expandable);

                expandable.AddItem(
                    new OpLine(new Vector2(MARGIN-3, - size - lineYOffset), new Vector2(500 - MARGIN * 2, 2))
                );

                OpCheckBox toggleAllCheckBox = expandable.AddItem(
                    new OpCheckBox(category.configurable, new Vector2(10, -size / 2 - 12))
                );


                toggleAllCheckBox.OnValueChanged += (UIconfig config, string value, string oldValue) =>
                {
                    if (updating) return;

                    category.features.ForEach(feature =>
                    {
                        if (!checkBoxes.TryGetValue(feature, out OpCheckBox checkBox)) return;

                        checkBox.SetValueBool(value == "true");
                    });
                };

                var expandButton = expandable.AddItem(
                    new OpExpandButton(new Vector2(500 - 10 - 24, -size / 2 - 12), new Vector2(24, 24))
                );

                expandable.SetExpandButton(expandButton);

                OpLabel titleLabel = expandable.AddItem(
                    new OpLabel(new Vector2(45, -size), new Vector2(10, size), category.name, FLabelAlignment.Left, true)
                );

                lastYPos = (lastYPos - 20) - size;

                int index = 0;
                float lastPos = -size - MARGIN - CHECKBOX_SIZE - lineYOffset;
                foreach (var feature in category.features)
                {
                    int xPos = MARGIN + (index % 2) * (500 / 2);

                    OpCheckBox checkBox = expandable.AddItem(
                        new OpCheckBox(feature.configurable, new Vector2(xPos, lastPos))
                        {
                            description = feature.Description,
                        }
                    );

                    checkBoxes.Add(feature, checkBox);

                    toggleAllCheckBox.SetValueBool(IsAllChecked(category.features));

                    checkBox.OnValueChanged += (UIconfig config, string value, string oldValue) =>
                    {
                        updating = true;
                        toggleAllCheckBox.SetValueBool(IsAllChecked(category.features));
                        updating = false;
                    };

                    OpLabel label = expandable.AddItem(
                        new OpLabel(new Vector2(xPos + CHECKBOX_SIZE + MARGIN, lastPos), new Vector2(0, CHECKBOX_SIZE), feature.Name, FLabelAlignment.Left)
                    );

                    if (feature.HexColor != "None" && ColorUtility.TryParseHtmlString("#" + feature.HexColor, out UnityEngine.Color color))
                    {
                        checkBox.colorEdge = color;
                        label.color = color;
                    }

                    if (index++ % 2 == 1)
                    {
                        lastPos -= CHECKBOX_SIZE + MARGIN;
                    }
                }
            }
        }

        private bool IsAllChecked(List<Feature> features)
        {
            return features.All(feature =>
            {
                if (!checkBoxes.TryGetValue(feature, out OpCheckBox checkBox)) return false;

                return checkBox.value == "true";
            });
        }

        public void Update()
        {
            float? lastYPos = null;
            float firstGudY = 0f;
            float lasYGudY = 0f;
            foreach (var expandable in expandables)
            {
                if (lastYPos.HasValue)
                {
                    expandable.pos = new Vector2(expandable.pos.x, lastYPos.Value - 20);
                }
                else
                {
                    firstGudY = expandable.GetPos().y + 20;
                }

                lastYPos = expandable.pos.y - expandable.size.y;
                lasYGudY = expandable.GetPos().y - expandable.size.y - 20;
            }

            float lastContentSize = scrollBox.contentSize;
            float newSize = firstGudY - lasYGudY;

            scrollBox.SetContentSize(newSize);

            scrollBox.targetScrollOffset -= scrollBox.contentSize - lastContentSize;
            scrollBox.scrollOffset = scrollBox.targetScrollOffset;
        }

        internal static List<OpExpandable> expandables = [];
    }
}
