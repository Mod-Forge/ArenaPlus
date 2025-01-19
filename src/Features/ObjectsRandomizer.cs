using ArenaPlus.Lib;
using ArenaPlus.Utils;
using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArenaPlus.Features
{
    [FeatureInfo(
        id: "objectsRandomizer",
        name: "Objects randomizer",
        description: "Whether objects in arena are random",
        enabledByDefault: false
    )]
    file class ObjectsRandomizer(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
    {
        protected override void Register()
        {
            On.Room.Loaded += Room_Loaded;
        }

        protected override void Unregister()
        {
            On.Room.Loaded -= Room_Loaded;
        }

        private void Room_Loaded(On.Room.orig_Loaded orig, Room self)
        {
            orig(self);
            if (self.game != null && self.game.IsArenaSession && !GameUtils.IsChallengeGameSession(self.game) && self.game.session is not SandboxGameSession)
            {
                List<AbstractPhysicalObject> addObjects = new List<AbstractPhysicalObject>();
                for (int i = 0; i < self.abstractRoom.entities.Count; i++)
                {
                    AbstractPhysicalObject obj = (AbstractPhysicalObject)self.abstractRoom.entities[i];
                    if (obj != null)
                    {
                        AbstractPhysicalObject newObject = null;
                        bool destroy = false;

                        if (obj.type == AbstractPhysicalObject.AbstractObjectType.Spear)
                        {
                            float random = Random.value;
                            //ConsoleWrite("Random value : " + random);
                            if (Random.value < 0.25 || (obj as AbstractSpear).explosive || (obj as AbstractSpear).electric || (obj as AbstractSpear).hue != 0f)
                            {
                                if (random < 0.25f)
                                {
                                    newObject = new AbstractSpear(self.world, null, self.abstractRoom.entities[i].pos, self.game.GetNewID(), true, false);
                                }
                                else if (random < 0.5f)
                                {
                                    newObject = new AbstractSpear(self.world, null, self.abstractRoom.entities[i].pos, self.game.GetNewID(), false, true);
                                }
                                else if (random < 0.75f)
                                {
                                    newObject = new AbstractSpear(self.world, null, self.abstractRoom.entities[i].pos, self.game.GetNewID(), false, Mathf.Lerp(0.35f, 0.6f, Custom.ClampedRandomVariation(0.5f, 0.5f, 2f)));
                                }
                                else
                                {
                                    //ConsoleWrite("new rifle");
                                    JokeRifle.AbstractRifle.AmmoType ammo = new JokeRifle.AbstractRifle.AmmoType(ExtEnum<JokeRifle.AbstractRifle.AmmoType>.values.entries[Random.Range(0, ExtEnum<JokeRifle.AbstractRifle.AmmoType>.values.entries.Count)]);
                                    newObject = new JokeRifle.AbstractRifle(self.world, null, self.abstractRoom.entities[i].pos, self.game.GetNewID(), ammo);
                                    (newObject as JokeRifle.AbstractRifle).setCurrentAmmo((int)Random.Range(5, 40));
                                }
                            }
                            else
                            {
                                if (random < 0.25f)
                                {
                                    newObject = new AbstractSpear(self.world, null, self.abstractRoom.entities[i].pos, self.game.GetNewID(), true, false);
                                }
                                else if (random < 0.5f)
                                {
                                    newObject = new AbstractSpear(self.world, null, self.abstractRoom.entities[i].pos, self.game.GetNewID(), false, true);
                                }
                                else
                                {
                                    newObject = new AbstractSpear(self.world, null, self.abstractRoom.entities[i].pos, self.game.GetNewID(), false, false);
                                }
                            }

                            //newObject = new AbstractSpear(self.world, null, self.abstractRoom.entities[i].pos, self.game.GetNewID(), false, false);

                            if (Random.value < 0.5f) // 1/2
                            {
                                destroy = true;
                            }
                        }
                        else if (obj.type == AbstractPhysicalObject.AbstractObjectType.Rock || obj.type == AbstractPhysicalObject.AbstractObjectType.DataPearl)
                        {
                            List<RandomObject> objectsList = new List<RandomObject>
                            {
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
                                new RandomObject(MoreSlugcatsEnums.AbstractObjectType.FireEgg, 5),
                                //new randomObject(MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType.MoonCloak, 5), // don't work
                                new RandomObject(MoreSlugcatsEnums.AbstractObjectType.EnergyCell, 5),
                                new RandomObject(AbstractPhysicalObject.AbstractObjectType.KarmaFlower, 5),
                                new RandomObject(MoreSlugcatsEnums.AbstractObjectType.SingularityBomb, 1),
                            };

                            newObject = MakeAbstractPhysicalObject(GetRandomObject(objectsList), self, obj.pos);

                            if (Random.value < 0.6f) // 3/5
                            {
                                destroy = true;
                            }
                        }

                        if (newObject != null)
                        {
                            if (destroy)
                            {
                                ConsoleWrite($"Object {obj.type} destroyed");
                                obj.Destroy();
                                self.abstractRoom.entities[i] = newObject;
                            }
                            else
                            {
                                addObjects.Add(newObject);
                            }
                            ConsoleWrite($"Replace object {obj.type} {i} by {newObject.type}");
                            ConsoleWrite("======================\n");
                        }
                    }
                }
                self.abstractRoom.entities.AddRange(addObjects);
            }
        }

        public class RandomObject(AbstractPhysicalObject.AbstractObjectType type, int chance)
        {
            public AbstractPhysicalObject.AbstractObjectType type = type;
            public int chance = chance;
        }

        public AbstractPhysicalObject.AbstractObjectType GetRandomObject(List<RandomObject> list)
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
                    //ConsoleWrite("Type Generated");
                    return list[i].type;
                }
            }
            return list[Random.Range(0, list.Count)].type;
        }

        public AbstractPhysicalObject MakeAbstractPhysicalObject(AbstractPhysicalObject.AbstractObjectType type, Room room, WorldCoordinate pos)
        {
            EntityID entityID = room.game.GetNewID();
            if (type == AbstractPhysicalObject.AbstractObjectType.VultureMask)
            {
                return new VultureMask.AbstractVultureMask(room.world, null, pos, entityID, entityID.RandomSeed, false);
            }
            if (type == AbstractPhysicalObject.AbstractObjectType.DataPearl)
            {
                return new DataPearl.AbstractDataPearl(room.world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, pos, entityID, -1, -1, null, new DataPearl.AbstractDataPearl.DataPearlType(ExtEnum<DataPearl.AbstractDataPearl.DataPearlType>.values.entries[Random.Range(0, ExtEnum<DataPearl.AbstractDataPearl.DataPearlType>.values.entries.Count)], false));
            }
            if (type == MoreSlugcatsEnums.AbstractObjectType.FireEgg)
            {
                return new FireEgg.AbstractBugEgg(room.world, null, pos, entityID, Mathf.Lerp(0.35f, 0.6f, Custom.ClampedRandomVariation(0.5f, 0.5f, 2f)));
            }
            if (type == MoreSlugcatsEnums.AbstractObjectType.MoonCloak)
            {
                return new AbstractConsumable(room.world, MoreSlugcatsEnums.AbstractObjectType.MoonCloak, null, pos, entityID, -1, -1, null);
            }
            if (AbstractConsumable.IsTypeConsumable(type))
            {
                return new AbstractConsumable(room.world, type, null, pos, entityID, -1, -1, null);
            }

            return new AbstractPhysicalObject(room.world, type, null, pos, entityID);
        }
    }
}
