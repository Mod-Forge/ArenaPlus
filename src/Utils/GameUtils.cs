using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ArenaPlus.Utils
{
    internal static class GameUtils
    {
        public static bool IsChallengeGameSession(RainWorldGame game)
        {
            if (game.IsArenaSession && ModManager.MSC && game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge)
            {
                return true;
            }
            return false;
        }

        public static RainWorld RainWorldInstance
        {
            get => UnityEngine.Object.FindObjectOfType<RainWorld>();
        }

        public static PlayerProgression.MiscProgressionData ProgressionData
        {
            get => RainWorldInstance.progression.miscProgressionData;
        }
    }
}
