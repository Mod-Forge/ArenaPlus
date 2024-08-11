
using MoreSlugcats;
using UnityEngine;
using Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Random = UnityEngine.Random;

namespace ArenaSlugcatsConfigurator.Freatures
{
    internal static class ResultMenuSlugcatSelection
    {
        internal static void Register()
        {
            logSource.LogInfo("ResultMenuSlugcatSelection Register");

            On.Menu.PlayerResultBox.Update += PlayerResultBox_Update;
            On.Menu.PlayerResultBox.ctor += PlayerResultBox_ctor;
        }

        public static Player.InputPackage[] lastInput = new Player.InputPackage[4];
        public static int[] randomSeeds = new int[4];
        public static SlugcatStats.Name[] lastCharactersNames = new SlugcatStats.Name[4];


        private static void PlayerResultBox_ctor(On.Menu.PlayerResultBox.orig_ctor orig, PlayerResultBox self, Menu.Menu menu, MenuObject owner, Vector2 pos, Vector2 size, ArenaSitting.ArenaPlayer player, int index)
        {
            orig(self, menu, owner, pos, size, player, index);
            randomSeeds[player.playerNumber] = (int)(Random.value * 100);
            lastCharactersNames[player.playerNumber] = player.playerClass;
        }

        private static void PlayerResultBox_Update(On.Menu.PlayerResultBox.orig_Update orig, Menu.PlayerResultBox self)
        {
            orig(self);
            if (!self.player.readyForNextRound && RainWorldInstance.processManager.arenaSetup.playerClass[self.player.playerNumber] != null && self.DeadPortraint && self is not FinalResultbox)
            {
                Player.InputPackage inputPackage = RWInput.PlayerInput(self.player.playerNumber);
                //logSource.LogInfo($"input y: {inputPackage.y}");
                if (inputPackage.y != 0 && lastInput[self.player.playerNumber].y == 0)
                {
                    List<SlugcatStats.Name> fullList = Plugin.GetSlugcatsList();

                    System.Random rand = new System.Random(randomSeeds[self.player.playerNumber]);
                    //Random.InitState(randomSeeds[self.player.playerNumber]);
                    List<SlugcatStats.Name> list = new List<SlugcatStats.Name>();

                    if (true)
                    {
                        list.Add(lastCharactersNames[self.player.playerNumber]);
                        logSource.LogInfo($"seed: {randomSeeds[self.player.playerNumber]}");

                        int emergencyCountdown = 100;
                        while (list.Count < 4 && emergencyCountdown > 0)
                        {
                            SlugcatStats.Name name = fullList[rand.Next(0, fullList.Count - 1)];
                            if (!list.Contains(name))
                            {
                                list.Add(name);
                            }
                            emergencyCountdown--;
                        }
                        logSource.LogInfo($"list size: {list.Count} in {100 - emergencyCountdown} try");
                    }
                    else
                    {
                        list = fullList;
                    }


                    int index = 0;
                    if (list.Contains(self.player.playerClass))
                    {
                        index = list.IndexOf(self.player.playerClass);
                        logSource.LogInfo($"found index of {self.player.playerClass}: " + index);
                    }
                    index += inputPackage.y;
                    if (index >= list.Count)
                        index = 0;
                    if (index < 0)
                        index = list.Count - 1;
                    logSource.LogInfo("index: " + index);


                    SlugcatStats.Name newName = list[index];

                    self.player.playerClass = newName;
                    //self.portrait.sprite.SetElementByName(string.Concat(new string[]
                    //{
                    //    "MultiplayerPortrait",
                    //    self.player.playerNumber.ToString(),
                    //    "1",
                    //    "-",
                    //    newName.value
                    //}));

                    Menu.Menu menu = self.portrait.menu;
                    self.portrait.RemoveSprites();
                    self.subObjects.Remove(self.portrait);

                    self.portrait = new MenuIllustration(menu, self, "", string.Concat(new string[]
                    {
                        "MultiplayerPortrait",
                        self.player.playerNumber.ToString(),
                        "1",
                        "-",
                        newName.value
                    }), new Vector2(self.originalSize.y / 2f, self.originalSize.y / 2f), true, true);

                    self.subObjects.Add(self.portrait);
                }
                lastInput[self.player.playerNumber] = inputPackage;
            }

        }
    }
}
