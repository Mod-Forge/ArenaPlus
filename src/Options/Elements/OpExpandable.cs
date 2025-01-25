using ArenaPlus.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Menu.Remix.MixedUI;

namespace ArenaPlus.Options.Elements
{
    internal class OpExpandable : OpHolder
    {
        private readonly Vector2 defaultSize;
        private UIelement lowestItem = null;
        protected OpRect rect;
        private OpExpandButton expandButton;

        public bool hasBorder = true;

        public float LowestY => lowestItem != null ? lowestItem.GetPos().y - 10 : 0;

        public OpExpandable(Vector2 pos, Vector2 defaultSize, float animationDuration = 1f) : base(pos)
        {
            this.size = defaultSize;
            this.defaultSize = defaultSize;
            this.animationDuration = animationDuration;
        }

        public override T AddItem<T>(T item)
        {
            var addedItem = base.AddItem(item);

            if (lowestItem is null || addedItem.pos.y < lowestItem.pos.y)
            {
                lowestItem = addedItem;
            }

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

        private float expandedSize => GetPos().y - LowestY;
        public bool Moving => expanding || retracting || animationProgress > 0;
        private float animationProgress = 0f;
        private readonly float animationDuration;

        public void ToggleExpand()
        {
            if (Moving) return;

            expandButton.greyedOut = true;

            if (expanded)
            {
                retracting = true;
            }
            else
            {
                expanding = true;
            }
        }

        private float EaseOut(float x)
        {
            return x == 1 ? 1 : 1 - Mathf.Pow(2, -10 * x);
        }

        public void AddDefaults()
        {
            rect = AddItem(new OpRect(new Vector2(0, -defaultSize.y), defaultSize));
        }

        public void SetExpandButton(OpExpandButton button)
        {
            expandButton = button;

            button.OnClick += (UIfocusable trigger) =>
            {
                ToggleExpand();
            };

            button._rect.pos -= new Vector2(-1000, 1000);
            button.sprite.rotation = 180;
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

                    expandButton.sprite.rotation = expanded ? 0f : 180f;
                    expandButton.greyedOut = false;
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

        [HookRegister]
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

    public class OpExpandButton(Vector2 pos, Vector2 size) : OpSimpleImageButton(pos, size, "Menu_Symbol_Arrow")
    {
    }
}
