using ArenaPlus.Lib;
using ArenaPlus.Options;
using ArenaPlus.Options.Tabs;
using Menu.Remix.MixedUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArenaPlus.Features
{
    [FeatureInfo(
        id: "spearsRespawn",
        name: "Spears respawn",
        description: "Whether spears reappear when they are all lost",
        enabledByDefault: false
    )]
    file class SpearsRespawn : Feature
    {
        private readonly Configurable<int> spearsRespawnTimer = OptionsInterface.instance.config.Bind("spearsRespawnTimer", 30, new ConfigurableInfo("The time in seconds before the spears respawn", new ConfigAcceptableRange<int>(0, 100), "", []));

        public SpearsRespawn(FeatureInfoAttribute featureInfo) : base(featureInfo)
        {
            SetComplementaryElement((feature, expandable, startPos) =>
            {
                OpUpdown updown = expandable.AddItem(
                    new OpUpdown(spearsRespawnTimer, startPos, 60f)
                );
                updown.pos -= new Vector2(0, (updown.size.y - FeaturesTab.CHECKBOX_SIZE) / 2);
                updown.description = configurable.info.description;

                if (feature.HexColor != "None" && ColorUtility.TryParseHtmlString("#" + feature.HexColor, out Color color))
                {
                    updown.colorEdge = color;
                }
            });
        }

        protected override void Register()
        {

        }

        protected override void Unregister()
        {

        }
    }
}
