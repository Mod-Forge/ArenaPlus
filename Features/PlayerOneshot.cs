
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ArenaSlugcatsConfigurator.Freatures
{
    internal static class PlayerOneshot
    {
        public static string[] expetions = new string[1]
        {
            "Gourmand"
        };

        public static ConditionalWeakTable<Weapon, Creature> killtags = new ConditionalWeakTable<Weapon, Creature>();

        internal static void Register()
        {
            logSource.LogInfo("PlayerOneshot Register");
            On.Spear.HitSomething += Spear_HitSomething;
            On.Weapon.Update += Weapon_Update;
        }

        private static void Weapon_Update(On.Weapon.orig_Update orig, Weapon self, bool eu)
        {
            orig(self, eu);
            if (self.thrownBy != null)
            {
                if (killtags.TryGetValue(self, out var killTag) && killTag != self.thrownBy)
                {
                    killtags.Remove(self);
                }

                if ((killTag == null || killTag != self.thrownBy) && self.thrownBy is Player)
                {
                    killtags.Add(self, self.thrownBy as Player);
                }
            }
        }

        private static bool Spear_HitSomething(On.Spear.orig_HitSomething orig, Spear self, SharedPhysics.CollisionResult result, bool eu)
        {
            double damage = 0;
            if (result.obj != null && result.obj is Player) {
                ConsoleWrite("spear hiting a player");
                damage = (result.obj as Player).playerState.permanentDamageTracking;
            }

            bool val = orig(self, result, eu);
            if (result.obj != null && result.obj is Player)  ConsoleWrite("damage taked: " + ((result.obj as Player).playerState.permanentDamageTracking - damage));

            self.thrownBy ??= killtags.TryGetValue(self, out var killTag) ? killTag : null;
            if (Options.enableSpearOneShot.Value && self.thrownBy != null && self.thrownBy is Player && result.obj != null && result.obj is Player && !expetions.Contains((self.thrownBy as Player).slugcatStats.name.value) && damage < (result.obj as Player).playerState.permanentDamageTracking)
            {
                ConsoleWrite("force kill player");
                (result.obj as Player).Die();
            }
            return val;
        }
    }
}
