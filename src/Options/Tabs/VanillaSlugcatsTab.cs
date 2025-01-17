using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using ArenaPlus.Utils;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace ArenaPlus.Options.Tabs
{
    internal class VanillaSlugcatsTab : OpTab
    {
        private static Vector2 checkBoxSpace = new(0f, 20f + 10f);
        private static readonly Vector2 labelSpace = new(40f, 3f);
        private static readonly Vector2 initialPos = new Vector2(20f, 405f) - checkBoxSpace;

        public VanillaSlugcatsTab(OptionInterface owner) : base(owner, "Vanilla")
        {
            Category[] categories = [   
                new Category(name, Feature.features),                new Category("dsgfh", [])

            ];

            int height = 475 - 20;
            int contentSize = Feature.features.Count * 30 + 24;

            OpScrollBox moddedScrollBox = new(initialPos - new Vector2(0, height - 100), new Vector2(560, height), contentSize, false, false);

            AddItems(moddedScrollBox, new OpRect(initialPos - new Vector2(0, height - 100) - new Vector2(0, 5), new Vector2(560, height + 10)));

            OpRect rect = new OpRect(initialPos - new Vector2(0, height - 100), new Vector2(560, height));

            OpSimpleButton upButton = new OpSimpleImageButton(new Vector2(0, 100), new Vector2(20, 20), "Sandbox_A");
            OpSimpleButton leftButton = new OpSimpleImageButton(new Vector2(-100, 0), new Vector2(20, 20), "Sandbox_A");
            OpSimpleButton rightButton = new OpSimpleImageButton(new Vector2(100, 0), new Vector2(20, 20), "Sandbox_A");
            OpSimpleButton bottomButton = new OpSimpleImageButton(new Vector2(0, -100), new Vector2(20, 20), "Sandbox_A");

            int size = 48;

            OpExpandable container = new(new Vector2(20, height - 20), new Vector2(500, size));


            moddedScrollBox.AddItems(container);

            var d = container.AddItem(
                new OpRect(new Vector2(0, -size), new Vector2(500, size))
            );

            container.OnExpandProgress += (Vector2 progression) =>
            {
                Log("Expanding outside");
                d.pos -= progression;
                d.size += progression;
            };

            container.AddItem(
                new OpCheckBox(Feature.features[0].configurable, new Vector2(10, -size / 2 - 12))
            );

            var btn = container.AddItem(
                new OpSimpleImageButton(new Vector2(500 - 10 - 24, -size/2-12), new Vector2(24, 24), "Menu_Symbol_Arrow")    
            );

            container.AddItem(
                new OpSimpleImageButton(new Vector2(500 - 10 - 24, -size / 2 - 12 - 200), new Vector2(24, 24), "Menu_Symbol_Arrow")
            );

            btn.OnClick += (UIfocusable trigger) =>
            {
                container.ToggleExpand();
            };

            btn._rect.pos -= new Vector2(-1000, 1000);
            btn.sprite.rotation = 180;

            OpLabel leabel = container.AddItem(
                new OpLabel(new Vector2(45, -size), new Vector2(20, size), "General", FLabelAlignment.Left, true)
            );


            float? lastYPos = null;

            foreach (var category in categories)
            {
                break;
                float verticalOffset = lastYPos != null ? (float) lastYPos - 40f : Mathf.Max(height, contentSize) - 40;
                var configurable = OptionsInterface.instance.config.Bind(null, false, new ConfigurableInfo("dec", null, "", []));
                OpCheckBox globalCheckbox = new(configurable, new Vector2(20f, 0f) + new Vector2(0, verticalOffset));
                OpLabel label = new(globalCheckbox.pos + labelSpace, default(Vector2), $"General", FLabelAlignment.Left, true, null);
                OpRect separator = new(globalCheckbox.pos - new Vector2(0, 10), new Vector2(500, 2));

                lastYPos = verticalOffset;

                int index = 0;
                foreach (var item in category.features)
                {
                    verticalOffset = (float) lastYPos - (index == 0 ? 50f : 30f);
                    OpCheckBox checkbox = new(item.configurable, new Vector2(20f, 0f) + new Vector2(0, verticalOffset));
                    OpLabel la = new(checkbox.pos + labelSpace, default(Vector2), item.Name, FLabelAlignment.Left, false, null);

                    //moddedScrollBox.AddItems(checkbox, la);

                    lastYPos = verticalOffset;
                    index++;
                }

                //moddedScrollBox.AddItems(globalCheckbox, label, separator);
            }
        }
    }

    internal class OpContainer : UIelement
    {
        protected readonly HashSet<UIelement> items = [];

        public OpContainer(Vector2 pos) : base(pos, default(Vector2))
        {
        }

        private Vector2 scrollOffset;

        public T AddItem<T>(T item) where T : UIelement
        {
            items.Add(item);
            if (InScrollBox)
            {
                var oldPos = item.pos;
                scrollBox.AddItems(item);
                scrollOffset = item.pos - oldPos;
                item._RemoveFromScrollBox();
            }
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

        public OpExpandable(Vector2 pos, Vector2 defaultSize) : base(pos)
        {
            this.defaultPos = pos;
            this.size = defaultSize;
            this.defaultSize = defaultSize;
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
        private bool expanded = false;

        private float ExpandingAnimationProgress => size.y / (GetPos().y - lowestItem.GetPos().y);
        
        // TODO: Implement that better
        private float RetractingAnimationProgress => 0.5f;

        public void ToggleExpand()
        {
            if (expanded)
            {
                Log("retracting");
                retracting = true;
            } else
            {
                expanding = true;
            }
        }

        private float EaseOutQuart(float x)
        {
            return x == 1 ? 1 : 1 - Mathf.Pow(2, -10 * x);
        }

        public override void Update()
        {
            if (expanding || retracting)
            {
                Log("Expanding inside");
                float x = (float)Math.Round(EaseOutQuart(1 - (retracting ? RetractingAnimationProgress : ExpandingAnimationProgress)) * 10 * 4, MidpointRounding.ToEven) / 4;

                if (expanding && ExpandingAnimationProgress > 0.99f)
                {
                    x += 0.1f;
                }

                Log(RetractingAnimationProgress);

                if (retracting && RetractingAnimationProgress > 0.99f)
                {
                    x += 0.1f;
                }

                x *= retracting ? -1 : 1;


                Log("Progress: " + x);
                OnExpandProgress(new Vector2(0, x));
                size += new Vector2(0, x);
                Log("Expanding inside 2");


                foreach (var item in items)
                {
                    if (IsItemInView(item))
                    {
                        item.Show();
                    } else
                    {
                        item.Hide();
                    }
                }
            }

            if (expanding)
            {
                if (GetPos().y - size.y <= lowestItem.GetPos().y)
                {
                    expanded = true;
                    expanding = false;
                }

            }

            if (retracting)
            {
                if (GetPos().y - size.y >= (defaultPos.y - defaultSize.y - 10))
                {
                    expanded = false;
                    retracting = false;
                }
            }

            base.Update();
        }

        public delegate void OnExpandProgressHandler(Vector2 progression);

        public event OnExpandProgressHandler OnExpandProgress;
    }
}
