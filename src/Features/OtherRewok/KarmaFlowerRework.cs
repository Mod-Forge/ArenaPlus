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

namespace ArenaPlus.Features.Test
{
    [FeatureInfo(
        id: "karmaFlowerRework",
        name: "Saint karma flower (Spoiler)",
        color: "ffff00",
        category: BuiltInCategory.Spoilers,
        description: "Allow players to ascend (Saint spoiler)",
        enabledByDefault: true
    )]
    file class KarmaFlowerRework(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
    {
        protected override void Register()
        {
            Log("Enabling karma flower rework");
            On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainer;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            On.Player.ClassMechanicsSaint += Player_ClassMechanicsSaint;
            On.KarmaFlower.BitByPlayer += KarmaFlower_BitByPlayer;
            On.Spear.HitSomethingWithoutStopping += Spear_HitSomethingWithoutStopping;
        }

        protected override void Unregister()
        {
            Log("Disabling karma flower rework");
            On.PlayerGraphics.AddToContainer -= PlayerGraphics_AddToContainer;
            On.PlayerGraphics.DrawSprites -= PlayerGraphics_DrawSprites;
            On.Player.ClassMechanicsSaint -= Player_ClassMechanicsSaint;
            On.KarmaFlower.BitByPlayer -= KarmaFlower_BitByPlayer;
            On.Spear.HitSomethingWithoutStopping -= Spear_HitSomethingWithoutStopping;
        }

        private void Spear_HitSomethingWithoutStopping(On.Spear.orig_HitSomethingWithoutStopping orig, Spear self, PhysicalObject obj, BodyChunk chunk, PhysicalObject.Appendage appendage)
        {
            if (self.room.game.IsArenaSession && !GameUtils.IsChallengeGameSession(self.room.game) && self.room.game.rainWorld.progression.miscProgressionData.beaten_Saint)
            {
                if (self.Spear_NeedleCanFeed())
                {
                    if (obj is KarmaFlower)
                    {
                        ConsoleWrite("do thig whouou");
                        Player player = self.thrownBy as Player;
                        if (!player.monkAscension && (player.tongue == null || !player.tongue.Attached))
                        {
                            player.maxGodTime = 565f;
                            player.godTimer = player.maxGodTime;
                            player.bodyMode = Player.BodyModeIndex.Default;
                            player.ActivateAscension();
                        }
                        obj.Destroy();
                        return;
                    }
                }
            }
            orig(self, obj, chunk, appendage);
        }

        private void KarmaFlower_BitByPlayer(On.KarmaFlower.orig_BitByPlayer orig, KarmaFlower self, Creature.Grasp grasp, bool eu)
        {
            if (self.room.game.IsArenaSession && !GameUtils.IsChallengeGameSession(self.room.game) && self.room.game.rainWorld.progression.miscProgressionData.beaten_Saint && self.bites < 2)
            {
                Player player = grasp.grabber as Player;
                if (!player.monkAscension && (player.tongue == null || !player.tongue.Attached))
                {
                    player.maxGodTime = 565f;
                    player.godTimer = player.maxGodTime;
                    player.bodyMode = Player.BodyModeIndex.Default;
                    player.ActivateAscension();
                }
            }
            orig(self, grasp, eu);
        }

        private void Player_ClassMechanicsSaint(On.Player.orig_ClassMechanicsSaint orig, Player self)
        {
            if (self.room.game.IsArenaSession && !GameUtils.IsChallengeGameSession(self.room.game) && self.room.game.rainWorld.progression.miscProgressionData.beaten_Saint)
            {
                if (self.maxGodTime != 565f)
                {
                    self.maxGodTime = 0f;
                    self.godTimer = 0f;
                }

                self.karmaCharging = -1;
                self.godTimer = (int)Math.Floor(self.godTimer);

                if (self.wantToJump > 0 && self.monkAscension)
                {
                    self.DeactivateAscension();
                    self.wantToJump = 0;
                }
                else if (self.wantToJump > 0 && self.input[0].pckp && self.canJump <= 0 && !self.monkAscension && (self.tongue == null || !self.tongue.Attached) && self.godTimer > 0)
                {
                    self.bodyMode = Player.BodyModeIndex.Default;
                    self.ActivateAscension();
                }

                if (self.monkAscension)
                {
                    self.godTimer--;
                }

                //Debug.Log("timer: " + self.godTimer + " / " + self.maxGodTime);
            }
            orig(self);
        }

        private void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (rCam.room.game.IsArenaSession && !GameUtils.IsChallengeGameSession(rCam.room.game) && rCam.room.game.rainWorld.progression.miscProgressionData.beaten_Saint)
            {
                PlayerCustomData customData = (self.owner as Player).GetCustomData<PlayerCustomData>();
                if (!rCam.room.game.DEBUGMODE && ModManager.MSC && self.player.room != null && self.player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint && sLeaser.sprites.Length >= customData.customSpriteIndex + 2 + self.numGodPips)
                {

                    //if (self.player.room.GetCustomData<RoomCustomData>().frame % 60 == 0)
                    //{
                    //    for (int i = 0; i < sLeaser.sprites.Length; i++)
                    //    {
                    //        ConsoleWrite($"sprite[{i}]: {sLeaser.sprites[i]}, {customData.customSpriteIndex}");
                    //    }
                    //}

                    if (self.player.killFac > 0f || self.player.forceBurst)
                    {
                        sLeaser.sprites[customData.customSpriteIndex].isVisible = true;
                        sLeaser.sprites[customData.customSpriteIndex].x = sLeaser.sprites[3].x + self.player.burstX;
                        sLeaser.sprites[customData.customSpriteIndex].y = sLeaser.sprites[3].y + self.player.burstY + 60f;
                        float f = Mathf.Lerp(self.player.lastKillFac, self.player.killFac, timeStacker);
                        sLeaser.sprites[customData.customSpriteIndex].scale = Mathf.Lerp(50f, 2f, Mathf.Pow(f, 0.5f));
                        sLeaser.sprites[customData.customSpriteIndex].alpha = Mathf.Pow(f, 3f);
                    }
                    else
                    {
                        sLeaser.sprites[customData.customSpriteIndex].isVisible = false;
                    }
                    if (self.player.killWait > self.player.lastKillWait || self.player.killWait == 1f || self.player.forceBurst)
                    {
                        self.rubberMouseX += (self.player.burstX - self.rubberMouseX) * 0.3f;
                        self.rubberMouseY += (self.player.burstY - self.rubberMouseY) * 0.3f;
                    }
                    else
                    {
                        self.rubberMouseX *= 0.15f;
                        self.rubberMouseY *= 0.25f;
                    }
                    if (Mathf.Sqrt(Mathf.Pow(sLeaser.sprites[3].x - self.rubberMarkX, 2f) + Mathf.Pow(sLeaser.sprites[3].y - self.rubberMarkY, 2f)) > 100f)
                    {
                        self.rubberMarkX = sLeaser.sprites[3].x;
                        self.rubberMarkY = sLeaser.sprites[3].y;
                    }
                    else
                    {
                        self.rubberMarkX += (sLeaser.sprites[3].x - self.rubberMarkX) * 0.15f;
                        self.rubberMarkY += (sLeaser.sprites[3].y - self.rubberMarkY) * 0.25f;
                    }
                    sLeaser.sprites[customData.customSpriteIndex + 1].x = self.rubberMarkX;
                    sLeaser.sprites[customData.customSpriteIndex + 1].y = self.rubberMarkY + 60f;
                    float num12;
                    if (self.player.monkAscension)
                    {
                        sLeaser.sprites[9].color = Custom.HSL2RGB(Random.value, Random.value, Random.value);
                        sLeaser.sprites[10].alpha = 0f;
                        sLeaser.sprites[11].alpha = 0f;
                        sLeaser.sprites[customData.customSpriteIndex + 1].color = sLeaser.sprites[9].color;
                        num12 = 1f;
                    }
                    else
                    {
                        num12 = 0f;
                    }
                    float num13;
                    if ((self.player.godTimer < self.player.maxGodTime || self.player.monkAscension) && !self.player.hideGodPips)
                    {
                        num13 = 1f;
                        float num14 = 15f;
                        if (!self.player.monkAscension)
                        {
                            num14 = 6f;
                        }
                        self.rubberRadius += (num14 - self.rubberRadius) * 0.045f;
                        if (self.rubberRadius < 5f)
                        {
                            self.rubberRadius = num14;
                        }
                        float num15 = self.player.maxGodTime / self.numGodPips;
                        for (int m = 0; m < self.numGodPips; m++)
                        {
                            float num16 = num15 * m;
                            float num17 = num15 * (m + 1);
                            if (self.player.godTimer <= num16)
                            {
                                sLeaser.sprites[customData.customSpriteIndex + 2 + m].scale = 0f;
                            }
                            else if (self.player.godTimer >= num17)
                            {
                                sLeaser.sprites[customData.customSpriteIndex + 2 + m].scale = 1f;
                            }
                            else
                            {
                                sLeaser.sprites[customData.customSpriteIndex + 2 + m].scale = (self.player.godTimer - num16) / num15;
                            }
                            if (self.player.karmaCharging > 0 && self.player.monkAscension)
                            {
                                sLeaser.sprites[customData.customSpriteIndex + 2 + m].color = sLeaser.sprites[9].color;
                            }
                            else
                            {
                                sLeaser.sprites[customData.customSpriteIndex + 2 + m].color = PlayerGraphics.SlugcatColor(self.CharacterForColor);
                            }
                        }
                    }
                    else
                    {
                        num13 = 0f;
                    }

                    self.rubberMarkX = sLeaser.sprites[0].x;
                    self.rubberMarkY = sLeaser.sprites[0].y;


                    sLeaser.sprites[customData.customSpriteIndex + 1].x = self.rubberMarkX + self.rubberMouseX;
                    sLeaser.sprites[customData.customSpriteIndex + 1].y = self.rubberMarkY + 60f + self.rubberMouseY;
                    self.rubberAlphaEmblem += (num12 - self.rubberAlphaEmblem) * 0.05f;
                    self.rubberAlphaPips += (num13 - self.rubberAlphaPips) * 0.05f;
                    sLeaser.sprites[customData.customSpriteIndex + 1].alpha = self.rubberAlphaEmblem;
                    sLeaser.sprites[10].alpha *= 1f - self.rubberAlphaPips;
                    sLeaser.sprites[11].alpha *= 1f - self.rubberAlphaPips;
                    for (int n = customData.customSpriteIndex + 2; n < customData.customSpriteIndex + 2 + self.numGodPips; n++)
                    {
                        sLeaser.sprites[n].alpha = self.rubberAlphaPips;
                        Vector2 vector14 = new Vector2(sLeaser.sprites[customData.customSpriteIndex + 1].x, sLeaser.sprites[customData.customSpriteIndex + 1].y);
                        vector14 += Custom.rotateVectorDeg(Vector2.one * self.rubberRadius, (n - 15) * (360f / self.numGodPips));
                        sLeaser.sprites[n].x = vector14.x;
                        sLeaser.sprites[n].y = vector14.y;
                    }


                }
            }
        }

        private void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            ConsoleWrite("PlayerGraphics_AddToContainer");


            if (rCam.room.game.IsArenaSession && !GameUtils.IsChallengeGameSession(rCam.room.game) && rCam.room.game.rainWorld.progression.miscProgressionData.beaten_Saint && self.player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                PlayerCustomData customData = (self.owner as Player).GetCustomData<PlayerCustomData>();
                if (!customData.initFinish)
                {
                    customData.customSpriteIndex = sLeaser.sprites.Length;
                    customData.initFinish = true;

                    FSprite[] newFSprites = new FSprite[customData.customSpriteIndex + 2 + self.numGodPips];
                    for (int i = 0; i < sLeaser.sprites.Length; i++)
                    {
                        newFSprites[i] = sLeaser.sprites[i];
                    }


                    newFSprites[customData.customSpriteIndex] = new FSprite("Futile_White", true); // not surr why this exist
                    newFSprites[customData.customSpriteIndex].shader = rCam.game.rainWorld.Shaders["FlatLight"];
                    newFSprites[customData.customSpriteIndex + 1] = new FSprite("guardEye", true);
                    for (int i = 0; i < self.numGodPips; i++)
                    {
                        newFSprites[customData.customSpriteIndex + 2 + i] = new FSprite("WormEye", true);
                    }

                    sLeaser.sprites = newFSprites;

                }

            }

            orig(self, sLeaser, rCam, newContatiner);
            if (rCam.room.game.IsArenaSession && !GameUtils.IsChallengeGameSession(rCam.room.game) && rCam.room.game.rainWorld.progression.miscProgressionData.beaten_Saint && self.player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                PlayerCustomData customData = (self.owner as Player).GetCustomData<PlayerCustomData>();

                //rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[12]);
                for (int j = customData.customSpriteIndex; j < sLeaser.sprites.Length; j++)
                {
                    //ConsoleWrite("adding: " + j + ", " + (j - customData.customSpriteIndex) + " / " + sLeaser.sprites.Length + ", " + customData.customSpriteIndex);
                    rCam.ReturnFContainer("HUD2").AddChild(sLeaser.sprites[j]);
                }
            }
        }
    }
}
