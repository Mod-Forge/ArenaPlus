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
        id: "allJokeRifle",
        name: "All Joke Rifle",
        category: BuiltInCategory.Spoilers,
        description: "Replacer every spears with Joke Rifles (Challenge spoiler)",
        incompatibilities: ["objectsRandomizer"],
        enabledByDefault: false
    )]
    public class AllJokeRifle(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
    {
        protected override void Unregister()
        {
            On.Room.Loaded -= Room_Loaded;
            On.Spear.Update -= Spear_Update;
        }
        protected override void Register()
        {
            On.Room.Loaded += Room_Loaded;
            On.Spear.Update += Spear_Update;
        }

        private void Spear_Update(On.Spear.orig_Update orig, Spear self, bool eu)
        {
            orig(self, eu);
            if (!self.slatedForDeletetion && self.room != null && self.abstractPhysicalObject.world.game.IsArenaSession && self.abstractPhysicalObject.world.game.session is not SandboxGameSession)
            {
                JokeRifle.AbstractRifle.AmmoType ammo = new JokeRifle.AbstractRifle.AmmoType(ExtEnum<JokeRifle.AbstractRifle.AmmoType>.values.entries[Random.Range(0, ExtEnum<JokeRifle.AbstractRifle.AmmoType>.values.entries.Count)]);
                JokeRifle.AbstractRifle rifle = new JokeRifle.AbstractRifle(self.abstractPhysicalObject.world, null, self.abstractPhysicalObject.pos, self.abstractPhysicalObject.world.game.GetNewID(), ammo);
                rifle.setCurrentAmmo((int)Random.Range(5, 40));
                self.room.abstractRoom.AddEntity(rifle);
                rifle.RealizeInRoom();

                if (self.grabbedBy != null)
                {
                    List<Creature.Grasp> grasps = new List<Creature.Grasp>(self.grabbedBy);
                    foreach (var grasp in grasps)
                    {
                        grasp.Release();
                        grasp.grabber.Grab(grasp.grabbed, grasp.graspUsed, grasp.chunkGrabbed, grasp.shareability, grasp.dominance, false, grasp.pacifying);
                    }
                }

                self.Destroy();
                self.abstractPhysicalObject.Destroy();
                self.room.abstractRoom.RemoveEntity(self.abstractPhysicalObject);
            }
        }

        private void Room_Loaded(On.Room.orig_Loaded orig, Room self)
        {
            orig(self);
            if (self.game != null && self.game.IsArenaSession && self.game.session is not SandboxGameSession)
            {
                for (int i = 0; i < self.abstractRoom.entities.Count; i++)
                {
                    if (self.abstractRoom.entities[i] is AbstractSpear abstSpear)
                    {
                        JokeRifle.AbstractRifle.AmmoType ammo = new JokeRifle.AbstractRifle.AmmoType(ExtEnum<JokeRifle.AbstractRifle.AmmoType>.values.entries[Random.Range(0, ExtEnum<JokeRifle.AbstractRifle.AmmoType>.values.entries.Count)]);
                        JokeRifle.AbstractRifle rifle = new JokeRifle.AbstractRifle(self.world, null, self.abstractRoom.entities[i].pos, self.game.GetNewID(), ammo);
                        rifle.setCurrentAmmo((int)Random.Range(5, 40));

                        abstSpear.Destroy();
                        self.abstractRoom.entities[i] = rifle;
                    }
                }
            }
        }
    }
}
