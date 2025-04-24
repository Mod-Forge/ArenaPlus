using ArenaBehaviors;
using ArenaPlus.Lib;
using ArenaPlus.Options;
using ArenaPlus.Options.Tabs;
using ArenaPlus.Utils;
using Menu.Remix.MixedUI;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArenaPlus.Features
{
    [FeatureInfo(
        id: "spawnKillProtection",
        name: "Start timer",
        description: "Add a spawn kill protection timer at the start of each rounds",
        enabledByDefault: false
    )]
    file class SpawnKillProtection : Feature
    {
        public static readonly Configurable<int> spawnKillProtectionTimerConfigurable = OptionsInterface.instance.config.Bind("spawnKillProtectionTimer", 5, new ConfigurableInfo("The time in seconds of the spawn kill protection", new ConfigAcceptableRange<int>(1, 100), "", []));


        public SpawnKillProtection(FeatureInfoAttribute featureInfo) : base(featureInfo)
        {
            SetComplementaryElement((expandable, startPos) =>
            {
                OpUpdown updown = expandable.AddItem(
                    new OpUpdown(spawnKillProtectionTimerConfigurable, startPos, 60f)
                );
                updown.pos -= new Vector2(0, (updown.size.y - FeaturesTab.CHECKBOX_SIZE) / 2);
                updown.description = spawnKillProtectionTimerConfigurable.info.description;

                if (HexColor != "None" && ColorUtility.TryParseHtmlString("#" + HexColor, out Color color))
                {
                    updown.colorEdge = color;
                }
            });
        }

        protected override void Register()
        {
            On.ArenaGameSession.ctor += ArenaGameSession_ctor;
            On.Player.Die += Player_Die;
            On.Player.Destroy += Player_Destroy;
            On.RWInput.PlayerInputLogic_int_int += RWInput_PlayerInputLogic_int_int;
            On.Player.ThrowObject += Player_ThrowObject;
        }

        protected override void Unregister()
        {
            On.ArenaGameSession.ctor -= ArenaGameSession_ctor;
            On.Player.Die -= Player_Die;
            On.Player.Destroy -= Player_Destroy;
            On.RWInput.PlayerInputLogic_int_int -= RWInput_PlayerInputLogic_int_int;
            On.Player.ThrowObject -= Player_ThrowObject;
        }

        private void Player_ThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
        {
            if (SpawnProtectionTimerBehavior.protection)
            {
                return;
            }
            orig(self, grasp, eu);
        }

        private Player.InputPackage RWInput_PlayerInputLogic_int_int(On.RWInput.orig_PlayerInputLogic_int_int orig, int categoryID, int playerNumber)
        {
            var input = orig(categoryID, playerNumber);
            if (SpawnProtectionTimerBehavior.protection && input.thrw)
            {
                foreach (var abstPlayer in GameUtils.rainWorldGame.Players)
                {
                    if (abstPlayer.realizedCreature is Player player && player.playerState.playerNumber == playerNumber && player.TryGetAttachedFeatureType<SpawnProtectionMessage>(out var feature))
                    {
                        feature.Throw();

                    }
                }

                input.thrw = false;
            }
            return input;
        }

        private bool forceKill;
        private void Player_Die(On.Player.orig_Die orig, Player self)
        {
            if (SpawnProtectionTimerBehavior.protection && !forceKill) { return; }
            orig(self);
            forceKill = false;
        }

        private void Player_Destroy(On.Player.orig_Destroy orig, Player self)
        {
            if (SpawnProtectionTimerBehavior.protection)
            {
                forceKill = true;
                self.Die();
            }
            orig(self);
        }

        private void ArenaGameSession_ctor(On.ArenaGameSession.orig_ctor orig, ArenaGameSession self, RainWorldGame game)
        {
            orig(self, game);
            if (self is not SandboxGameSession) // || self.arenaSitting.sandboxPlayMode
            {
                self.AddBehavior(new SpawnProtectionTimerBehavior(self));
            }
        }
    }

    file class SpawnProtectionTimerBehavior : ArenaGameBehavior
    {
        private const string timerText = "Figth start in";
        private static UI.ArenaTimer.Timer timer;
        public static bool protection => timer != null && !timer.Done;

        public SpawnProtectionTimerBehavior(ArenaGameSession gameSession) : base(gameSession)
        {
        }

        public override void Initiate()
        {
            timer = UI.ArenaTimer.StartTimer(timerText, (SpawnKillProtection.spawnKillProtectionTimerConfigurable.Value + 1) * 40, true);
            foreach (var abstPlayer in game.Players)
            {
                //LogInfo("player", abstPlayer.realizedCreature);
                if (abstPlayer.realizedCreature is Player player)
                {
                    player.AddAttachedFeature(new SpawnProtectionMessage());
                }
            }
        }

        public override void Update()
        {
            if (!protection) return;

            foreach (var shortCut in game.shortcuts.transportVessels)
            {
                if (shortCut.creature is not Player)
                {
                    shortCut.wait = 4;
                }
            }
        }

        public override void Destroy()
        {
            base.Destroy();
        }
    }

    file class SpawnProtectionMessage : PlayerCosmeticFeature
    {
        private static string[] messages = ["Nuh hh", "Nope", "Try again", "Try later", "Skill issue", "Can't do that"];
        public int showCountdown;
        public FLabel label = new FLabel(Custom.GetFont(), "some message");
        public Vector2 pos;
        public SpawnProtectionMessage() : base()
        {
        }

        public void Throw()
        {
            if (showCountdown > 0) return;
            label.text = messages[Random.Range(0, messages.Length)];
            pos = player.mainBodyChunk.pos + new Vector2(0, 20);
            showCountdown = 40;
            room.PlaySound(SoundID.MENU_Error_Ping, pos, 5f, Random.Range(0.7f, 1.25f));
        }

        public override void Update(bool eu)
        {
            if (showCountdown > 0) showCountdown--;
            if (!SpawnProtectionTimerBehavior.protection)
            {
                Destroy();
                return;
            }
            base.Update(eu);
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[0];
            rCam.ReturnFContainer("HUD").AddChild(label);
            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            label.SetPosition(pos - camPos);
            label.alpha = Mathf.Clamp01(showCountdown / 20f);

            if (!sLeaser.deleteMeNextFrame && (base.slatedForDeletetion || this.room != rCam.room))
            {
                label.RemoveFromContainer();
            }
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }
    }
}
