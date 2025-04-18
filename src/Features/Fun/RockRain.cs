﻿using ArenaPlus.Utils;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MoreSlugcats;
using ArenaPlus.Lib;
using MonoMod.Cil;

namespace ArenaPlus.Features.Fun
{
    [FeatureInfo(
        id: "rockRain",
        name: "Rock rain",
        category: BuiltInCategory.Fun,
        description: "Make rock rain",
        enabledByDefault: false
    )]
    file class RockRain(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
    {
        protected override void Unregister()
        {
            On.RoomRain.ctor -= RoomRain_ctor;
            On.RoomRain.Update -= RoomRain_Update;
            On.RoomRain.DrawSprites -= RoomRain_DrawSprites;
            On.Player.Grabability -= Player_Grabability;
        }

        protected override void Register()
        {
            On.RoomRain.ctor += RoomRain_ctor;
            On.RoomRain.Update += RoomRain_Update;
            On.RoomRain.DrawSprites += RoomRain_DrawSprites;
            On.Player.Grabability += Player_Grabability;
        }

        private Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
        {
            if (obj is TemporaryTock)
            {
                return Player.ObjectGrabability.CantGrab;
            }
            return orig(self, obj);
        }

        private void RoomRain_DrawSprites(On.RoomRain.orig_DrawSprites orig, RoomRain self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (GameUtils.IsCompetitiveOrSandboxSession && (self.dangerType == RoomRain.DangerType.FloodAndRain || self.dangerType == RoomRain.DangerType.Rain) && !self.slatedForDeletetion)
            {
                foreach (var sprite in sLeaser.sprites)
                {
                    sprite.isVisible = false;
                }
            }
        }

        private static void RoomRain_ctor(On.RoomRain.orig_ctor orig, RoomRain self, GlobalRain globalRain, Room rm)
        {
            orig.Invoke(self, globalRain, rm);
            if (GameUtils.IsCompetitiveOrSandboxSession && (self.dangerType == RoomRain.DangerType.FloodAndRain || self.dangerType == RoomRain.DangerType.Rain))
                self.splashes = 0;
        }


        private static void RoomRain_Update(On.RoomRain.orig_Update orig, RoomRain self, bool eu)
        {
            orig.Invoke(self, eu);
            if (GameUtils.IsCompetitiveOrSandboxSession && self.intensity > 0f && self.room.BeingViewed && (self.dangerType == RoomRain.DangerType.FloodAndRain || self.dangerType == RoomRain.DangerType.Rain))
            {
                for (int x = 0; x < self.room.Width; x++)
                {
                    if (!self.room.GetTile(x, self.room.Height - 1).Solid && Random.value < (self.intensity / 30))
                    {
                        AbstractPhysicalObject abstRock = new AbstractPhysicalObject(self.room.world, AbstractPhysicalObject.AbstractObjectType.Rock, null, self.room.GetWorldCoordinate(new IntVector2(x, self.room.Height)), self.room.game.GetNewID());
                        Rock rock = new TemporaryTock(abstRock, self.room.world);
                        rock.firstChunk.HardSetPosition(self.room.MiddleOfTile(new IntVector2(x, self.room.Height)));
                        rock.tailPos = rock.firstChunk.pos;
                        abstRock.realizedObject = rock;
                        abstRock.RealizeInRoom();
                        rock.Shoot(null, rock.firstChunk.pos, Vector2.down, 1f, eu);
                    }
                }
            }
        }
    }

    public class TemporaryTock : Rock
    {
        int lifeTime;
        public TemporaryTock(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
        {
            lifeTime = 40;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            lifeTime--;
            if (lifeTime <= 0)
            {
                Destroy();
            }
        }

        public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
        {
            base.Collide(otherObject, myChunk, otherChunk);
            Destroy();
        }

        public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
        {
            if (result.hitSomething)
            {
                Destroy();
            }
            return base.HitSomething(result, eu);
        }
    }
}
