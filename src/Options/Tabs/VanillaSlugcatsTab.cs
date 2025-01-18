using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using ArenaPlus.Utils;
using Menu;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.PlayerLoop;

namespace ArenaPlus.Options.Tabs
{
    internal class VanillaSlugcatsTab : OpTab
    {
        private static Vector2 checkBoxSpace = new(0f, 20f + 10f);
        private static readonly Vector2 labelSpace = new(40f, 3f);
        private static readonly Vector2 initialPos = new Vector2(20f, 405f) - checkBoxSpace;
        private Dictionary<Feature, OpCheckBox> checkBoxes = new();

        public VanillaSlugcatsTab(OptionInterface owner) : base(owner, "Vanilla")
        {
            int height = 475 - 20;
            int contentSize = Feature.features.Count * 30 + 24;


            scrollBox = new(initialPos - new Vector2(0, height - 100), new Vector2(560, height), contentSize, false, false);
            AddItems(scrollBox, new OpRect(initialPos - new Vector2(0, height - 100) - new Vector2(0, 5), new Vector2(560, height + 10)));

            float lastYPos = height;

            foreach (var category in Feature.categories)
            {
                int size = 48;
                OpExpandable expandable = new(new Vector2(20, lastYPos - 20), new Vector2(500, size), 1);

                scrollBox.AddItems(expandable);

                int margin = 10;
                int lineYOffset = 2; 

                var line = expandable.AddItem(
                    new OpLine(new Vector2(margin-3, - size - lineYOffset), new Vector2(500 - margin * 2, 2))
                );

                OpCheckBox toggleAllCheckBox = expandable.AddItem(
                    new OpCheckBox(category.configurable, new Vector2(10, -size / 2 - 12))
                );

                bool updating = false;

                toggleAllCheckBox.OnValueChanged += (UIconfig config, string value, string oldValue) =>
                {
                    if (updating) return;

                    category.features.ForEach(feature =>
                    {
                        if (!checkBoxes.TryGetValue(feature, out OpCheckBox checkBox)) return;

                        checkBox.SetValueBool(value == "true");
                    });
                };

                var expandBtn = expandable.AddItem(
                    new OpSimpleImageButton(new Vector2(500 - 10 - 24, -size / 2 - 12), new Vector2(24, 24), "Menu_Symbol_Arrow")
                );

                expandBtn.OnClick += (UIfocusable trigger) =>
                {
                    expandable.ToggleExpand();
                };

                expandBtn.OnUpdate += () =>
                {
                    expandBtn.greyedOut = expandable.Moving;
                    expandBtn.sprite.rotation = expandable.expanded ? 0f : 180f;
                };

                expandBtn._rect.pos -= new Vector2(-1000, 1000);
                expandBtn.sprite.rotation = 180;

                OpLabel label = expandable.AddItem(
                    new OpLabel(new Vector2(45, -size), new Vector2(10, size), category.name, FLabelAlignment.Left, true)
                );

                expandables.Add(expandable);
                lastYPos = (lastYPos - 20) - size;

                int index = 0;
                int checkBoxSize = 24;
                float lastPos = -size - margin - checkBoxSize - lineYOffset;
                foreach (var feature in category.features)
                {
                    int xPos = margin + (index % 2) * (500 / 2);

                    OpCheckBox checkBox = expandable.AddItem(
                        new OpCheckBox(feature.configurable, new Vector2(xPos, lastPos))
                        {
                            description = feature.Description,<
                        }
                    );



                    checkBox.OnValueChanged += (UIconfig config, string value, string oldValue) =>
                    {
                        updating = true;
                        toggleAllCheckBox.SetValueBool(IsAllChecked(category.features));
                        updating = false;
                    };

                    checkBoxes.Add(feature, checkBox);

                    OpLabel l = expandable.AddItem(
                        new OpLabel(new Vector2(xPos + checkBoxSize + margin, lastPos), new Vector2(0, checkBoxSize), feature.Name, FLabelAlignment.Left)
                    );

                    if (feature.HexColor != "None" && ColorUtility.TryParseHtmlString("#" + feature.HexColor, out UnityEngine.Color color))
                    {
                        checkBox.colorEdge = color;
                        l.color = color;
                    }

                    if (index++ % 2 == 1)
                    {
                        lastPos -= checkBoxSize + margin;
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

            if (scrollBox.scrollOffset == 1)
            {
                scrollBox.scrollOffset = 0;
            }
            scrollBox.targetScrollOffset -= scrollBox.contentSize - lastContentSize;
            scrollBox.scrollOffset = scrollBox.targetScrollOffset;
        }

        internal static OpScrollBox scrollBox;
        internal static List<OpExpandable> expandables = new List<OpExpandable>();

    }

    internal class OpContainer : UIelement
    {
        protected readonly HashSet<UIelement> items = [];
        public OpContainer(Vector2 pos) : base(pos, default(Vector2))
        {
        }

        public T AddItem<T>(T item) where T : UIelement
        {
            items.Add(item);
            item.pos += pos;
            if (InScrollBox && !item.InScrollBox)
            {
                item.InScrollBox = true;
                item.scrollBox = scrollBox;
                item.Change();
                scrollBox.items.Add(item);
                if (scrollBox._lastFocusedElement == null && item is UIfocusable focusableItem)
                {
                    scrollBox._lastFocusedElement = focusableItem;
                }
            }
            tab.AddItems(item);
            return item;
        }

        public void AddItems(params UIelement[] items)
        {
            Array.ForEach(items, (item) => AddItem(item));
        }

        private void UpdateItems(Vector2 lastPos, Vector2 pos)
        {
            Vector2 offset = pos - lastPos;
            foreach (var item in items)
            {
                item.pos += offset;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Styles d'affectation de noms")]
        public new Vector2 pos
        {
            get => _pos;
            set
            {
                UpdateItems(_pos, value);
                _pos = value;
            }
        }
    }

    internal class OpExpandable : OpContainer
    {
        private Vector2 defaultPos;
        private readonly Vector2 defaultSize;
        private UIelement lowestItem = null;
        public float lowestY => lowestItem != null ? lowestItem.GetPos().y - 10 : 0;

        protected OpRect rect;
        public bool hasBorder = true;


        public OpExpandable(Vector2 pos, Vector2 defaultSize, float animationDuration = 1f) : base(pos)
        {
            this.defaultPos = pos;
            this.size = defaultSize;
            this.defaultSize = defaultSize;
            this.animationDuration = animationDuration;
        }

        public new T AddItem<T>(T item) where T : UIelement
        {
            var addedItem = base.AddItem(item);

            if (lowestItem is null || addedItem.pos.y < lowestItem.pos.y)
            {
                lowestItem = addedItem;
            }

            //Log(item.pos.y);

            if (!IsItemInView(item))
            {
                item.Hide();
            }


            return addedItem;
        }

        private bool IsItemInView(UIelement item)
        {
            return item.GetPos().y >= GetPos().y - size.y;
        }


        private bool expanding = false;
        private bool retracting = false;
        public bool expanded { get; private set; }

        private float expandedSize => GetPos().y - lowestY;
        public bool Moving => expanding || retracting || animationProgress > 0;
        private float animationProgress = 0f;
        private float animationDuration = 1f;

        public void ToggleExpand()
        {
            if (Moving) return;
            if (expanded)
            {
                Log("retracting");
                retracting = true;
            } else
            {
                expanding = true;
            }
        }

        private float EaseOut(float x)
        {
            return x == 1 ? 1 : 1 - Mathf.Pow(2, -10 * x);
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            if (x < 1 / d1)
            {
                return n1 * x * x;
            }
            else if (x < 2 / d1)
            {
                return n1 * (x -= 1.5f / d1) * x + 0.75f;
            }
            else if (x < 2.5 / d1)
            {
                return n1 * (x -= 2.25f / d1) * x + 0.9375f;
            }
            else
            {
                return n1 * (x -= 2.625f / d1) * x + 0.984375f;
            }
        }

        public void AddDefaults()
        {
            rect = AddItem(new OpRect(new Vector2(0, -defaultSize.y), defaultSize));
        }

        public override void Update()
        {
            if (Moving)
            {
                float dt = 0.025f;
                float step = dt * (1 / animationDuration);
                animationProgress += step;
                animationProgress = Mathf.Clamp(animationProgress, 0f, 1f);

                float from = defaultSize.y;
                float to = expandedSize;
                if (retracting)
                {
                    to = from;
                    from = expandedSize;
                }

                float t = EaseOut(animationProgress);
                float lastT = EaseOut(Mathf.Max(animationProgress - step, 0));

                float y = Mathf.Lerp(from, to, t);
                float yStep = Mathf.Lerp(from, to, t) - Mathf.Lerp(from, to, lastT);

                if (rect != null)
                {
                    rect.pos += Vector2.down * yStep;
                    rect.size += Vector2.up * yStep;
                }

                size = new Vector2(0, y);

                if (animationProgress >= 1)
                {
                    animationProgress = 0f;
                    expanded = !expanded;
                    expanding = false;
                    retracting = false;
                }

                foreach (var item in items)
                {
                    if (IsItemInView(item) || (rect != null && item == rect))
                    {
                        item.Show();
                    }
                    else
                    {
                        item.Hide();
                    }
                }

                OnExpandProgress?.Invoke(y, yStep);
            }

            base.Update();
        }

        public delegate void OnExpandProgressHandler(float sizeY, float yStep);

        public event OnExpandProgressHandler OnExpandProgress;

        [HookRegister.HookRegister]
        public static void RegisterHooks()
        {
            On.Menu.Remix.MixedUI.UIelement._SetTab += (On.Menu.Remix.MixedUI.UIelement.orig__SetTab orig, UIelement self, OpTab newTab) =>
            {
                orig(self, newTab);
                if (self is OpExpandable expandable)
                {
                    expandable.AddDefaults();
                }
            };
        }
    }

    public class OpLine : UIelement
    {
        public FSprite sprite;

        public OpLine(Vector2 pos, Vector2 size) : base(pos, size)
        {
            sprite = new FSprite("pixel", true);
            myContainer.AddChild(
                sprite
            );
            sprite.color = MenuColorEffect.rgbMediumGrey;
            sprite.anchorX = 0;
            sprite.anchorY = 0;
            sprite.SetPosition(Vector2.zero);
        }

        public override void GrafUpdate(float timeStacker)
        {
            myContainer.SetPosition(pos);
            sprite.scaleX = size.x;
            sprite.scaleY = size.y;

        }
    }
}
