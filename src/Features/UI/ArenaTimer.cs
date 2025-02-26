using ArenaPlus.Lib;
using ArenaPlus.Utils;
using HUD;
using Menu;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;

namespace ArenaPlus.Features.UI
{
    [ImmutableFeature]
    public class ArenaTimer : ImmutableFeature
    {
        protected override void Register()
        {
            On.HUD.HUD.InitMultiplayerHud += HUD_InitMultiplayerHud;
        }

        private void HUD_InitMultiplayerHud(On.HUD.HUD.orig_InitMultiplayerHud orig, HUD.HUD self, ArenaGameSession session)
        {
            orig(self, session);
            self.AddPart(new ArenaTimerHUD(self));
        }

        public static void StartTimer(string text, DateTime endTime, bool pause = false)
        {
            Assert(!Active || text == ArenaTimerHUD.text, "Only one timer can by active at the time");
            ArenaTimerHUD.time = endTime;
            ArenaTimerHUD.text = text;
            ArenaTimerHUD.pause = pause;
        }

        public static bool StopTimer(string text)
        {
            if (text == Text)
            {
                StopTimer();
                return true;
            }
            return false;
        }

        public static void StopTimer()
        {
            ArenaTimerHUD.time = default;
            ArenaTimerHUD.text = "None";
        }

        public static bool IsTimerDone(string text)
        {
            return ArenaTimerHUD.text != text || !Active;
        }

        public static bool Active => DateTime.Now < ArenaTimerHUD.time;
        public static string Text { get => ArenaTimerHUD.text; }
        public static DateTime endTime { get => ArenaTimerHUD.time; }

        private class ArenaTimerHUD : HudPart
        {
            internal static DateTime time;
            internal static TimeSpan remaningTime;
            internal static string text = "None";
            internal static bool pause;
            internal static bool paused;
            internal FLabel fLabel;
            public ArenaTimerHUD(HUD.HUD hud) : base(hud)
            {
                fLabel = new FLabel(Custom.GetFont(), "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas a turpis tortor. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Proin non mauris elit. Curabitur viverra suscipit elit vitae elementum. Donec in fringilla nunc, vel efficitur ipsum. Ut congue felis in neque lobortis, vitae scelerisque diam egestas. Aenean congue lectus ut orci consectetur pulvinar.");
                hud.fContainers[1].AddChild(fLabel);
                fLabel.scale = 2.5f;
                fLabel.anchorX = 0f;
                Vector2 pos = this.hud.rainWorld.screenSize * new Vector2(0f, 1f) + new Vector2(50, -50);
                fLabel.SetPosition(pos);
            }

            public override void Update()
            {
                base.Update();
                fLabel.isVisible = DateTime.Now < time;

                if (GameUtils.rainWorldGame.GamePaused != paused && pause)
                {
                    paused = GameUtils.rainWorldGame.GamePaused;

                    if (paused)
                    {
                        remaningTime = (endTime - DateTime.Now);
                    }
                }

                if (paused)
                {
                    time = DateTime.Now + remaningTime;
                }
                else
                {
                    if (DateTime.Now <= time)
                    {
                        fLabel.text = text + " " + ((time - DateTime.Now).ToString(@"ss\:ff"));
                    }
                    else
                    {
                        text = "None";
                    }
                }
            }
        }
    }
}