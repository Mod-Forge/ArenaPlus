using ArenaPlus.Lib;
using ArenaPlus.Options;
using ArenaPlus.Options.Tabs;
using ArenaPlus.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Menu.Remix.MixedUI;

namespace ArenaPlus.Features.NPC
{
    [FeatureInfo(
        id: "NPCAttackPlayers",
        name: "CPU attack players",
        description: "Make CPU attack players",
        enabledByDefault: true
    )]
    internal class NPCAttackPlayers : Feature
    {
        public static readonly Configurable<bool> npcAttackNPC = OptionsInterface.instance.config.Bind("npcAttackNPC", true);

        public NPCAttackPlayers(FeatureInfoAttribute featureInfo) : base(featureInfo)
        {
            SetComplementaryElement((expandable, startPos) =>
            {
                OpCheckBox checkBox = expandable.AddItem(
                    new OpCheckBox(npcAttackNPC, startPos)
                );

                checkBox.pos -= new Vector2(0, (checkBox.size.y - FeaturesTab.CHECKBOX_SIZE) / 2);
                checkBox.description = "Make CPU Attacks other CPU";

                if (HexColor != "None" && ColorUtility.TryParseHtmlString("#" + HexColor, out Color color))
                {
                    checkBox.colorEdge = color;
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
