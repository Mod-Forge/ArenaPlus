using ArenaPlus.Lib;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArenaPlus.Features.Slugcats
{
    [ImmutableFeature]
    file class InvBellyBomb : ImmutableFeature
    {
        private List<AbstractPhysicalObject> arenaEggs = [];

        protected override void Register()
        {
            On.ArenaGameSession.SpawnPlayers += ArenaGameSession_SpawnPlayers;
            On.MoreSlugcats.SingularityBomb.ctor += SingularityBomb_ctor;
        }

        private void SingularityBomb_ctor(On.MoreSlugcats.SingularityBomb.orig_ctor orig, SingularityBomb self, AbstractPhysicalObject abstractPhysicalObject, World world)
        {
            orig(self, abstractPhysicalObject, world);

            if (arenaEggs.Contains(abstractPhysicalObject))
            {
                self.zeroMode = true;
                self.explodeColor = new Color(1f, 0.2f, 0.2f);
                self.connections = [];
                self.holoShape = null;
            }
        }

        private void ArenaGameSession_SpawnPlayers(On.ArenaGameSession.orig_SpawnPlayers orig, ArenaGameSession self, Room room, List<int> suggestedDens)
        {
            orig(self, room, suggestedDens);
            List<AbstractPhysicalObject> eggs = [];
            for (int i = 0; i < self.Players.Count; i++)
            {
                Player player = (Player)self.Players[i].realizedCreature;
                if (player.SlugCatClass.ToString() == "Inv")
                {
                    AbstractPhysicalObject singularityBomb = new(room.world, MoreSlugcatsEnums.AbstractObjectType.SingularityBomb, null, self.Players[i].pos, room.game.GetNewID());
                    eggs.Add(singularityBomb);

                    player.objectInStomach = singularityBomb;
                    player.objectInStomach.Abstractize(player.abstractCreature.pos);
                }
            }
            arenaEggs = eggs;
        }
    }
}
