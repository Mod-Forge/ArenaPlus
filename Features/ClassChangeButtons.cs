
using MoreSlugcats;
using UnityEngine;
using RWCustom;
using ArenaSlugcatsConfigurator.Features.ResultMenuSlugcatSelection;
using System;
using Menu;

namespace ArenaSlugcatsConfigurator.Features
{
    internal class ClassChangeButtons
    {
        internal static void Register()
        {
            logSource.LogInfo("ClassChangeButtons Register");
            On.Menu.MultiplayerMenu.Singal += MultiplayerMenu_Singal;
            On.Menu.MultiplayerMenu.InitiateGameTypeSpecificButtons += MultiplayerMenu_InitiateGameTypeSpecificButtons;
            //On.Menu.MultiplayerMenu.CustomUpdateInfoText += MultiplayerMenu_CustomUpdateInfoText;
        }

        private static void MultiplayerMenu_ClearGameTypeSpecificButtons(On.Menu.MultiplayerMenu.orig_ClearGameTypeSpecificButtons orig, MultiplayerMenu self)
        {
            MultiplayerMenuData data = self.GetCustomData<MultiplayerMenuData>();
            if (data.nextClassButtons != null)
            {
                for (int j = 0; j < data.nextClassButtons.Length; j++)
                {
                    data.nextClassButtons[j].RemoveSprites();
                    self.pages[0].RemoveSubObject(data.nextClassButtons[j]);
                }
                data.nextClassButtons = null;
            }
            if (data.previousClassButtons != null)
            {
                for (int j = 0; j < data.previousClassButtons.Length; j++)
                {
                    data.previousClassButtons[j].RemoveSprites();
                    self.pages[0].RemoveSubObject(data.previousClassButtons[j]);
                }
                data.previousClassButtons = null;
            }
            orig(self);
        }

        private static string MultiplayerMenu_CustomUpdateInfoText(On.Menu.MultiplayerMenu.orig_CustomUpdateInfoText orig, Menu.MultiplayerMenu self)
        {
            MultiplayerMenuData data = self.GetCustomData<MultiplayerMenuData>();
            return orig(self);
        }

        private static void MultiplayerMenu_InitiateGameTypeSpecificButtons(On.Menu.MultiplayerMenu.orig_InitiateGameTypeSpecificButtons orig, Menu.MultiplayerMenu self)
        {
            orig(self);
            try
            {
                MultiplayerMenuData data = self.GetCustomData<MultiplayerMenuData>();
                data.nextClassButtons = new SymbolButton[4];
                data.previousClassButtons = new SymbolButton[4];
                for (int i = 0; i < self.playerClassButtons.Length; i++)
                {
                    data.nextClassButtons[i] = new SymbolButton(self, self.pages[0], "Menu_Symbol_Arrow", "CLASSCHANGE" + i.ToString(), self.playerClassButtons[i].pos + new Vector2(self.playerClassButtons[i].size.x - 23f, 145f));
                    data.nextClassButtons[i].symbolSprite.rotation = 90;
                    data.previousClassButtons[i] = new SymbolButton(self, self.pages[0], "Menu_Symbol_Arrow", "PREVIOUSCLASS" + i.ToString(), self.playerClassButtons[i].pos + new Vector2(0f, 145f));
                    data.previousClassButtons[i].symbolSprite.rotation = -90;


                    self.pages[0].subObjects.AddRange([data.nextClassButtons[i], data.previousClassButtons[i]]);
                    self.ResetSelection();
                }
            }
            catch (Exception e)
            {
                Plugin.logSource.LogError(e);
            }

        }

        private static void MultiplayerMenu_Singal(On.Menu.MultiplayerMenu.orig_Singal orig, Menu.MultiplayerMenu self, Menu.MenuObject sender, string message)
        {
            Plugin.logSource.LogInfo("got message: " + message);
            if (message != null && self.currentGameType == ArenaSetup.GameTypeID.Competitive || self.currentGameType == ArenaSetup.GameTypeID.Sandbox)
            {
                for (int num6 = 0; num6 < self.playerClassButtons.Length; num6++)
                {
                    if (message == "PREVIOUSCLASS" + num6.ToString())
                    {
                        self.GetArenaSetup.playerClass[num6] = Plugin.PreviousClass(self, self.GetArenaSetup.playerClass[num6]);
                        self.playerClassButtons[num6].menuLabel.text = self.Translate(SlugcatStats.getSlugcatName(self.GetArenaSetup.playerClass[num6]));
                        self.playerJoinButtons[num6].portrait.fileName = self.ArenaImage(self.GetArenaSetup.playerClass[num6], num6);
                        self.playerJoinButtons[num6].portrait.LoadFile();
                        self.playerJoinButtons[num6].portrait.sprite.SetElementByName(self.playerJoinButtons[num6].portrait.fileName);
                        self.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
                    }
                }
            }
            orig(self, sender, message);
        }

        public class MultiplayerMenuData : CustomData
        {
            public SymbolButton[] nextClassButtons;
            public SymbolButton[] previousClassButtons;

            // Constructeur par défaut
            public MultiplayerMenuData() : base(null) { }

            // Autres constructeurs si nécessaire
            public MultiplayerMenuData(object obj) : base(obj) { }
        }
    }
}
