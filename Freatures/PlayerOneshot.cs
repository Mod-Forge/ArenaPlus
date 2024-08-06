
using System;
using System.Linq;

namespace ArenaSlugcatsConfigurator.Freatures
{
    internal static class PlayerOneshot
    {
        public static string[] expetions = new string[1]
        {
            "Gourmand"
        };

        internal static void OnEnable()
        {
            logSource.LogInfo("PlayerOneshot OnEnable");
            On.Spear.HitSomething += Spear_HitSomething;
        }

        private static bool Spear_HitSomething(On.Spear.orig_HitSomething orig, Spear self, SharedPhysics.CollisionResult result, bool eu)
        {
            double heath = 0;
            if (result.obj != null && result.obj is Player) {
                heath = (result.obj as Player).playerState.permanentDamageTracking;
            }

            bool val = orig(self, result, eu);
            if (Options.enableSpearOneShot.Value && self.thrownBy != null && self.thrownBy is Player && result.obj != null && result.obj is Player && !expetions.Contains((self.thrownBy as Player).slugcatStats.name.value) && heath > (result.obj as Player).playerState.permanentDamageTracking)
            {
                (result.obj as Player).Die();
            }
            return val;
        }
    }
}
