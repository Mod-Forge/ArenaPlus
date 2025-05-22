using ArenaPlus.Lib;
using ArenaPlus.Utils;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArenaPlus.Features
{
    [ImmutableFeature]
    public class ScoreForEveryone : ImmutableFeature
    {
        public static int[] defaultKillScores = new int[ExtEnum<MultiplayerUnlocks.SandboxUnlockID>.values.Count];
        private bool scoreInitiated;

        protected override void Register()
        {
            On.Menu.SandboxSettingsInterface.DefaultKillScores += SandboxSettingsInterface_DefaultKillScores;
            On.StaticWorld.InitStaticWorld += StaticWorld_InitStaticWorld;
        }

        private void StaticWorld_InitStaticWorld(On.StaticWorld.orig_InitStaticWorld orig)
        {
            orig();
            if (!scoreInitiated)
            {
                try { Menu.SandboxSettingsInterface.DefaultKillScores(ref ScoreForEveryone.defaultKillScores); }
                catch (Exception e) { LogError(e); }

                scoreInitiated = true;
            }
        }

        public static int GetProceduralScore(string critName)
        {
            int score = 0;

            CreatureTemplate template = StaticWorld.GetCreatureTemplate(critName);
            if (template == null && CreatureTemplate.Type.values.entries.Contains(critName))
            {
                template = StaticWorld.GetCreatureTemplate(new CreatureTemplate.Type(critName, false));
            }

            if (template != null)
            {
                float newScore = (template.meatPoints > 0f ? template.meatPoints : 1f) * (template.dangerousToPlayer > 0f ? template.dangerousToPlayer : 1f) * (template.baseDamageResistance + template.baseStunResistance); ;

                LogDebug("generating score for creature", critName, "score", newScore);
                score = (int)newScore;
            }
            else
            {
                LogDebug("faild to find creature", critName);
                return 0;
            }

            return Mathf.Clamp(score, 1, 25);
        }

        private void SandboxSettingsInterface_DefaultKillScores(On.Menu.SandboxSettingsInterface.orig_DefaultKillScores orig, ref int[] killScores)
        {
            orig(ref killScores);

            LogDebug("adding custom scores");
            for (int i = 0; i < killScores.Length; i++)
            {
                if (killScores[i] == 0)
                {
                    var name = MultiplayerUnlocks.SandboxUnlockID.values.GetEntry(i);
                    killScores[i] = GetProceduralScore(name);
                }
            }

        }
    }
}
