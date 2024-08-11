
using MoreSlugcats;
using UnityEngine;
using Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ArenaSlugcatsConfigurator.Freatures
{
    internal static class ResultMenuSlugcatSelection
    {
        internal static void Register()
        {
            logSource.LogInfo("ResultMenuSlugcatSelection Register");

            On.Menu.PlayerResultBox.Update += PlayerResultBox_Update;  
        }

        public static Player.InputPackage lastInput;

        private static void PlayerResultBox_Update(On.Menu.PlayerResultBox.orig_Update orig, Menu.PlayerResultBox self)
        {
            orig(self);
            if (!self.player.readyForNextRound && RainWorldInstance.processManager.arenaSetup.playerClass[self.player.playerNumber] != null && self.DeadPortraint)
            {
                Player.InputPackage inputPackage = RWInput.PlayerInput(self.player.playerNumber);
                //logSource.LogInfo($"input y: {inputPackage.y}");
                if (inputPackage.y != 0 && lastInput.y == 0)
                {
                    List<SlugcatStats.Name> list = Plugin.GetSlugcatsList();
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

                    RainWorldInstance.processManager.arenaSitting.AddPlayerWithClass(self.player.playerNumber, newName);
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
                lastInput = inputPackage;
            }

        }
    }
}
