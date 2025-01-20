using ArenaBehaviors;
using ArenaPlus.Lib;
using ArenaPlus.Options;
using ArenaPlus.Options.Tabs;
using Menu.Remix.MixedUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArenaPlus.Features
{
    [FeatureInfo(
        id: "spawnKillProtection",
        name: "Start timer",
        description: "Add a spawn kill protection timer at the start of each rounds",
        enabledByDefault: false
    )]
    file class SpawnKillProtection : Feature
    {
        public static readonly Configurable<int> spawnKillProtectionTimerConfigurable = OptionsInterface.instance.config.Bind("spawnKillProtectionTimer", 5, new ConfigurableInfo("The time in seconds of the spawn kill protection", new ConfigAcceptableRange<int>(0, 100), "", []));


        public SpawnKillProtection(FeatureInfoAttribute featureInfo) : base(featureInfo)
        {
            SetComplementaryElement((expandable, startPos) =>
            {
                OpUpdown updown = expandable.AddItem(
                    new OpUpdown(spawnKillProtectionTimerConfigurable, startPos, 60f)
                );
                updown.pos -= new Vector2(0, (updown.size.y - FeaturesTab.CHECKBOX_SIZE) / 2);
                updown.description = spawnKillProtectionTimerConfigurable.info.description;

                if (HexColor != "None" && ColorUtility.TryParseHtmlString("#" + HexColor, out Color color))
                {
                    updown.colorEdge = color;
                }
            });
        }

        protected override void Register()
        {
            On.ArenaGameSession.ctor += ArenaGameSession_ctor;
            On.Player.Die += Player_Die;
            On.Player.Destroy += Player_Destroy;
            On.RWInput.PlayerInputLogic_int_int += RWInput_PlayerInputLogic_int_int;
        }

        protected override void Unregister()
        {
            On.ArenaGameSession.ctor -= ArenaGameSession_ctor;
            On.Player.Die -= Player_Die;
            On.RWInput.PlayerInputLogic_int_int -= RWInput_PlayerInputLogic_int_int;
        }

        private Player.InputPackage RWInput_PlayerInputLogic_int_int(On.RWInput.orig_PlayerInputLogic_int_int orig, int categoryID, int playerNumber)
        {
            var input = orig(categoryID, playerNumber);
            if (SpawnProtectionTimerBehavior.protection)
            {
                input.thrw = false;
            }
            return input;
        }

        private bool forceKill;
        private void Player_Die(On.Player.orig_Die orig, Player self)
        {
            if (SpawnProtectionTimerBehavior.protection && !forceKill) { return; }
            orig(self);
            forceKill = false;
        }

        private void Player_Destroy(On.Player.orig_Destroy orig, Player self)
        {
            if (SpawnProtectionTimerBehavior.protection)
            {
                forceKill = true;
                self.Die();
            }
            orig(self);
        }

        private void ArenaGameSession_ctor(On.ArenaGameSession.orig_ctor orig, ArenaGameSession self, RainWorldGame game)
        {
            orig(self, game);
            if (self is not SandboxGameSession || !self.arenaSitting.sandboxPlayMode)
            {
                self.AddBehavior(new SpawnProtectionTimerBehavior(self));
            }
        }
    }

    file class SpawnProtectionTimerBehavior : ArenaGameBehavior
    {
        private static DateTime endTime;
        private const string timerText = "Figth start in";
        public static bool protection => endTime > DateTime.Now;

        public SpawnProtectionTimerBehavior(ArenaGameSession gameSession) : base(gameSession)
        {
        }

        public override void Initiate()
        {
            endTime = DateTime.Now.AddSeconds(SpawnKillProtection.spawnKillProtectionTimerConfigurable.Value);
            UI.ArenaTimer.StartTimer(timerText, endTime);
        }

        public override void Update()
        {
            if (!protection) return;
            foreach (var shortCut in game.shortcuts.transportVessels)
            {
                if (shortCut.creature is not Player)
                {
                    shortCut.wait = 4;
                }
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            endTime = default;
        }
    }
}
