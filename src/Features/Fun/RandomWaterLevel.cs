using ArenaPlus.Utils;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MoreSlugcats;
using ArenaPlus.Lib;
using ArenaPlus.Options.Tabs;
using ArenaPlus.Options;
using Menu.Remix.MixedUI;

namespace ArenaPlus.Features.Fun
{
    [FeatureInfo(
        id: "randomWaterLevel",
        name: "Random water level",
        category: BuiltInCategory.Fun,
        description: "Add water to every levels with a random initial height",
        enabledByDefault: false
    )]
    file class RandomWaterLevel : Feature
    {
        public static readonly Configurable<int> maxWaterHeightConfigurable = OptionsInterface.instance.config.Bind("maxWaterHeight", 50, new ConfigurableInfo("The maximum height the water can have (between 1 and 100)", new ConfigAcceptableRange<int>(1, 100), "", []));

        public RandomWaterLevel(FeatureInfoAttribute featureInfo) : base(featureInfo)
        {
            SetComplementaryElement((expandable, startPos) =>
            {
                OpUpdown updown = expandable.AddItem(
                    new OpUpdown(maxWaterHeightConfigurable, startPos, 60f)
                );
                updown.pos -= new Vector2(0, (updown.size.y - FeaturesTab.CHECKBOX_SIZE) / 2);
                updown.description = maxWaterHeightConfigurable.info.description;

                if (HexColor != "None" && ColorUtility.TryParseHtmlString("#" + HexColor, out Color color))
                {
                    updown.colorEdge = color;
                }
            });
        }

        protected override void Unregister()
        {
            On.ArenaGameSession.ctor -= ArenaGameSession_ctor;
        }

        protected override void Register()
        {
            On.ArenaGameSession.ctor += ArenaGameSession_ctor;
        }

        private void ArenaGameSession_ctor(On.ArenaGameSession.orig_ctor orig, ArenaGameSession self, RainWorldGame game)
        {
            orig(self, game);
            LogDebug("adding RandomWaterLevelBehavior");
            self.AddBehavior(new RandomWaterLevelBehavior(self));
        }
    }

    file class RandomWaterLevelBehavior : ArenaBehaviors.ArenaGameBehavior
    {
        public RandomWaterLevelBehavior(ArenaGameSession gameSession) : base(gameSession)
        {
        }

        public override void Initiate()
        {
            if (Random.value < 0.16)
            {
                if (room.water)
                {
                    room.waterObject.Destroy();
                    room.waterObject = null;
                }
                LogDebug("Random water initiated with no water");

            }
            else
            {
                int maxLevel = (int)(room.Height * ((float)RandomWaterLevel.maxWaterHeightConfigurable.Value / 100f));
                room.defaultWaterLevel = maxLevel;
                if (game.session is not SandboxGameSession || (game.session as ArenaGameSession).arenaSitting.sandboxPlayMode)
                {
                    room.defaultWaterLevel = (int)Random.Range(1, Mathf.Max(maxLevel, 1));
                }
                room.floatWaterLevel = room.MiddleOfTile(new IntVector2(0, room.defaultWaterLevel)).y;
                if (!room.water)
                {
                    room.waterInFrontOfTerrain = true;
                }
                else
                {
                    room.waterObject.Destroy();
                    room.waterObject = null;
                }
                room.AddWater();
                room.waterObject.WaterIsLethal = false;
                LogDebug("Random water initiated with level", room.defaultWaterLevel, "/", maxLevel);
            }

        }
    }
}
