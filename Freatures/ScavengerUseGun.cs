using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using ScavengerCosmetic;
using Unity.Mathematics;
using UnityEngine;

namespace ArenaSlugcatsConfigurator.Freatures
{
    internal static class ScavengerUseGun
    {
        internal static void OnEnable()
        {
            logSource.LogInfo("ScavengerUseGun OnEnable");
            On.Scavenger.Throw += Scavenger_Throw;
            On.JokeRifle.Update += JokeRifle_Update;
            On.ScavengerAI.WeaponScore += ScavengerAI_WeaponScore;
            On.ScavengerAI.RealWeapon += ScavengerAI_RealWeapon;

        }

        private static bool ScavengerAI_RealWeapon(On.ScavengerAI.orig_RealWeapon orig, ScavengerAI self, PhysicalObject obj)
        {
            return orig(self, obj) || obj is JokeRifle;
        }

        private static int ScavengerAI_WeaponScore(On.ScavengerAI.orig_WeaponScore orig, ScavengerAI self, PhysicalObject obj, bool pickupDropInsteadOfWeaponSelection)
        {
            if (obj is JokeRifle)
            {
                return (obj as JokeRifle).abstractRifle.currentAmmo() > 0 ? 10 : 0;
            }
            return orig(self, obj, pickupDropInsteadOfWeaponSelection);
        }

        private static void JokeRifle_Update(On.JokeRifle.orig_Update orig, JokeRifle self, bool eu)
        {
            orig(self, eu);
            if (self.grabbedBy.Count > 0 && self.grabbedBy[0].grabber is Scavenger)
            {
                Scavenger scav = (self.grabbedBy[0].grabber as Scavenger);
                if (scav.graphicsModule != null && scav.grasps[0] != null && scav.grasps[0].grabbed is JokeRifle)
                {
                    //(scav.grasps[0].grabbed as JokeRifle).aimDir = Custom.DirVec(scav.grasps[0].grabbed.firstChunk.pos, new float2((scav.graphicsModule as ScavengerGraphics).hands[0].pos.x, (scav.graphicsModule as ScavengerGraphics).hands[0].pos.y));
                    //(scav.grasps[0].grabbed as JokeRifle).aimDir = new Vector2((scav.grasps[0].grabbed as JokeRifle).aimDir.x, Math.Abs((scav.grasps[0].grabbed as JokeRifle).aimDir.y));
                    (scav.grasps[0].grabbed as JokeRifle).aimDir = new Vector2(-scav.firstChunk.Rotation.x, scav.firstChunk.Rotation.y);
                }
            }
        }

        private static void Scavenger_Throw(On.Scavenger.orig_Throw orig, Scavenger self, Vector2 throwDir)
        {
            if (self.grasps[0].grabbed is JokeRifle && (self.grasps[0].grabbed as JokeRifle).abstractRifle.currentAmmo() > 0)
            {
                JokeRifle rifle = (JokeRifle)self.grasps[0].grabbed;
                rifle.aimDir = throwDir;
                rifle.Use(rifle.evenUpdate);
            }
            else
            {
                orig(self, throwDir);
            }
        }
    }
}
