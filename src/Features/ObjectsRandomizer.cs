using ArenaPlus.Lib;
using ArenaPlus.Utils;
using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Util;
using UnityEngine;

namespace ArenaPlus.Features
{
    [FeatureInfo(
        id: "objectsRandomizer",
        name: "Objects randomizer",
        description: "Whether objects in arena are random",
        enabledByDefault: false
    )]
    public class ObjectsRandomizer(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
    {
        protected override void Unregister()
        {
            On.Room.Loaded -= Room_Loaded;
        }

        protected override void Register()
        {
            On.Room.Loaded += Room_Loaded;
            InitRandomObjects();
        }

        bool _initialized;
        public void InitRandomObjects()
        {
            if (_initialized) return;
            _initialized = true;

            LogInfo("random object initialized");
            var explosiveSpear = new SpearRandomObject(SpearRandomObject.Type.Explosive, 4);
            LogInfo("explosive spear rare", explosiveSpear.rare);

            AddRangeRandomObject([
                new SpearRandomObject(SpearRandomObject.Type.Basic, 20),
                new SpearRandomObject(SpearRandomObject.Type.Explosive, 5),
                new SpearRandomObject(SpearRandomObject.Type.Electric, 5),

                //      MSC      //
                new SpearRandomObject(SpearRandomObject.Type.Fire, 5),
                new RifleRandomObject(null, null, 5),

                //      SHARED      //
                new SpearRandomObject(SpearRandomObject.Type.Poison, 10),
            ], true);

            AddRangeRandomObject([
                // TubeWorm 20
                new RandomObject(AbstractPhysicalObject.AbstractObjectType.Rock, 20),
                new RandomObject(AbstractPhysicalObject.AbstractObjectType.Rock, 20),
                new RandomObject(AbstractPhysicalObject.AbstractObjectType.FlareBomb, 20),
                new RandomObject(AbstractPhysicalObject.AbstractObjectType.Mushroom, 20),
                new RandomObject(AbstractPhysicalObject.AbstractObjectType.JellyFish, 20),
                new RandomObject(AbstractPhysicalObject.AbstractObjectType.PuffBall, 20),
                new RandomObject(AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, 20),
                // VultureGrub 10
                new RandomObject(AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, 10),
                new RandomObject(AbstractPhysicalObject.AbstractObjectType.VultureMask, 10),

                //      MSC      //
                new RandomObject(MoreSlugcatsEnums.AbstractObjectType.FireEgg, 5),
                //new randomObject(MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType.MoonCloak, 5), // don't work
                new RandomObject(MoreSlugcatsEnums.AbstractObjectType.EnergyCell, 5),
                new RandomObject(AbstractPhysicalObject.AbstractObjectType.KarmaFlower, 5),

                //      WATCHER      //
                new RandomObject(Watcher.WatcherEnums.AbstractObjectType.Boomerang, 20),

                //      SHARED      //
                new RandomObject(DLCSharedEnums.AbstractObjectType.SingularityBomb, 1),
                new RandomObject(AbstractPhysicalObject.AbstractObjectType.KarmaFlower, 5),
            ], false);
        }

        public static void AddRandomObject(RandomObject obj, bool replaceWeapon = false)
        {
            AddRangeRandomObject([obj], replaceWeapon);
        }

        public static void AddRangeRandomObject(RandomObject[] objects, bool replaceWeapon = false)
        {
            if (replaceWeapon)
                weaponsOverrides.AddRange(objects);
            else
                objectsOverrides.AddRange(objects);
        }

        private static List<RandomObject> objectsOverrides = new List<RandomObject>();
        private static List<RandomObject> weaponsOverrides = new List<RandomObject>();

        private void Room_Loaded(On.Room.orig_Loaded orig, Room self)
        {
            orig(self);
            if (!GameUtils.IsCompetitiveSession) return;

            var filteredObjectsOverrides = FilterObjectsList(objectsOverrides);
            var filteredWeaponsOverrides = FilterObjectsList(weaponsOverrides);


            List<AbstractPhysicalObject> addObjects = new List<AbstractPhysicalObject>();
            for (int i = 0; i < self.abstractRoom.entities.Count; i++)
            {
                if (self.abstractRoom.entities[i] is not AbstractPhysicalObject obj) continue;

                AbstractPhysicalObject newObject = null;
                bool destroy = false;

                if (obj.type == AbstractPhysicalObject.AbstractObjectType.Spear)
                {
                    RandomObject randObj = GetRandomObject(filteredWeaponsOverrides);

                    // rare override
                    if ((obj as AbstractSpear).explosive || (obj as AbstractSpear).electric || (obj as AbstractSpear).hue != 0f)
                    {
                        randObj = GetRandomObject(filteredWeaponsOverrides.Where(ro => ro.rare).ToList());
                    }


                    newObject = MakeAbstractPhysicalObject(randObj, self, obj.pos);

                    if (Random.value < 0.5f)
                        destroy = true;
                }
                else if (obj.type == AbstractPhysicalObject.AbstractObjectType.Rock || obj.type == AbstractPhysicalObject.AbstractObjectType.DataPearl)
                {
                    RandomObject randObj = GetRandomObject(filteredObjectsOverrides);

                    newObject = MakeAbstractPhysicalObject(randObj, self, obj.pos);

                    if (Random.value < 0.6f)
                        destroy = true;
                }



                if (newObject != null)
                {
                    if (destroy)
                    {
                        LogDebug($"Object {obj.type} destroyed");
                        obj.Destroy();
                        self.abstractRoom.entities[i] = newObject;
                    }
                    else
                    {
                        addObjects.Add(newObject);
                    }
                    LogDebug($"Replaced object {obj.type} {i} by {newObject.type}");
                    LogDebug("======================\n");
                }
            }

            self.abstractRoom.entities.AddRange(addObjects);
        }

        private RandomObject GetRandomObject(List<RandomObject> list)
        {
            if (list.Count == 0) throw new Exception("List is empty");
            try
            {
                int sum = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    sum += list[i].chance;
                }

                int rand = Random.Range(0, sum);

                sum = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    sum += list[i].chance;
                    if (sum > rand)
                    {
                        //LogDebug("Type Generated");
                        return list[i];
                    }
                }
                return list[Random.Range(0, list.Count)];
            }
            catch (Exception e)
            {
                LogInfo("list", list.FormatEnumarable());
                throw new Exception("get random object failed", e);
            }
        }

        private List<RandomObject> FilterObjectsList(List<RandomObject> list)
        {
            return list.Where(randomObject => randomObject.type != null && randomObject.Unlocked()).ToList();
        }

        private static AbstractPhysicalObject MakeAbstractPhysicalObject(RandomObject randObj, Room room, WorldCoordinate pos)
        {
            if (randObj.type == null)
                return null;
            EntityID entityID = room.game.GetNewID();


            if (randObj is SpearRandomObject spearRandObj)
            {
                if (((int)spearRandObj.spearType) <= 2)
                {
                    return new AbstractSpear(room.world, null, pos, entityID, spearRandObj.spearType == SpearRandomObject.Type.Explosive, spearRandObj.spearType == SpearRandomObject.Type.Electric);
                }
                else if (ModManager.MSC && spearRandObj.spearType == SpearRandomObject.Type.Fire)
                {
                    return new AbstractSpear(room.world, null, pos, entityID, false, Mathf.Lerp(0.35f, 0.6f, Custom.ClampedRandomVariation(0.5f, 0.5f, 2f)));

                }
                else if (ModManager.Watcher && spearRandObj.spearType == SpearRandomObject.Type.Poison)
                {
                    AbstractPhysicalObject newObject = new AbstractSpear(room.world, null, pos, entityID, false);
                    (newObject as AbstractSpear).poison = 1.7f;
                    (newObject as AbstractSpear).poisonHue = 0.3f + UnityEngine.Random.value * 0.6f;
                    return newObject;
                }

                return new AbstractSpear(room.world, null, pos, entityID, false);
            }

            if (randObj is RifleRandomObject rifleRandObj)
            {
                JokeRifle.AbstractRifle.AmmoType ammo = new JokeRifle.AbstractRifle.AmmoType(ExtEnum<JokeRifle.AbstractRifle.AmmoType>.values.entries[Random.Range(0, ExtEnum<JokeRifle.AbstractRifle.AmmoType>.values.entries.Count)]);
                if (rifleRandObj.bulletType != null)
                {
                    ammo = rifleRandObj.bulletType;
                }
                
                AbstractPhysicalObject newObject = new JokeRifle.AbstractRifle(room.world, null, pos, entityID, ammo);
                (newObject as JokeRifle.AbstractRifle).setCurrentAmmo(rifleRandObj.bulletAmount.HasValue ? rifleRandObj.bulletAmount.Value : (int)Random.Range(5, 40));
                return newObject;
            }

            if (randObj.type == AbstractPhysicalObject.AbstractObjectType.VultureMask)
            {
                return new VultureMask.AbstractVultureMask(room.world, null, pos, entityID, entityID.RandomSeed, false);
            }
            if (randObj.type == AbstractPhysicalObject.AbstractObjectType.DataPearl)
            {
                return new DataPearl.AbstractDataPearl(room.world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, pos, entityID, -1, -1, null, new DataPearl.AbstractDataPearl.DataPearlType(ExtEnum<DataPearl.AbstractDataPearl.DataPearlType>.values.entries[Random.Range(0, ExtEnum<DataPearl.AbstractDataPearl.DataPearlType>.values.entries.Count)], false));
            }
            if (randObj.type == MoreSlugcatsEnums.AbstractObjectType.FireEgg)
            {
                return new FireEgg.AbstractBugEgg(room.world, null, pos, entityID, Mathf.Lerp(0.35f, 0.6f, Custom.ClampedRandomVariation(0.5f, 0.5f, 2f)));
            }
            if (randObj.type == MoreSlugcatsEnums.AbstractObjectType.MoonCloak)
            {
                return new AbstractConsumable(room.world, MoreSlugcatsEnums.AbstractObjectType.MoonCloak, null, pos, entityID, -1, -1, null);
            }
            if (AbstractConsumable.IsTypeConsumable(randObj.type))
            {
                return new AbstractConsumable(room.world, randObj.type, null, pos, entityID, -1, -1, null);
            }

            return new AbstractPhysicalObject(room.world, randObj.type, null, pos, entityID);
        }

        private static bool SandboxItemUnlocked(MultiplayerUnlocks.SandboxUnlockID unlockID) => GameUtils.rainWorldGame.GetArenaGameSession.arenaSitting.multiplayerUnlocks.SandboxItemUnlocked(unlockID);

        public class RandomObject(AbstractPhysicalObject.AbstractObjectType type, int chance, bool rare = false)
        {
            public AbstractPhysicalObject.AbstractObjectType type = type;
            public int chance = chance;
            public bool rare = rare;

            public virtual bool Unlocked()
            {
                MultiplayerUnlocks.SandboxUnlockID unlockID = null;
                if (MultiplayerUnlocks.SandboxUnlockID.values.entries.Contains(type.value))
                {
                    unlockID = new MultiplayerUnlocks.SandboxUnlockID(type.value, false);
                }

                return type != null && (unlockID == null || SandboxItemUnlocked(unlockID));
            }

            public virtual AbstractPhysicalObject CreateAbstractPhysicalObject(Room room, WorldCoordinate pos)
            {
                return MakeAbstractPhysicalObject(this ,  room,  pos);
            }

            public override string ToString()
            {
                return $"{this.GetType().Name} {type.value} chance:{chance} rare:{rare}";
            }
        }

        public class SpearRandomObject(SpearRandomObject.Type type, int chance) : RandomObject(AbstractPhysicalObject.AbstractObjectType.Spear, chance, type != SpearRandomObject.Type.Basic && type != SpearRandomObject.Type.Poison)
        {
            public SpearRandomObject.Type spearType = type;
            public enum Type
            {
                Basic,
                Explosive,
                Electric,
                Fire,
                Poison
            }

            public override bool Unlocked() => spearType switch
            {
                Type.Explosive => SandboxItemUnlocked(MultiplayerUnlocks.SandboxUnlockID.FireSpear),
                Type.Electric => ModManager.MSC && SandboxItemUnlocked(MoreSlugcatsEnums.SandboxUnlockID.ElectricSpear),
                Type.Fire => ModManager.MSC && SandboxItemUnlocked(MoreSlugcatsEnums.SandboxUnlockID.HellSpear),
                Type.Poison => ModManager.Watcher && true,
                _ => true,
            };

            public override string ToString()
            {
                return $"{this.GetType().Name} Spear.{spearType} chance:{chance} rare:{rare}";
            }
        }

        public class RifleRandomObject(JokeRifle.AbstractRifle.AmmoType bulletType, int? bulletAmount, int chance) : RandomObject(new AbstractPhysicalObject.AbstractObjectType("JokeRifle", false), chance, true)
        {
            public JokeRifle.AbstractRifle.AmmoType bulletType = bulletType;
            public int? bulletAmount = bulletAmount;

            public override bool Unlocked()
            {
                return ModManager.MSC && SandboxItemUnlocked(MultiplayerUnlocks.SandboxUnlockID.FireSpear);
            }

            public override string ToString()
            {
                return $"{this.GetType().Name} Rifle bullet:{(bulletType != null ? bulletType : "rand")}({(bulletAmount.HasValue ? bulletAmount.value : "rand")}) chance:{chance} rare:{rare}";
            }
        }


        //private void Room_Loaded(On.Room.orig_Loaded orig, Room self)
        //{
        //    orig(self);
        //    if (GameUtils.IsCompetitiveSession)
        //    {
        //        List<AbstractPhysicalObject> addObjects = new List<AbstractPhysicalObject>();
        //        for (int i = 0; i < self.abstractRoom.entities.Count; i++)
        //        {
        //            AbstractPhysicalObject obj = (AbstractPhysicalObject)self.abstractRoom.entities[i];
        //            if (obj != null)
        //            {
        //                AbstractPhysicalObject newObject = null;
        //                bool destroy = false;

        //                if (obj.type == AbstractPhysicalObject.AbstractObjectType.Spear)
        //                {
        //                    float random = Random.value;
        //                    //LogDebug("Random value : " + random);
        //                    if (Random.value < 0.25 || (obj as AbstractSpear).explosive || (obj as AbstractSpear).electric || (obj as AbstractSpear).hue != 0f)
        //                    {
        //                        if (random < 0.25f)
        //                        {
        //                            newObject = new AbstractSpear(self.world, null, self.abstractRoom.entities[i].pos, self.game.GetNewID(), true, false);
        //                        }
        //                        else if (random < 0.5f)
        //                        {
        //                            newObject = new AbstractSpear(self.world, null, self.abstractRoom.entities[i].pos, self.game.GetNewID(), false, true);
        //                        }
        //                        else if (random < 0.75f)
        //                        {
        //                            newObject = new AbstractSpear(self.world, null, self.abstractRoom.entities[i].pos, self.game.GetNewID(), false, Mathf.Lerp(0.35f, 0.6f, Custom.ClampedRandomVariation(0.5f, 0.5f, 2f)));
        //                        }
        //                        else if (ModManager.MSC)
        //                        {
        //                            //LogDebug("new rifle");
        //                            JokeRifle.AbstractRifle.AmmoType ammo = new JokeRifle.AbstractRifle.AmmoType(ExtEnum<JokeRifle.AbstractRifle.AmmoType>.values.entries[Random.Range(0, ExtEnum<JokeRifle.AbstractRifle.AmmoType>.values.entries.Count)]);
        //                            newObject = new JokeRifle.AbstractRifle(self.world, null, self.abstractRoom.entities[i].pos, self.game.GetNewID(), ammo);
        //                            (newObject as JokeRifle.AbstractRifle).setCurrentAmmo((int)Random.Range(5, 40));
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (random < 0.25f)
        //                        {
        //                            newObject = new AbstractSpear(self.world, null, self.abstractRoom.entities[i].pos, self.game.GetNewID(), true, false);
        //                        }
        //                        else if (ModManager.MSC && random < 0.5f)
        //                        {
        //                            newObject = new AbstractSpear(self.world, null, self.abstractRoom.entities[i].pos, self.game.GetNewID(), false, true);
        //                        }
        //                        else
        //                        {
        //                            newObject = new AbstractSpear(self.world, null, self.abstractRoom.entities[i].pos, self.game.GetNewID(), false, false);
        //                            if (ModManager.Watcher && random < 0.5f)
        //                            {
        //                                (newObject as AbstractSpear).poison = 1.7f;
        //                                (newObject as AbstractSpear).poisonHue = 0.3f + UnityEngine.Random.value * 0.6f;
        //                            }
        //                        }
        //                    }

        //                    if (Random.value < 0.5f) // 1/2
        //                    {
        //                        destroy = true;
        //                    }
        //                }
        //                else if (obj.type == AbstractPhysicalObject.AbstractObjectType.Rock || obj.type == AbstractPhysicalObject.AbstractObjectType.DataPearl)
        //                {
        //                    List<RandomObject> objectsList = new List<RandomObject>
        //                    {
        //                        // TubeWorm 20
        //                        new RandomObject(AbstractPhysicalObject.AbstractObjectType.Rock, 20),
        //                        new RandomObject(AbstractPhysicalObject.AbstractObjectType.Rock, 20),
        //                        new RandomObject(AbstractPhysicalObject.AbstractObjectType.FlareBomb, 20),
        //                        new RandomObject(AbstractPhysicalObject.AbstractObjectType.Mushroom, 20),
        //                        new RandomObject(AbstractPhysicalObject.AbstractObjectType.JellyFish, 20),
        //                        new RandomObject(AbstractPhysicalObject.AbstractObjectType.PuffBall, 20),
        //                        new RandomObject(AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, 20),
        //                        // VultureGrub 10
        //                        new RandomObject(AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, 10),
        //                        new RandomObject(AbstractPhysicalObject.AbstractObjectType.VultureMask, 10),
        //                    };

        //                    if (ModManager.MSC)
        //                    {
        //                        objectsList.AddRange([
        //                            new RandomObject(MoreSlugcatsEnums.AbstractObjectType.FireEgg, 5),
        //                            //new randomObject(MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType.MoonCloak, 5), // don't work
        //                            new RandomObject(MoreSlugcatsEnums.AbstractObjectType.EnergyCell, 5),
        //                            new RandomObject(AbstractPhysicalObject.AbstractObjectType.KarmaFlower, 5),
        //                        ]);
        //                    }

        //                    if (ModManager.Watcher)
        //                    {
        //                        objectsList.AddRange([
        //                            new RandomObject(Watcher.WatcherEnums.AbstractObjectType.Boomerang, 20),
        //                        ]);
        //                    }

        //                    if (ModManager.MSC || ModManager.Watcher)
        //                    {
        //                        objectsList.AddRange([
        //                            new RandomObject(DLCSharedEnums.AbstractObjectType.SingularityBomb, 1),
        //                            new RandomObject(AbstractPhysicalObject.AbstractObjectType.KarmaFlower, 5),
        //                        ]);
        //                    }

        //                    newObject = MakeAbstractPhysicalObject(GetRandomObject(objectsList), self, obj.pos);

        //                    if (Random.value < 0.6f) // 3/5
        //                    {
        //                        destroy = true;
        //                    }
        //                }

        //                if (newObject != null)
        //                {
        //                    if (destroy)
        //                    {
        //                        LogDebug($"Object {obj.type} destroyed");
        //                        obj.Destroy();
        //                        self.abstractRoom.entities[i] = newObject;
        //                    }
        //                    else
        //                    {
        //                        addObjects.Add(newObject);
        //                    }
        //                    LogDebug($"Replace object {obj.type} {i} by {newObject.type}");
        //                    LogDebug("======================\n");
        //                }
        //            }
        //        }
        //        self.abstractRoom.entities.AddRange(addObjects);
        //    }
        //}

        //public class RandomObject(AbstractPhysicalObject.AbstractObjectType type, int chance)
        //{
        //    public AbstractPhysicalObject.AbstractObjectType type = type;
        //    public int chance = chance;
        //}


        //public AbstractPhysicalObject MakeAbstractPhysicalObject(AbstractPhysicalObject.AbstractObjectType type, Room room, WorldCoordinate pos)
        //{
        //    EntityID entityID = room.game.GetNewID();
        //    if (type == AbstractPhysicalObject.AbstractObjectType.VultureMask)
        //    {
        //        return new VultureMask.AbstractVultureMask(room.world, null, pos, entityID, entityID.RandomSeed, false);
        //    }
        //    if (type == AbstractPhysicalObject.AbstractObjectType.DataPearl)
        //    {
        //        return new DataPearl.AbstractDataPearl(room.world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, pos, entityID, -1, -1, null, new DataPearl.AbstractDataPearl.DataPearlType(ExtEnum<DataPearl.AbstractDataPearl.DataPearlType>.values.entries[Random.Range(0, ExtEnum<DataPearl.AbstractDataPearl.DataPearlType>.values.entries.Count)], false));
        //    }
        //    if (type == MoreSlugcatsEnums.AbstractObjectType.FireEgg)
        //    {
        //        return new FireEgg.AbstractBugEgg(room.world, null, pos, entityID, Mathf.Lerp(0.35f, 0.6f, Custom.ClampedRandomVariation(0.5f, 0.5f, 2f)));
        //    }
        //    if (type == MoreSlugcatsEnums.AbstractObjectType.MoonCloak)
        //    {
        //        return new AbstractConsumable(room.world, MoreSlugcatsEnums.AbstractObjectType.MoonCloak, null, pos, entityID, -1, -1, null);
        //    }
        //    if (AbstractConsumable.IsTypeConsumable(type))
        //    {
        //        return new AbstractConsumable(room.world, type, null, pos, entityID, -1, -1, null);
        //    }

        //    return new AbstractPhysicalObject(room.world, type, null, pos, entityID);
        //}
    }
}
