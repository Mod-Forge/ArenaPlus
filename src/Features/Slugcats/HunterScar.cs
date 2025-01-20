using ArenaPlus.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaPlus.Features.Slugcats
{
    [ImmutableFeature]
    file class HunterScar : ImmutableFeature
    {
        protected override void Register()
        {
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
        }

        private void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, UnityEngine.Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (self.player.slugcatStats?.name.ToString() == "Red")
            {

                FSprite fspriteFace = sLeaser.sprites[9];
                if (fspriteFace.element.name.StartsWith("Face"))
                {
                    bool keepFace = fspriteFace.element.name.Contains("A0") || fspriteFace.element.name.Contains("A8");

                    if (fspriteFace.scaleX > 0 || keepFace)
                    {
                        fspriteFace.scaleX = Math.Abs(fspriteFace.scaleX);
                        fspriteFace.SetElementByName("Hunter" + fspriteFace.element.name);
                    }
                }
            }
        }
    }
}
