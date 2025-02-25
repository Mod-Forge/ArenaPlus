using ArenaPlus.Lib;
using ArenaPlus.Options.Tabs;
using ArenaPlus.Options;
using ArenaPlus.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Menu.Remix.MixedUI;
using RWCustom;

namespace ArenaPlus.Features;

[FeatureInfo(
    id: "creaturesRandomizer",
    name: "Creatures randomizer",
    description: "Randomize creature spawned",
    enabledByDefault: false
)]
file class CreaturesRandomizer : Feature
{
    public static readonly Configurable<int> randomizerTolerance = OptionsInterface.instance.config.Bind("randomizerTolerance", 0, new ConfigurableInfo("The maximum point difference allowed for a creature to override", new ConfigAcceptableRange<int>(0, 25), "", []));


    public CreaturesRandomizer(FeatureInfoAttribute featureInfo) : base(featureInfo)
    {
        SetComplementaryElement((expandable, startPos) =>
        {
            OpUpdown updown = expandable.AddItem(
                new OpUpdown(randomizerTolerance, startPos, 60f)
            );
            updown.pos -= new Vector2(0, (updown.size.y - FeaturesTab.CHECKBOX_SIZE) / 2);
            updown.description = randomizerTolerance.info.description;

            if (HexColor != "None" && ColorUtility.TryParseHtmlString("#" + HexColor, out Color color))
            {
                updown.colorEdge = color;
            }
        });
    }

    private static int[] defaultKillScores = new int[ExtEnum<MultiplayerUnlocks.SandboxUnlockID>.values.Count];
    private static List<string> exceptions = ["BigEel", "StowawayBug", "Slugcat", "SlugNPC", "Fly"];
    private static HashSet<string> spawnedCreatures = new();
    protected override void Unregister()
    {
        On.ArenaCreatureSpawner.CreateAbstractCreature -= ArenaCreatureSpawner_CreateAbstractCreature;
    }

    protected override void Register()
    {
        Menu.SandboxSettingsInterface.DefaultKillScores(ref defaultKillScores);
        On.ArenaCreatureSpawner.CreateAbstractCreature += ArenaCreatureSpawner_CreateAbstractCreature;
        // test / debug
        On.ArenaCreatureSpawner.SpawnArenaCreatures += ArenaCreatureSpawner_SpawnArenaCreatures;
        On.NoiseTracker.HeardNoise += NoiseTracker_HeardNoise;
    }

    private void NoiseTracker_HeardNoise(On.NoiseTracker.orig_HeardNoise orig, NoiseTracker self, Noise.InGameNoise noise)
    {
        try
        {
            if (!Custom.DistLess(self.AI.creature.realizedCreature.mainBodyChunk.pos, noise.pos, noise.strength * (1f - self.AI.creature.realizedCreature.Deaf) * self.hearingSkill * (1f - self.room.BackgroundNoise) * ((self.AI.creature.realizedCreature.room.PointSubmerged(noise.pos) || self.AI.creature.realizedCreature.room.GetTile(self.AI.creature.realizedCreature.mainBodyChunk.pos).DeepWater) ? 0.2f : 1f)))
            {
                return;
            }
            if (self.ignoreSeenNoises)
            {
                if (self.AI.VisualContact(noise.pos, 0f))
                {
                    return;
                }
                Tracker.CreatureRepresentation creatureRepresentation = self.tracker.RepresentationForObject(noise.sourceObject, false);
                if (creatureRepresentation == null)
                {
                    int num = 0;
                    while (num < noise.sourceObject.grabbedBy.Count && creatureRepresentation == null)
                    {
                        creatureRepresentation = self.tracker.RepresentationForObject(noise.sourceObject.grabbedBy[num].grabber, false);
                        num++;
                    }
                }
                if (creatureRepresentation != null && creatureRepresentation.VisualContact)
                {
                    return;
                }
            }
            float num2 = float.MaxValue;
            NoiseTracker.TheorizedSource theorizedSource = null;
            for (int i = 0; i < self.sources.Count; i++)
            {
                float num3 = self.sources[i].NoiseMatch(noise);
                if (num3 < num2 && num3 < ((self.sources[i].creatureRep != null) ? Custom.LerpMap((float)self.sources[i].creatureRep.TicksSinceSeen, 20f, 600f, 200f, 1000f) : 300f))
                {
                    num2 = num3;
                    theorizedSource = self.sources[i];
                }
            }
            if (theorizedSource != null)
            {
                if (theorizedSource.creatureRep == null && theorizedSource.age > 10)
                {
                    self.mysteriousNoises += self.HowInterestingIsThisNoiseToMe(noise);
                    self.mysteriousNoiseCounter = 200;
                }
                theorizedSource.Refresh(noise);
            }
            else
            {
                Tracker.CreatureRepresentation creatureRepresentation2 = null;
                num2 = float.MaxValue;
                int num4 = 0;
                for (int j = 0; j < self.tracker.CreaturesCount; j++)
                {
                    float num5 = self.NoiseMatch(noise, self.tracker.GetRep(j));
                    if (num5 < num2 && num5 < Custom.LerpMap((float)self.tracker.GetRep(j).TicksSinceSeen, 20f, 600f, 200f, 1000f))
                    {
                        num2 = num5;
                        creatureRepresentation2 = self.tracker.GetRep(j);
                    }
                    if (!self.tracker.GetRep(j).VisualContact)
                    {
                        num4++;
                    }
                }
                if (num2 > Custom.LerpMap((float)num4, 0f, (float)self.tracker.maxTrackedCreatures, 1000f, 300f))
                {
                    creatureRepresentation2 = null;
                }
                if (creatureRepresentation2 == null)
                {
                    self.mysteriousNoises += self.HowInterestingIsThisNoiseToMe(noise);
                    self.mysteriousNoiseCounter = 200;
                }
                theorizedSource = new NoiseTracker.TheorizedSource(self, noise.pos, creatureRepresentation2);
                self.sources.Add(theorizedSource);
                theorizedSource.Refresh(noise);
            }
            self.UpdateExamineSound();
            if (self.AI is IAINoiseReaction)
            {
                (self.AI as IAINoiseReaction).ReactToNoise(theorizedSource, noise);
            }
        } catch (Exception e)
        {
            LogError("NoiseTracker_HeardNoise throw", e);
            ConsoleWrite(e, Color.red);
        }
    }

    private void ArenaCreatureSpawner_SpawnArenaCreatures(On.ArenaCreatureSpawner.orig_SpawnArenaCreatures orig, RainWorldGame game, ArenaSetup.GameTypeSetup.WildLifeSetting wildLifeSetting, ref List<AbstractCreature> availableCreatures, ref MultiplayerUnlocks unlocks)
    {
        spawnedCreatures.Clear();
        orig(game, wildLifeSetting, ref availableCreatures, ref unlocks);
        //orig(game, wildLifeSetting, ref availableCreatures, ref unlocks);
        //orig(game, wildLifeSetting, ref availableCreatures, ref unlocks);
        //orig(game, wildLifeSetting, ref availableCreatures, ref unlocks);
        //orig(game, wildLifeSetting, ref availableCreatures, ref unlocks);
        LogDebug("[CreaturesRandomizer] spawned creatures", spawnedCreatures.FormatEnumarableRecursive());
    }

    private AbstractCreature ArenaCreatureSpawner_CreateAbstractCreature(On.ArenaCreatureSpawner.orig_CreateAbstractCreature orig, World world, CreatureTemplate.Type critType, WorldCoordinate pos, ref List<AbstractCreature> availableCreatures)
    {
        AbstractCreature abstractCreature = orig(world, critType, pos, ref availableCreatures);
        if (GameUtils.IsCompetitiveSession && abstractCreature != null)
        {
            // replace the creature by something that give the same amout of points
            return OverrideCreature(abstractCreature, world.game.GetArenaGameSession.arenaSitting.multiplayerUnlocks);

        }
        return abstractCreature;
    }

    private AbstractCreature OverrideCreature(AbstractCreature abstractCreature, MultiplayerUnlocks unlocks)
    {
        int origScore;
        try
        {
            origScore = defaultKillScores[GetAbstractCreatureScoreIndex(abstractCreature)];
        } catch (Exception e)
        {
            LogError("[CreaturesRandomizer] failed to get creature points", e);
            return abstractCreature;
        }

        if (origScore == 0) return abstractCreature;

        List<CreatureTemplate.Type> possibleOverrides = new List<CreatureTemplate.Type>();
        for (global::System.Int32 i = 0; i < ExtEnum<MultiplayerUnlocks.SandboxUnlockID>.values.Count; i++)
        {
            string unlockName = ExtEnum<MultiplayerUnlocks.SandboxUnlockID>.values.GetEntry(i);
            int score = defaultKillScores[i];
            if (score != 0 && Mathf.Abs(score - origScore) <= randomizerTolerance.Value)
            {
                if (ExtEnum<CreatureTemplate.Type>.values.entries.Contains(unlockName) && unlocks.IsCreatureUnlockedForLevelSpawn(new CreatureTemplate.Type(unlockName, false)) && !exceptions.Contains(unlockName))
                {
                    LogDebug("adding", unlockName, "to possible creature overrides");
                    possibleOverrides.Add(new CreatureTemplate.Type(unlockName, false));
                }
            }
        }

        if (possibleOverrides.Count == 0) return abstractCreature;

        CreatureTemplate.Type type = possibleOverrides[Random.Range(0, possibleOverrides.Count)];
        AbstractCreature newAbstCreature = new AbstractCreature(abstractCreature.world, StaticWorld.GetCreatureTemplate(type), null, abstractCreature.pos, abstractCreature.world.game.GetNewID());
        //abstractCreature.Destroy();
        if (Random.value > 0.35f)
        {
            abstractCreature.Die();
            abstractCreature.slatedForDeletion = true;
        }

        try
        {
            int newScore = defaultKillScores[GetAbstractCreatureScoreIndex(newAbstCreature)];
            LogDebug("[CreaturesRandomizer] overriding", abstractCreature.creatureTemplate.name, "by", newAbstCreature.creatureTemplate.name, "with a diference of", Mathf.Abs(newScore - origScore), "/", randomizerTolerance.Value);
        } catch (Exception e)
        {
            LogDebug("[CreaturesRandomizer] overriding", abstractCreature.creatureTemplate.name, "by", newAbstCreature.creatureTemplate.name, "with a diference of Error");
            LogError("[CreaturesRandomizer] failed to get override points", e);
        }
        SpecialCase(abstractCreature);
        spawnedCreatures.Add(type.value);
        return newAbstCreature;
    }

    private void SpecialCase(AbstractCreature abstractCreature)
    {
        AbstractRoom abstractRoom = abstractCreature.world.GetAbstractRoom(0);
        if (abstractCreature.creatureTemplate.type == CreatureTemplate.Type.YellowLizard || abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Scavenger)
        {
            for (global::System.Int32 i = 0; i < Random.Range(0, 4); i++)
            {
               abstractRoom.MoveEntityToDen(new AbstractCreature(abstractCreature.world, abstractCreature.creatureTemplate, null, abstractCreature.pos, abstractCreature.world.game.GetNewID()));
            }
        }
    }

    private int GetAbstractCreatureScoreIndex(AbstractCreature abstractCreature)
    {
        return MultiplayerUnlocks.SandboxUnlockForSymbolData(CreatureSymbol.SymbolDataFromCreature(abstractCreature)).index;
    }
}

