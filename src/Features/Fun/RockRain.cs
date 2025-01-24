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
            throw new NotImplementedException();
        }

        protected override void Register()
        {
            On.RoomRain.ctor += RoomRain_ctor;
            On.RoomRain.Update += RoomRain_Update;
            On.RoomRain.DrawSprites += RoomRain_DrawSprites;
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
                    if (!self.room.GetTile(x, self.room.Height - 1).Solid && Random.Range(0, 100) < Mathf.Lerp(1, 4, self.intensity))
                    {
                        AbstractPhysicalObject abstRock = new AbstractPhysicalObject(self.room.world, AbstractPhysicalObject.AbstractObjectType.Rock, null, self.room.GetWorldCoordinate(new IntVector2(x, self.room.Height)), self.room.game.GetNewID());
                        Rock rock = new TemporaryTock(abstRock, self.room.world);
                        abstRock.realizedObject = rock;
                        abstRock.RealizeInRoom();
                        rock.firstChunk.HardSetPosition(self.room.MiddleOfTile(new IntVector2(x, self.room.Height)));
                        rock.Shoot(null, rock.firstChunk.pos, Vector2.down, 1f, eu);
                    }
                }
            }
        }
    }

    public class TemporaryTock : Rock
    {
        public TemporaryTock(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
        {
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
        }

        public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
        {
            base.Collide(otherObject, myChunk, otherChunk);
            Destroy();
        }

        public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
        {
            Destroy();
            return base.HitSomething(result, eu);
        }
    }
}
