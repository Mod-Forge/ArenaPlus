
using MoreSlugcats;
using UnityEngine;

namespace ArenaSlugcatsConfigurator.Freatures
{
    internal static class ElectroBoom
    {
        internal static void OnEnable()
        {
            logSource.LogInfo("ElectroBoom OnEnable");
            On.MoreSlugcats.ElectricSpear.ChangeMode += ElectricSpear_ChangeMode;
        }

        private static void ElectricSpear_ChangeMode(On.MoreSlugcats.ElectricSpear.orig_ChangeMode orig, ElectricSpear self, Weapon.Mode newMode)
        {
            if (self.room.game.IsArenaSession && !IsChallengeGameSession(self.room.game) && newMode == Weapon.Mode.StuckInWall && self.room.gravity != 0 && self.abstractSpear.electricCharge > 0 && self.rotation == new Vector2(0, -1))
            {
                //float boomSize = 40f * self.abstractSpear.electricCharge;
                self.room.AddObject(new ZapCoil.ZapFlash(self.sparkPoint, 40f * self.abstractSpear.electricCharge));
                self.room.PlaySound(SoundID.Zapper_Zap, self.sparkPoint, 1f, (self.zapPitch == 0f) ? (1.5f + Random.value * 1.5f) : self.zapPitch);
                self.room.AddObject(new ShockWave(self.sparkPoint, 600f * self.abstractSpear.electricCharge, 0.035f, 20, false));


                for (int j = 0; j < self.room.physicalObjects.Length; j++)
                {
                    for (int k = 0; k < self.room.physicalObjects[j].Count; k++)
                    {
                        if (self != self.room.physicalObjects[j][k] && !self.room.physicalObjects[j][k].slatedForDeletetion)
                        {
                            if (self.room.physicalObjects[j][k] is Creature)
                            {
                                Creature creature = self.room.physicalObjects[j][k] as Creature;
                                if (Vector2.Distance(self.firstChunk.pos, creature.mainBodyChunk.pos) < 70f * self.abstractSpear.electricCharge)
                                {
                                    creature.Stun((int)((!(creature is Player)) ? (320f * 2f * Mathf.Lerp(creature.Template.baseStunResistance, 1f, 0.5f)) : 140f));
                                    self.room.AddObject(new CreatureSpasmer(creature, false, creature.stun));

                                }
                            }
                        }
                    }
                }
                self.ExplosiveShortCircuit();
                ConsoleWrite("rot: " + self.rotation);
                newMode = Weapon.Mode.Free;
            }

            orig(self, newMode);
        }
    }
}
