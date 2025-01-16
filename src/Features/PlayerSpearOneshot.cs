using ArenaPlus.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ArenaPlus.Features
{
    [FeatureInfo(
        id: "playerSpearOneshot",
        name: "Player spear oneshot",
        description: "Whether spears throwed by players oneshot other slugcats",
        enabledByDefault: false
    )]
    file class PlayerSpearOneshot(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
    {
        private readonly string[] exceptions = ["Gourmand"];

        private readonly ConditionalWeakTable<Weapon, Creature> killtags = new();

        protected override void Register()
        {
            On.Spear.HitSomething += Spear_HitSomething;
            On.Weapon.Update += Weapon_Update;
        }

        protected override void Unregister()
        {
            On.Spear.HitSomething -= Spear_HitSomething;
            On.Weapon.Update -= Weapon_Update;
        }

        private void Weapon_Update(On.Weapon.orig_Update orig, Weapon self, bool eu)
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

        private bool Spear_HitSomething(On.Spear.orig_HitSomething orig, Spear self, SharedPhysics.CollisionResult result, bool eu)
        {
            double damage = 0;
            if (result.obj != null && result.obj is Player)
            {
                ConsoleWrite("spear hiting a player");
                damage = (result.obj as Player).playerState.permanentDamageTracking;
            }

            bool val = orig(self, result, eu);
            if (result.obj != null && result.obj is Player) ConsoleWrite("damage taked: " + ((result.obj as Player).playerState.permanentDamageTracking - damage));

            self.thrownBy ??= killtags.TryGetValue(self, out var killTag) ? killTag : null;
            if (self.thrownBy != null && self.thrownBy is Player && result.obj != null && result.obj is Player && !exceptions.Contains((self.thrownBy as Player).slugcatStats.name.value) && damage < (result.obj as Player).playerState.permanentDamageTracking)
            {
                ConsoleWrite("force kill player");
                (result.obj as Player).Die();
            }
            return val;
        }
    }
}
