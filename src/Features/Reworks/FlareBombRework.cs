using ArenaPlus.Utils;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArenaPlus.Features.Reworks
{
    [FeatureInfo(
        id: "flareBombRework",
        name: "Flare bomb rework",
        description: "Make the flare bomb blind players",
        enabledByDefault: true
    )]
    file class FlareBombRework(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
    {
        protected override void Register()
        {
            On.FlareBomb.DrawSprites += FlareBomb_DrawSprites;
            On.FlareBomb.Update += FlareBomb_Update;
        }

        protected override void Unregister()
        {
            On.FlareBomb.DrawSprites -= FlareBomb_DrawSprites;
            On.FlareBomb.Update -= FlareBomb_Update;
        }

        protected void FlareBomb_DrawSprites(On.FlareBomb.orig_DrawSprites orig, FlareBomb self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (!self.slatedForDeletetion && self.room == rCam.room && self.room.game.IsArenaSession && !GameUtils.IsChallengeGameSession(self.room.game) && self.burning > 0)
            {
                sLeaser.sprites[2].shader = rCam.room.game.rainWorld.Shaders["FlatLightNoisy"];
                sLeaser.sprites[2].color = Color.white;
                sLeaser.sprites[2].scale = 400f;
                sLeaser.sprites[2].alpha = Math.Min(1, sLeaser.sprites[2].alpha * 4f);
            }
        }

        protected void FlareBomb_Update(On.FlareBomb.orig_Update orig, FlareBomb self, bool eu)
        {
            if (self.room.game.IsArenaSession && !GameUtils.IsChallengeGameSession(self.room.game))
            {
                if (self.burning > 0f)
                {
                    self.burning = Math.Max(0.01f, self.burning - 0.005f);
                }
            }
            orig(self, eu);
        }
    }
}
