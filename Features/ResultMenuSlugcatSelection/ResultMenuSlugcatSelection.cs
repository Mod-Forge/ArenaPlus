
using MoreSlugcats;
using UnityEngine;
using Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Random = UnityEngine.Random;
using ArenaSlugcatsConfigurator.Features.ResultMenuSlugcatSelection;

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

        public static int SelectionCount = 3;

        public static Player.InputPackage[] lastInput = new Player.InputPackage[SelectionCount];
        public static int[] randomSeeds = new int[SelectionCount];
        public static SlugcatStats.Name[] lastCharactersNames = new SlugcatStats.Name[SelectionCount];


        private static void PlayerResultBox_ctor(On.Menu.PlayerResultBox.orig_ctor orig, PlayerResultBox self, Menu.Menu menu, MenuObject owner, Vector2 pos, Vector2 size, ArenaSitting.ArenaPlayer player, int index)
        {
            orig(self, menu, owner, pos, size, player, index);
            if (!Options.enableResultMenuSelection.Value) return;
            PlayerResultBoxCustomData data = self.GetCustomData<PlayerResultBoxCustomData>();
            data.scrollUpButton = new VisualScrollButton(menu, self, "UP", new Vector2(0.01f - 30f, (0.01f + self.size.y) - 40), 0);
            data.scrollUpButton.inactive = false;
            self.subObjects.Add(data.scrollUpButton);
            data.scrollDownButton = new VisualScrollButton(menu, self, "DOWN", new Vector2(0.01f - 30f, -25.99f + 40), 2);
            data.scrollUpButton.inactive = false;
            self.subObjects.Add(data.scrollDownButton);

            randomSeeds[player.playerNumber] = (int)(Random.value * 100);
            lastCharactersNames[player.playerNumber] = player.playerClass;
        }

        private static void PlayerResultBox_Update(On.Menu.PlayerResultBox.orig_Update orig, Menu.PlayerResultBox self)
        {
            orig(self);
            if (!Options.enableResultMenuSelection.Value) return;
            PlayerResultBoxCustomData data = self.GetCustomData<PlayerResultBoxCustomData>();
            if (RainWorldInstance.processManager.arenaSetup.playerClass[self.player.playerNumber] != null && self.DeadPortraint && self is not FinalResultbox)
            {
                data.scrollUpButton.greyedOut = self.player.readyForNextRound;
                data.scrollDownButton.greyedOut = self.player.readyForNextRound;


                if (!self.player.readyForNextRound)
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
                            while (list.Count < SelectionCount && emergencyCountdown > 0)
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
                        self.menu.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);

                        //if (inputPackage.y > 0)
                        //data.scrollDownButton.Bump();
                    }
                    lastInput[self.player.playerNumber] = inputPackage;
                }
            }
            else
            {
                data.scrollUpButton.RemoveSprites();
                self.subObjects.Remove(data.scrollUpButton);
                data.scrollDownButton.RemoveSprites();
                self.subObjects.Remove(data.scrollDownButton);
            }
        }
    }
}
