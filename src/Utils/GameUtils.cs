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

        public static bool IsCompetitiveOrSandboxSession
        {
            get
            {
                if (RainWorldInstance.processManager.currentMainLoop is RainWorldGame game)
                {
                    return game.IsArenaSession && !(ModManager.MSC && game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge);
                }
                else return false;
            }
        }

        public static bool IsCompetitiveSession
        {
            get
            {
                if (RainWorldInstance.processManager.currentMainLoop is RainWorldGame game)
                {
                    return game.IsArenaSession && game.session is not SandboxGameSession;
                }
                else return false;
            }
        }

        public static bool IsChallengeGameSession(RainWorldGame game)
        {
            return game.IsArenaSession && ModManager.MSC && game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge;
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
}
