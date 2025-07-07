using ArenaPlus.Lib;
using ArenaPlus.Utils;
using Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ArenaPlus.Features
{
    [ImmutableFeature]
    public class ScavengerMindControl : ImmutableFeature
    {
        private static ConditionalWeakTable<Scavenger, AbstractCreature> scavengerPlayer = new();
        protected override void Register()
        {
            On.ScavengerAI.PlayerRelationship += ScavengerAI_PlayerRelationship;
        }

        private CreatureTemplate.Relationship ScavengerAI_PlayerRelationship(On.ScavengerAI.orig_PlayerRelationship orig, ScavengerAI self, RelationshipTracker.DynamicRelationship dRelation)
        {
            var val = orig(self, dRelation);
            if (scavengerPlayer.TryGetValue(self.scavenger, out AbstractCreature abstPlayer) && dRelation?.trackerRep?.representedCreature == abstPlayer)
            {
                return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Pack, 1f);
            }
            return val;
        }

        public static void ForceLove(Scavenger scavenger, AbstractCreature abstPlayer)
        {
            if (scavengerPlayer.TryGetValue(scavenger, out _))
            {
                scavengerPlayer.Remove(scavenger);
            }

            scavengerPlayer.Add(scavenger, abstPlayer);
        }
    }
}
