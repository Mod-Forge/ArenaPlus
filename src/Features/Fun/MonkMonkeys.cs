using ArenaPlus.Utils;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MoreSlugcats;
using ArenaPlus.Lib;

namespace ArenaPlus.Features.Fun
{
    [FeatureInfo(
        id: "monkMonkeys",
        name: "Monk Monkeys",
        category: BuiltInCategory.Spoilers,
        description: "Make all scavengers believe in karma (Watcher spoiler)",
        requireDLC: [DLCIdentifiers.Watcher],
        enabledByDefault: false
    )]
    public class MonkMonkeys(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
    {
        protected override void Unregister()
        {
            On.AbstractCreature.ctor -= AbstractCreature_ctor;
        }
        protected override void Register()
        {
            On.AbstractCreature.ctor += AbstractCreature_ctor;
            On.ScavengerAI.ctor += ScavengerAI_ctor; ;
        }

        private void ScavengerAI_ctor(On.ScavengerAI.orig_ctor orig, ScavengerAI self, AbstractCreature creature, World world)
        {
            orig(self, creature, world);
            if (GameUtils.IsCompetitiveSession && self.scavenger.Templar && !self.scavenger.Disciple)
            {
                self.bloodLustAggressionThreshold = 0;
            }
        }

        private void AbstractCreature_ctor(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
        {
            if (GameUtils.IsCompetitiveSession)
            {
                if (creatureTemplate.type == CreatureTemplate.Type.Scavenger)
                {
                    if (Random.value > 0.25f)
                    {
                        creatureTemplate = StaticWorld.GetCreatureTemplate(Watcher.WatcherEnums.CreatureTemplateType.ScavengerTemplar);
                    }
                    else
                    {
                        creatureTemplate = StaticWorld.GetCreatureTemplate(Watcher.WatcherEnums.CreatureTemplateType.ScavengerDisciple);
                    }
                }
                else if (creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.ScavengerElite)
                {
                    creatureTemplate = StaticWorld.GetCreatureTemplate(Watcher.WatcherEnums.CreatureTemplateType.ScavengerDisciple);
                }
            }
            orig(self, world, creatureTemplate, realizedCreature, pos, ID);
        }
    }
}
