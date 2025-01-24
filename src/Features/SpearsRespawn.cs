﻿using ArenaPlus.Lib;
using ArenaPlus.Options;
using ArenaPlus.Options.Tabs;
using ArenaPlus.Utils;
using Menu.Remix.MixedUI;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ArenaPlus.Features
{
    [FeatureInfo(
        id: "spearsRespawn",
        name: "Spears respawn",
        description: "Whether spears reappear when they are all lost",
        enabledByDefault: false
    )]
    public class SpearsRespawn : Feature
    {
        private readonly Configurable<int> spearsRespawnTimerConfigurable = OptionsInterface.instance.config.Bind("spearsRespawnTimer", 30, new ConfigurableInfo("The time in seconds before the spears respawn", new ConfigAcceptableRange<int>(0, 100), "", []));
        private Timer spearsRespawnTimer;
        private int spearsCheckTicks;

        public SpearsRespawn(FeatureInfoAttribute featureInfo) : base(featureInfo)
        {
            SetComplementaryElement((expandable, startPos) =>
            {
                OpUpdown updown = expandable.AddItem(
                    new OpUpdown(spearsRespawnTimerConfigurable, startPos, 60f)
                );
                updown.pos -= new Vector2(0, (updown.size.y - FeaturesTab.CHECKBOX_SIZE) / 2);
                updown.description = spearsRespawnTimerConfigurable.info.description;

                if (HexColor != "None" && ColorUtility.TryParseHtmlString("#" + HexColor, out Color color))
                {
                    updown.colorEdge = color;
                }
            });
        }

        public static void RegisterExecption(Spear spear)
        {
            if (!spear.slatedForDeletetion && spear.room != null)
            {
                RoomCustomData roomData = CustomDataManager.GetCustomData<RoomCustomData>(spear.room);
                roomData.spearsRespawnExecption.Add(spear);
            }
        }

        private static bool IsException(Spear spear)
        {
            if (!spear.slatedForDeletetion && spear.room != null)
            {
                RoomCustomData roomData = CustomDataManager.GetCustomData<RoomCustomData>(spear.room);
                return roomData.spearsRespawnExecption.Contains(spear);
            }
            return false;
        }

        private bool CheckSpearGrability(Spear spear)
        {
            foreach (var AbstPLayer in spear.room.game.AlivePlayers)
            {
                if (AbstPLayer.realizedCreature is Player player && player.Grabability(spear) != Player.ObjectGrabability.CantGrab)
                {
                    return true;
                }
            }
            return false;
        }

        protected override void Register()
        {
            On.ArenaGameSession.Update += ArenaGameSession_Update;
        }

        protected override void Unregister()
        {
            On.ArenaGameSession.Update -= ArenaGameSession_Update;
        }

        private const string timerText = "Spears respawn in";

        private void ArenaGameSession_Update(On.ArenaGameSession.orig_Update orig, ArenaGameSession self)
        {
            spearsCheckTicks++;
            if (!self.initiated && spearsRespawnTimer != null)
            {
                ConsoleWrite("Stop timer: INIT", Color.red);
                UI.ArenaTimer.StopTimer(timerText);
                spearsRespawnTimer.Dispose();
                spearsRespawnTimer = null;
            }
            orig(self);

            bool otherTimer = UI.ArenaTimer.Active && UI.ArenaTimer.Text == timerText;
            if (self.game.session is not SandboxGameSession && !GameUtils.IsChallengeGameSession(self.game) && self.room != null && self.playersSpawned && spearsCheckTicks > 30 && !otherTimer)
            {
                spearsCheckTicks = 0;
                int spearCount = 0;

                if (self.room.physicalObjects[2] != null)
                {
                    for (int i = 0; i < self.room.physicalObjects[2].Count; i++)
                    {
                        PhysicalObject obj = self.room.physicalObjects[2][i];
                        if (obj != null && obj is Spear spear && !spear.slatedForDeletetion && CheckSpearGrability(spear) && !IsException(spear))
                        {
                            //ConsoleWrite($"Visible spear {i} : " + (self.game.cameras[0] as RoomCamera).IsViewedByCameraPosition((self.game.cameras[0] as RoomCamera).currentCameraPosition, obj.firstChunk.pos));
                            if (self.game.cameras[0].IsViewedByCameraPosition(self.game.cameras[0].currentCameraPosition, obj.firstChunk.pos))
                            {
                                spearCount++;
                            }
                        }
                    }
                }

                if (spearCount <= 0 && spearsRespawnTimer == null)
                {
                    LogInfo("Starting spears respawning timer");
                    ConsoleWrite("Start timer", Color.green);

                    if (spearsRespawnTimerConfigurable.Value <= 3)
                    {
                        spearsRespawnTimer = new Timer(x => RespawnTimerEnd(self.room), null, spearsRespawnTimerConfigurable.Value * 1000, 0);
                        UI.ArenaTimer.StartTimer("Spears respawn in", DateTime.Now.AddSeconds(spearsRespawnTimerConfigurable.Value));
                    }
                    else
                    {
                        spearsRespawnTimer = new Timer(x =>
                        {
                            new Timer(y => RespawnTimerEnd(self.room), null, 3 * 1000, 0);
                            LogInfo("Starting spears respawning 3s timer 2");
                            UI.ArenaTimer.StartTimer("Spears respawn in", DateTime.Now.AddSeconds(3));
                        }, null, (spearsRespawnTimerConfigurable.Value - 3) * 1000, 0);
                    }
                }
                else if (spearCount > 0 && spearsRespawnTimer != null)
                {
                    ConsoleWrite($"Stop timer: {spearCount} > 0", Color.red);
                    spearsRespawnTimer.Dispose();
                    spearsRespawnTimer = null;
                    UI.ArenaTimer.StopTimer(timerText);
                }

                ConsoleWrite("spearCount : " + spearCount);
            }
        }

        private void RespawnTimerEnd(Room room)
        {
            ConsoleWrite("RespawnTimerEnd", Color.green);
            if (room != null)
            {
                LogInfo("Respawning spears...");

                //ConsoleWrite($"Start processe of {room.roomSettings.placedObjects.Count} items");

                for (int i = 0; i < room.roomSettings.placedObjects.Count; i++)
                {
                    PlacedObject placedObj = room.roomSettings.placedObjects[i];
                    if (placedObj.data is PlacedObject.MultiplayerItemData && ((placedObj.data as PlacedObject.MultiplayerItemData).type == PlacedObject.MultiplayerItemData.Type.Spear || (placedObj.data as PlacedObject.MultiplayerItemData).type == PlacedObject.MultiplayerItemData.Type.ExplosiveSpear))
                    {
                        //ConsoleWrite("Spawn spear");
                        AbstractSpear spear = new(room.world, null, room.GetWorldCoordinate(placedObj.pos), room.game.GetNewID(), false);
                        spear.RealizeInRoom();
                    }
                }
            }

            if (UI.ArenaTimer.Text == "Spears respawn in")
            {
                UI.ArenaTimer.StopTimer(timerText);
            }
        }
    }
}
