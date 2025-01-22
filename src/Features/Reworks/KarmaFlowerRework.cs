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
using static ArenaPlus.Lib.AttachedPlayerFeatureUtils;

namespace ArenaPlus.Features.Reworks
{
    [FeatureInfo(
        id: "karmaFlowerRework",
        name: "Saint karma flower (Spoiler)",
        color: "ffff00",
        category: BuiltInCategory.Spoilers,
        description: "Allow players to ascend (Saint spoiler)",
        enabledByDefault: false
    )]
    file class KarmaFlowerRework(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
    {
        protected override void Register()
        {
            Log("Enabling karma flower rework");
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            On.Player.ClassMechanicsSaint += Player_ClassMechanicsSaint;
            On.KarmaFlower.BitByPlayer += KarmaFlower_BitByPlayer;
            On.Spear.HitSomethingWithoutStopping += Spear_HitSomethingWithoutStopping;
            On.Player.ActivateAscension += Player_ActivateAscension;
        }

        protected override void Unregister()
        {
            Log("Disabling karma flower rework");
            On.PlayerGraphics.DrawSprites -= PlayerGraphics_DrawSprites;
            On.Player.ClassMechanicsSaint -= Player_ClassMechanicsSaint;
            On.KarmaFlower.BitByPlayer -= KarmaFlower_BitByPlayer;
            On.Spear.HitSomethingWithoutStopping -= Spear_HitSomethingWithoutStopping;
        }

        private void Player_ActivateAscension(On.Player.orig_ActivateAscension orig, Player self)
        {
            if (GameUtils.IsCompetitiveOrSandboxSession && self.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint && !self.HasAttachedFeatureType<KarmaFlowerPowerVisual>())
            {
                self.AddAttachedFeature(new KarmaFlowerPowerVisual(self));
            }
            orig(self);
        }

        private void Spear_HitSomethingWithoutStopping(On.Spear.orig_HitSomethingWithoutStopping orig, Spear self, PhysicalObject obj, BodyChunk chunk, PhysicalObject.Appendage appendage)
        {
            if (GameUtils.IsCompetitiveOrSandboxSession && self.room.game.rainWorld.progression.miscProgressionData.beaten_Saint)
            {
                if (self.Spear_NeedleCanFeed())
                {
                    if (obj is KarmaFlower)
                    {
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
            if (GameUtils.IsCompetitiveOrSandboxSession && self.room.game.rainWorld.progression.miscProgressionData.beaten_Saint && self.bites < 2)
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
            if (GameUtils.IsCompetitiveOrSandboxSession && self.room.game.rainWorld.progression.miscProgressionData.beaten_Saint)
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
            KarmaFlowerPowerVisual powerVisual = self?.player.GetAttachedFeatureType<KarmaFlowerPowerVisual>();
            if (GameUtils.IsCompetitiveOrSandboxSession && rCam.room.game.rainWorld.progression.miscProgressionData.beaten_Saint && powerVisual != null && powerVisual.sLeaser != null)
            {
                if (!rCam.room.game.DEBUGMODE && ModManager.MSC && self.player.room != null && self.player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint)
                {
                    if (self.player.killFac > 0f || self.player.forceBurst)
                    {
                        powerVisual.sLeaser.sprites[0].isVisible = true;
                        powerVisual.sLeaser.sprites[0].x = sLeaser.sprites[3].x + self.player.burstX;
                        powerVisual.sLeaser.sprites[0].y = sLeaser.sprites[3].y + self.player.burstY + 60f;
                        float f = Mathf.Lerp(self.player.lastKillFac, self.player.killFac, timeStacker);
                        powerVisual.sLeaser.sprites[0].scale = Mathf.Lerp(50f, 2f, Mathf.Pow(f, 0.5f));
                        powerVisual.sLeaser.sprites[0].alpha = Mathf.Pow(f, 3f);
                    }
                    else
                    {
                        powerVisual.sLeaser.sprites[0].isVisible = false;
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
                    powerVisual.sLeaser.sprites[0 + 1].x = self.rubberMarkX;
                    powerVisual.sLeaser.sprites[0 + 1].y = self.rubberMarkY + 60f;
                    float num12;
                    if (self.player.monkAscension)
                    {
                        sLeaser.sprites[9].color = Custom.HSL2RGB(Random.value, Random.value, Random.value);
                        sLeaser.sprites[10].alpha = 0f;
                        sLeaser.sprites[11].alpha = 0f;
                        powerVisual.sLeaser.sprites[0 + 1].color = sLeaser.sprites[9].color;
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
                                powerVisual.sLeaser.sprites[0 + 2 + m].scale = 0f;
                            }
                            else if (self.player.godTimer >= num17)
                            {
                                powerVisual.sLeaser.sprites[0 + 2 + m].scale = 1f;
                            }
                            else
                            {
                                powerVisual.sLeaser.sprites[0 + 2 + m].scale = (self.player.godTimer - num16) / num15;
                            }
                            if (self.player.karmaCharging > 0 && self.player.monkAscension)
                            {
                                powerVisual.sLeaser.sprites[0 + 2 + m].color = sLeaser.sprites[9].color;
                            }
                            else
                            {
                                powerVisual.sLeaser.sprites[0 + 2 + m].color = PlayerGraphics.SlugcatColor(self.CharacterForColor);
                            }
                        }
                    }
                    else
                    {
                        num13 = 0f;
                    }

                    self.rubberMarkX = sLeaser.sprites[0].x;
                    self.rubberMarkY = sLeaser.sprites[0].y;


                    powerVisual.sLeaser.sprites[0 + 1].x = self.rubberMarkX + self.rubberMouseX;
                    powerVisual.sLeaser.sprites[0 + 1].y = self.rubberMarkY + 60f + self.rubberMouseY;
                    self.rubberAlphaEmblem += (num12 - self.rubberAlphaEmblem) * 0.05f;
                    self.rubberAlphaPips += (num13 - self.rubberAlphaPips) * 0.05f;
                    powerVisual.sLeaser.sprites[0 + 1].alpha = self.rubberAlphaEmblem;
                    sLeaser.sprites[10].alpha *= 1f - self.rubberAlphaPips;
                    sLeaser.sprites[11].alpha *= 1f - self.rubberAlphaPips;
                    for (int n = 0 + 2; n < 0 + 2 + self.numGodPips; n++)
                    {
                        powerVisual.sLeaser.sprites[n].alpha = self.rubberAlphaPips;
                        Vector2 vector14 = new Vector2(powerVisual.sLeaser.sprites[0 + 1].x, powerVisual.sLeaser.sprites[0 + 1].y);
                        vector14 += Custom.rotateVectorDeg(Vector2.one * self.rubberRadius, (n - 15) * (360f / self.numGodPips));
                        powerVisual.sLeaser.sprites[n].x = vector14.x;
                        powerVisual.sLeaser.sprites[n].y = vector14.y;
                    }


                }
            }


        }
    }

    public class KarmaFlowerPowerVisual : AttachedPlayerFeature, IDrawable
    {
        public KarmaFlowerPowerVisual(Player player) : base(player)
        {
        }

        public RoomCamera.SpriteLeaser sLeaser;
        public const int numGodPips = 12;

        // from player graphics
        public float rubberMarkX;
        public float rubberMarkY;
        public float rubberMouseX;
        public float rubberMouseY;
        public float rubberRadius;
        public float rubberAlphaPips;
        public float rubberAlphaEmblem;
        public SlugcatStats.Name CharacterForColor
        {
            get
            {
                if (this.player.room != null && this.player.room.game.setupValues.arenaDefaultColors)
                {
                    return this.player.SlugCatClass;
                }
                return this.player.playerState.slugcatCharacter;
            }
        }



        public override void Update(bool eu)
        {
            base.Update(eu);

            if (player.slatedForDeletetion || player.dead || player.godTimer <= 0)
            {
                Destroy();
                return;
            }
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[2 + numGodPips];

            sLeaser.sprites[0] = new FSprite("Futile_White", true);
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["FlatLight"];
            sLeaser.sprites[1] = new FSprite("guardEye", true);
            for (int i = 0; i < numGodPips; i++)
            {
                sLeaser.sprites[2 + i] = new FSprite("WormEye", true);
            }
            AddToContainer(sLeaser, rCam, null);
            this.sLeaser = sLeaser;
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            newContatiner ??= rCam.ReturnFContainer("HUD2");

            foreach (FSprite fsprite in sLeaser.sprites)
            {
                fsprite.RemoveFromContainer();
                newContatiner.AddChild(fsprite);
            }
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (slatedForDeletetion || room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }
    }
}
