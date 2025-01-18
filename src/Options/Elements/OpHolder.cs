using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace ArenaPlus.Options.Elements
{
    internal class OpHolder(Vector2 pos) : UIelement(pos, default(Vector2))
    {
        protected readonly HashSet<UIelement> items = [];

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
}
