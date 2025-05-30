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
        private UI.ArenaTimer.Timer spearsRespawnVisualTimer;
        private int spearsCheckTicks;
        public int lastSpearCount = 0;

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

        private bool RespawnRifles => ModManager.MSC && FeaturesManager.GetFeature("allJokeRifle").configurable.Value;

        private void ArenaGameSession_Update(On.ArenaGameSession.orig_Update orig, ArenaGameSession self)
        {
            spearsCheckTicks++;
            if (!self.initiated && spearsRespawnTimer != null)
            {
                LogDebug("Stop timer: INIT");
                spearsRespawnTimer.Dispose();
                spearsRespawnTimer = null;
                spearsRespawnVisualTimer?.Dispose();
                spearsRespawnVisualTimer = null;
            }
            orig(self);

            if (self.game.session is not SandboxGameSession && !GameUtils.IsChallengeGameSession(self.game) && self.room != null && self.playersSpawned && spearsCheckTicks > 30)
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
                        if (RespawnRifles && obj != null && obj is JokeRifle rifle && self.game.cameras[0].IsViewedByCameraPosition(self.game.cameras[0].currentCameraPosition, obj.firstChunk.pos))
                        {
                            if (rifle.abstractRifle.currentAmmo() > 0)
                            {
                                spearCount++;
                            }
                        }
                    }
                }

                if (spearCount <= 0 && spearsRespawnTimer == null)
                {
                    LogInfo("Starting spears respawning timer");

                    if (spearsRespawnTimerConfigurable.Value <= 3)
                    {
                        spearsRespawnTimer = new Timer(x => RespawnTimerEnd(self.room), null, spearsRespawnTimerConfigurable.Value * 1000, 0);
                        spearsRespawnVisualTimer = UI.ArenaTimer.StartTimer("Spears respawn in", spearsRespawnTimerConfigurable.Value * 40);
                    }
                    else
                    {
                        spearsRespawnTimer = new Timer(x =>
                        {
                            new Timer(y => RespawnTimerEnd(self.room), null, 3 * 1000, 0);
                            LogDebug("Starting spears respawning 3s timer 2");
                            spearsRespawnVisualTimer = UI.ArenaTimer.StartTimer("Spears respawn in", 3 * 40);
                        }, null, (spearsRespawnTimerConfigurable.Value - 3) * 1000, 0);
                    }
                }
                else if (spearCount > 0 && spearsRespawnTimer != null)
                {
                    LogDebug($"Stop timer: {spearCount} > 0", Color.red);
                    spearsRespawnTimer.Dispose();
                    spearsRespawnTimer = null;
                    spearsRespawnVisualTimer?.Dispose();
                    spearsRespawnVisualTimer = null;
                }

                LogDebug("spearCount : " + spearCount);
                lastSpearCount = spearCount;
            }
        }

        private void RespawnTimerEnd(Room room)
        {
            LogDebug("RespawnTimerEnd");
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
                        if (RespawnRifles)
                        {
                            JokeRifle.AbstractRifle.AmmoType ammo = new JokeRifle.AbstractRifle.AmmoType(ExtEnum<JokeRifle.AbstractRifle.AmmoType>.values.entries[Random.Range(0, ExtEnum<JokeRifle.AbstractRifle.AmmoType>.values.entries.Count)]);
                            JokeRifle.AbstractRifle rifle = new JokeRifle.AbstractRifle(room.world, null, room.GetWorldCoordinate(placedObj.pos), room.game.GetNewID(), ammo);
                            rifle.setCurrentAmmo((int)Random.Range(5, 40));
                            room.abstractRoom.AddEntity(rifle);
                            rifle.RealizeInRoom();
                        }
                        else
                        {
                            AbstractSpear spear = new(room.world, null, room.GetWorldCoordinate(placedObj.pos), room.game.GetNewID(), false);
                            spear.RealizeInRoom();
                        }
                    }
                }
            }

            spearsRespawnVisualTimer?.Dispose();
            spearsRespawnVisualTimer = null;
        }

        [MyCommand("getspearcount")]
        private static void WriteSpearCount()
        {
            if (FeaturesManager.TryGetFeature("spearsRespawn", out var feature) && GameUtils.IsCompetitiveSession && feature.configurable.Value && feature is SpearsRespawn spearRespawn)
            {
                ConsoleWrite($"count: {spearRespawn.lastSpearCount}");
                return;
            }

            if (feature == null)
            {
                ConsoleWrite("SpearRespawn not found", Color.red);
            }
            else if (!feature.configurable.Value)
            {
                ConsoleWrite("SpearRespawn not enabled", Color.red);
            }
            else if (!GameUtils.IsCompetitiveSession)
            {
                ConsoleWrite("SpearRespawn is not enabled in this game mode", Color.red);
            }
            else
            {
                ConsoleWrite("SpearRespawn error", Color.red);
            }
        }
    }
}
