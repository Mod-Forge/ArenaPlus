using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace ArenaPlus.Utils
{
    internal static class GameUtils
    {
        private static RainWorld rainWorld;
        public static RainWorldGame rainWorldGame { get; private set; }

        [HookRegister]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051")]
        private static void Register()
        {
            On.RainWorldGame.ctor += RainWorldGame_ctor;
            On.RainWorldGame.ShutDownProcess += RainWorldGame_ShutDownProcess;
        }

        private static void RainWorldGame_ShutDownProcess(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame self)
        {
            rainWorldGame = null;
            orig(self);
        }

        private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
        {
            rainWorldGame = self;
            orig(self, manager);
        }

        public static bool IsCompetitiveOrSandboxSession
        {
            get
            {
                if (rainWorldGame != null)
                {
                    return rainWorldGame.IsArenaSession && !(ModManager.MSC && rainWorldGame.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == DLCSharedEnums.GameTypeID.Challenge);
                }
                else return false;
            }
        }

        public static bool IsCompetitiveSession
        {
            get
            {
                if (rainWorldGame != null)
                {
                    return IsCompetitiveOrSandboxSession && rainWorldGame.session is not SandboxGameSession;
                }
                else return false;
            }
        }

        public static bool IsChallengeGameSession(RainWorldGame game)
        {
            return game.IsArenaSession && ModManager.MSC && game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == DLCSharedEnums.GameTypeID.Challenge;
        }

        public static RainWorld RainWorldInstance
        {
            get
            {
                rainWorld ??= UnityEngine.Object.FindObjectOfType<RainWorld>();
                return rainWorld;
            }
        }

        public static PlayerProgression.MiscProgressionData ProgressionData
        {
            get => RainWorldInstance.progression.miscProgressionData;
        }
    }

    public static class FContainerLayer
    {
        public const string Shadows = "Shadows";
		public const string BackgroundShortcuts = "BackgroundShortcuts";
		public const string Background = "Background";
		public const string Midground = "Midground";
		public const string Items = "Items";
		public const string Foreground = "Foreground";
		public const string Sand = "Sand";
		public const string ForegroundLights = "ForegroundLights";
		public const string Shortcuts = "Shortcuts";
		public const string Water = "Water";
		public const string GrabShaders = "GrabShaders";
		public const string Bloom = "Bloom";
		public const string WarpPoint = "WarpPoint";
		public const string HUD = "HUD";
		public const string HUD2 = "HUD2";
    }
}
