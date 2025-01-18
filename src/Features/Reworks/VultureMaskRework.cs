using ArenaPlus.Utils;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArenaPlus.Features.Reworks
{
    [FeatureInfo(
        id: "vultureMaskRework",
        name: "Vulture mask block",
                category: "Reworks",
        description: "Wether vulture masks block projectiles before breaking",
        enabledByDefault: true
    )]
    file class VultureMaskRework(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
    {
        protected override void Register()
        {
            On.Spear.HitSomething += Spear_HitSomething;
        }

        protected override void Unregister()
        {
            On.Spear.HitSomething -= Spear_HitSomething;
        }

        private bool Spear_HitSomething(On.Spear.orig_HitSomething orig, Spear self, SharedPhysics.CollisionResult result, bool eu)
        {
            if (self.room.game.IsStorySession) return orig(self, result, eu);

            if (result.obj is not null and Creature and Player player)
            {
                for (int i = 0; i < player.grasps.Length; i++)
                {
                    if (player.grasps[i] != null)
                    {
                        if (player.grasps[i].grabbed is VultureMask vultureMask && vultureMask.donned > 0)
                        {
                            if (!vultureMask.King || Random.value > 0.33333)
                            {
                                for (int n = 17; n > 0; n--)
                                {
                                    vultureMask.room.AddObject(new Spark(vultureMask.firstChunk.pos, Custom.RNV() * 2, Color.white, null, 10, 20));
                                }
                                vultureMask.Destroy();
                            }
                            self.room.PlaySound(SoundID.Spear_Bounce_Off_Creauture_Shell, self.firstChunk);
                            self.vibrate = 20;
                            self.ChangeMode(Weapon.Mode.Free);
                            self.firstChunk.vel = self.firstChunk.vel * -0.5f + Custom.DegToVec(Random.value * 360f) * Mathf.Lerp(0.1f, 0.4f, Random.value) * self.firstChunk.vel.magnitude;
                            self.SetRandomSpin();
                            return false;
                        }
                    }
                }
            }
            return orig(self, result, eu);
        }
    }
}
