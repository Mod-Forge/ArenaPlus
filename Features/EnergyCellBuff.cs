using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArenaSlugcatsConfigurator;
using MoreSlugcats;
using UnityEngine;

namespace ArenaSlugcatsConfigurator.Freatures
{
    internal static class EnergyCellBuff
    {
        public static float gravityFieldSize = 1f;
        internal static void Register()
        {
            logSource.LogInfo("EnergyCellBuff Register");
            On.MoreSlugcats.EnergyCell.Use += EnergyCell_Use;
            On.MoreSlugcats.EnergyCell.DrawSprites += EnergyCell_DrawSprites;
            On.Room.Update += Room_Update;
        }

        private static void EnergyCell_Use(On.MoreSlugcats.EnergyCell.orig_Use orig, EnergyCell self, bool forced)
        {
            orig(self, forced);
            if (self.usingTime == 600f) self.usingTime *= 2f;
        }

        private static void EnergyCell_DrawSprites(On.MoreSlugcats.EnergyCell.orig_DrawSprites orig, EnergyCell self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            sLeaser.sprites[3].scale = 15f * gravityFieldSize;
        }

        private static void Room_Update(On.Room.orig_Update orig, Room self)
        {
            orig(self);

            if (!self.game.IsArenaSession || IsChallengeGameSession(self.game))
            {
                return;
            }

            RoomCustomData customData = self.GetCustomData<RoomCustomData>();
            customData.frame++;

            //if (customData.frame % 20 > 0)
            //{
            //    return;
            //}

            List<EnergyCell> energyCells = new List<EnergyCell>();
            for (int j = 0; j < self.physicalObjects.Length; j++)
            {
                for (int k = 0; k < self.physicalObjects[j].Count; k++)
                {
                    if (self.physicalObjects[j][k] is EnergyCell)
                    {
                        energyCells.Add(self.physicalObjects[j][k] as EnergyCell);
                    }
                }
            }


            for (int j = 0; j < self.physicalObjects.Length; j++)
            {
                for (int k = 0; k < self.physicalObjects[j].Count; k++)
                {
                    PhysicalObject obj = self.physicalObjects[j][k];

                    bool notProtected = obj is not Creature || (obj as Creature).grasps == null || (obj as Creature).grasps[0] == null || (obj as Creature).grasps[0].grabbed is not EnergyCell || ((obj as Creature).grasps[0].grabbed as EnergyCell).usingTime == 0f;

                    if (!obj.slatedForDeletetion && notProtected)
                    {
                        foreach (EnergyCell energyCell in energyCells)
                        {
                            bool doBreak = false;



                            if (!energyCell.slatedForDeletetion)
                            {
                                if (energyCell.usingTime > 0f)
                                {
                                    (energyCell as PhysicalObject).gravity = 0.45f;
                                    //energyCell.firstChunk.vel = energyCell.firstChunk.vel.normalized * Math.Min(energyCell.firstChunk.vel.magnitude, 10f);
                                }

                                if (obj is not EnergyCell)
                                {
                                    if (energyCell.usingTime > 0f)
                                    {
                                        foreach (BodyChunk chuck in obj.bodyChunks)
                                        {
                                            float dist = Vector2.Distance(energyCell.firstChunk.pos, chuck.pos);
                                            if (dist < 110f * gravityFieldSize) // 75
                                            {
                                                //Debug.Log("no gravity to " + obj);

                                                if (obj is Creature)
                                                {
                                                    energyCell.firstChunk.vel += (chuck.pos - energyCell.firstChunk.pos).normalized * 0.25f;
                                                }


                                                if (obj is Player)
                                                {
                                                    (obj as Player).customPlayerGravity = 0f;
                                                    (obj as Player).animation = Player.AnimationIndex.ZeroGSwim;
                                                }
                                                else
                                                {
                                                    chuck.vel.y += obj.gravity;
                                                    //Debug.Log("max: " + chuck.mass * 200);
                                                    chuck.vel = chuck.vel.normalized * Math.Min(chuck.vel.magnitude, chuck.mass * 100);
                                                }

                                                doBreak = true;
                                                //blackList.Add(chuck);
                                            }

                                        }
                                    }
                                    else
                                    {
                                        if (obj is Player && ((obj as Player).animation == Player.AnimationIndex.ZeroGSwim || (obj as Player).animation == Player.AnimationIndex.ZeroGPoleGrab))
                                        {
                                            (obj as Player).customPlayerGravity = 0.9f;
                                            (obj as Player).animation = Player.AnimationIndex.None;
                                        }
                                    }
                                }
                            }

                            if (doBreak) break;
                        }
                    }
                }
            }
        }
    }
}

