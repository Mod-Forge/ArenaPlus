using Menu;
using Menu.Remix.MixedUI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArenaPlus.Options.Elements
{
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
