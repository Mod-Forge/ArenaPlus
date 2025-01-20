using ArenaPlus.Lib;
using ArenaPlus.Utils;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaPlus.Features.Slugcats
{
    [SlugcatFeatureInfo(
        id: "gourmandBetterItemGeneration",
        name: "Gourmand better item generation",
        description: "Whether the Gourmand generate better item for crafting in arena",
        slugcat: "Gourmand"
    )]
    file class GourmandBetterItemGeneration(SlugcatFeatureInfoAttribute featureInfo) : SlugcatFeature(featureInfo)
    {
        protected override void Register()
        {
            On.Player.Regurgitate += Player_Regurgitate;
        }

        protected override void Unregister()
        {
            On.Player.Regurgitate -= Player_Regurgitate;
        }

        private void Player_Regurgitate(On.Player.orig_Regurgitate orig, Player self)
        {
            if (self.objectInStomach == null && self.isGourmand && GameUtils.IsCompetitiveOrSandboxSession)
            {
                if (Random.value < 0.67)
                {
                    self.objectInStomach = RandomStomachItem(self);
                }
                else
                {
                    self.objectInStomach = GourmandCombos.RandomStomachItem(self);
                }
            }
            orig(self);
        }

        public static AbstractPhysicalObject RandomStomachItem(PhysicalObject caller)
        {
            float value = Random.value;
            AbstractPhysicalObject abstractPhysicalObject;
            if (value <= 0.65f)
            {
                abstractPhysicalObject = new AbstractConsumable(caller.room.world, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID(), -1, -1, null);
            }
            else
            {
                abstractPhysicalObject = new AbstractConsumable(caller.room.world, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID(), -1, -1, null);
            }
            if (AbstractConsumable.IsTypeConsumable(abstractPhysicalObject.type))
            {
                (abstractPhysicalObject as AbstractConsumable).isFresh = false;
                (abstractPhysicalObject as AbstractConsumable).isConsumed = true;
            }
            return abstractPhysicalObject;
        }
    }
}
