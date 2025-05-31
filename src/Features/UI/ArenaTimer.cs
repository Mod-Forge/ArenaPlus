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

        static HashSet<Timer> timers = new HashSet<Timer>();
        public static Timer StartTimer(string text, int ticks, bool pause = true)
        {
            LogDebug($"starting timer \"{text}\"");
            var timer = new Timer(text, ticks, pause);
            timers.Add(timer);
            return timer;
        }

        private static void StopTimer(Timer timer)
        {
            if (!timers.Contains(timer)) return;
            timer.life = 0;
            timers.Remove(timer);
        }


        public class Timer(string text, int ticks, bool pause)
        {
            public string text = text;
            public float life = ticks;
            public bool Done => life <= 0;
            public bool pause = pause;
            public bool Paused => GameUtils.rainWorldGame.GamePaused && pause;
            public void Dispose() => StopTimer(this);
            public Action<Timer> onTimerEnd;
        }

        private class ArenaTimerHUD : HudPart
        {
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
                string text = string.Empty;
                HashSet<Timer> removeQue = new HashSet<Timer>();

                foreach (var timer in timers)
                {
                    if (!timer.Paused)
                        timer.life -= 40f / (float)GameUtils.rainWorldGame.framesPerSecond;

                    if (timer.Done)
                    {
                        removeQue.Add(timer);
                    }
                    else
                    {
                        DateTime endTime = DateTime.Now.AddSeconds((timer.life + (timer.Paused ? 0f : GameUtils.rainWorldGame.myTimeStacker)) / 40f);
                        text += $"\n{timer.text} {(endTime - DateTime.Now).ToString(@"mm\:ss\:ff")}";
                    }
                }

                foreach (var timer in removeQue)
                {
                    timers.Remove(timer);
                    timer.onTimerEnd?.Invoke(timer);
                }

                fLabel.isVisible = text.Length > 0;
                fLabel.text = text;
            }

            public override void ClearSprites()
            {
                base.ClearSprites();
                timers.Clear();
            }
        }
    }
}