using ArenaPlus.Lib;
using ArenaPlus.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArenaPlus.Features.Reworks
{
    [FeatureInfo(
        id: "jokeRifleRework",
        name: "Joke rifle rework",
        category: BuiltInCategory.Spoilers,
        description: "Wheter Scavengers can use the Joke Rifle (Challenge spoiler)",
        enabledByDefault: true
    )]
    file class JokeRifleRework(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
    {
        protected override void Register()
        {
            On.JokeRifle.Use += JokeRifle_Use;
            On.JokeRifle.Update += JokeRifle_Update;
            On.MoreSlugcats.AbstractBullet.ctor += AbstractBullet_ctor;
            On.Scavenger.Throw += Scavenger_Throw;
            On.ScavengerAI.WeaponScore += ScavengerAI_WeaponScore;
            On.ScavengerAI.RealWeapon += ScavengerAI_RealWeapon;
        }

        private bool ScavengerAI_RealWeapon(On.ScavengerAI.orig_RealWeapon orig, ScavengerAI self, PhysicalObject obj)
        {
            return orig(self, obj) || obj is JokeRifle;
        }

        private int ScavengerAI_WeaponScore(On.ScavengerAI.orig_WeaponScore orig, ScavengerAI self, PhysicalObject obj, bool pickupDropInsteadOfWeaponSelection)
        {
            if (obj is JokeRifle)
            {
                return (obj as JokeRifle).abstractRifle.currentAmmo() > 0 ? 10 : 0;
            }
            return orig(self, obj, pickupDropInsteadOfWeaponSelection);
        }

        private void Scavenger_Throw(On.Scavenger.orig_Throw orig, Scavenger self, Vector2 throwDir)
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

        protected override void Unregister()
        {
            On.JokeRifle.Use -= JokeRifle_Use;
            On.MoreSlugcats.AbstractBullet.ctor -= AbstractBullet_ctor;
        }

        private void AbstractBullet_ctor(On.MoreSlugcats.AbstractBullet.orig_ctor orig, MoreSlugcats.AbstractBullet self, World world, MoreSlugcats.Bullet realizedObject, WorldCoordinate pos, EntityID ID, JokeRifle.AbstractRifle.AmmoType type, int timeToLive)
        {
            orig(self, world, realizedObject, pos, ID, type, timeToLive);

            if (GameUtils.IsCompetitiveOrSandboxSession)
            {
                self.timeToLive = 40;
            }
        }

        private void JokeRifle_Update(On.JokeRifle.orig_Update orig, JokeRifle self, bool eu)
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

        private void JokeRifle_Use(On.JokeRifle.orig_Use orig, JokeRifle self, bool eu)
        {
            if (GameUtils.IsCompetitiveOrSandboxSession && self.counter < 1 && self.abstractRifle.currentAmmo() > 0 && self.abstractRifle.ammoStyle == JokeRifle.AbstractRifle.AmmoType.Pearl)
            {
                Vector2 pos = self.firstChunk.pos + self.aimDir;
                Vector2 dir = pos + self.aimDir * 200f;
                SharedPhysics.CollisionResult collisionResult = SharedPhysics.TraceProjectileAgainstBodyChunks(null, self.room, pos, ref dir, 1f, 1, (self.grabbedBy[0].grabber as PhysicalObject), false);
                if (collisionResult.chunk != null)
                {
                    AbstractPhysicalObject abtractObject = collisionResult.chunk.owner.abstractPhysicalObject;
                    bool shoot = true;

                    string text = "result: " + abtractObject.type;

                    if (abtractObject is AbstractCreature)
                    {
                        text += " " + (abtractObject as AbstractCreature).creatureTemplate.name;
                        if (abtractObject == self.abstractPhysicalObject && ((abtractObject as AbstractCreature).creatureTemplate.type == CreatureTemplate.Type.Slugcat && self.grabbedBy[0].grabber == (abtractObject.realizedObject as Creature)))
                        {
                            shoot = false;
                            text += " (shoot ower/self)";
                        }
                    }

                    if (shoot)
                    {
                        float falloff = Vector2.Distance(pos, collisionResult.chunk.pos) / 20;
                        abtractObject.realizedObject.firstChunk.vel = self.aimDir * 100f / falloff;
                    }
                }
            }
            orig(self, eu);
        }
    }
}
