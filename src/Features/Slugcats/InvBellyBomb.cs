using ArenaPlus.Lib;
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
        private HashSet<WeakReference<AbstractPhysicalObject>> arenaEggs = [];

        protected override void Register()
        {
            On.ArenaGameSession.SpawnPlayers += ArenaGameSession_SpawnPlayers;
            On.MoreSlugcats.SingularityBomb.ctor += SingularityBomb_ctor;
        }

        private void SingularityBomb_ctor(On.MoreSlugcats.SingularityBomb.orig_ctor orig, MoreSlugcats.SingularityBomb self, AbstractPhysicalObject abstractPhysicalObject, World world)
        {
            orig(self, abstractPhysicalObject, world);
            if (!ModManager.MSC) return;
            if (arenaEggs.Any(wao => wao.TryGetTarget(out var ao) && ao == abstractPhysicalObject))
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
            if (!ModManager.MSC) return;
            for (int i = 0; i < self.Players.Count; i++)
            {
                Player player = (Player)self.Players[i].realizedCreature;
                if (player.SlugCatClass.ToString() == "Inv")
                {
                    AbstractPhysicalObject singularityBomb = new(room.world, DLCSharedEnums.AbstractObjectType.SingularityBomb, null, self.Players[i].pos, room.game.GetNewID());
                    player.room.abstractRoom.AddEntity(singularityBomb);
                    if (arenaEggs.Any(wao => wao.TryGetTarget(out var ao) && ao == singularityBomb))
                    {
                        arenaEggs.Add(new WeakReference<AbstractPhysicalObject>(singularityBomb));
                    }

                    player.objectInStomach = singularityBomb;
                    player.objectInStomach.Abstractize(player.abstractCreature.pos);
                }
            }
        }
    }
}
