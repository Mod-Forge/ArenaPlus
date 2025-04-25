using ArenaPlus.Lib;
using ArenaPlus.Utils;
using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArenaPlus.Features.Slugcats
{
    [SlugcatFeatureInfo(
        id: "saintBatflyPop",
        name: "Saint pop batfly",
        description: "Allow the Saint to pop batflies... wait, they can already do that...",
        slugcat: "Saint"
    )]
    file class SaintBatflyPop(SlugcatFeatureInfoAttribute featureInfo) : SlugcatFeature(featureInfo)
    {
        protected override void Unregister()
        {
            On.Player.TossObject -= Player_TossObject;
            On.Fly.Collide -= Fly_Collide;
            On.Creature.TerrainImpact -= Creature_TerrainImpact;
        }

        protected override void Register()
        {
            On.Player.TossObject += Player_TossObject;
            On.Fly.Collide += Fly_Collide;
            On.Creature.TerrainImpact += Creature_TerrainImpact;
        }

        private void Creature_TerrainImpact(On.Creature.orig_TerrainImpact orig, Creature self, int chunk, IntVector2 direction, float speed, bool firstContact)
        {
            orig(self, chunk, direction, speed, firstContact);
            if (self is not Fly fly || !GameUtils.IsCompetitiveOrSandboxSession) return;

            if (!tossBy.TryGetValue(fly, out var player)) return;

            LogUnity("tossed batfly collide");
            if (self.grabbedBy.Count == 0 && self.Stunned)
            {
                self.room.PlaySound(SoundID.Snail_Pop, self.mainBodyChunk, false, 1f, 1.5f + UnityEngine.Random.value);
                self.room.AddObject(new ShockWave(self.firstChunk.pos, 25f, 0.8f, 4, false));
                for (int l = 0; l < 5; l++)
                {
                    self.room.AddObject(new Spark(self.firstChunk.pos, Custom.RNV() * 3f, Color.yellow, null, 25, 90));
                }
                self.Destroy();
                self.abstractPhysicalObject.Destroy();
                self.room.AddObject(new Explosion(self.room, self, self.mainBodyChunk.pos, 2, 100f, 4.2f, 0f, 40f, 0f, player, 0.7f, 20f, 1f));
            }
            tossBy.Remove(fly);
        }

        private void Fly_Collide(On.Fly.orig_Collide orig, Fly self, PhysicalObject otherObject, int myChunk, int otherChunk)
        {
            orig(self, otherObject, myChunk, otherChunk);


        }

        ConditionalWeakTable<Fly, Player> tossBy = new ConditionalWeakTable<Fly, Player>();
        private void Player_TossObject(On.Player.orig_TossObject orig, Player self, int grasp, bool eu)
        {
            orig(self, grasp, eu);
            LogUnity("tossed object");
            if (GameUtils.IsCompetitiveOrSandboxSession && self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint && self.grasps[grasp].grabbed is Fly fly)
            {
                LogUnity("tossed batfly");
                if (!tossBy.TryGetValue(fly, out _))
                {
                    LogUnity("tossed batfly saved");
                    tossBy.Add(fly, self);
                    fly.Stun(100);
                }
            }
        }
    }
}