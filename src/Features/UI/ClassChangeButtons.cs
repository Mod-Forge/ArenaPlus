using ArenaPlus.Lib;
using ArenaPlus.Utils;
using Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArenaPlus.Features.UI
{
    [ImmutableFeature]
    file class ClassChangeButtons : ImmutableFeature
    {
        protected override void Register()
        {
            On.Menu.MultiplayerMenu.Singal += MultiplayerMenu_Singal;
            On.Menu.MultiplayerMenu.InitiateGameTypeSpecificButtons += MultiplayerMenu_InitiateGameTypeSpecificButtons;
            On.Menu.MultiplayerMenu.ClearGameTypeSpecificButtons += MultiplayerMenu_ClearGameTypeSpecificButtons;
        }

        private void MultiplayerMenu_ClearGameTypeSpecificButtons(On.Menu.MultiplayerMenu.orig_ClearGameTypeSpecificButtons orig, MultiplayerMenu self)
        {
            if (self.playerClassButtons != null)
            {
                MultiplayerMenuData data = self.GetCustomData<MultiplayerMenuData>();
                foreach (var buttons in data.nextClassButtons)
                {
                    buttons.RemoveSprites();
                    self.pages[0].RemoveSubObject(buttons);
                }
                data.nextClassButtons = null;

                foreach (var buttons in data.previousClassButtons)
                {
                    buttons.RemoveSprites();
                    self.pages[0].RemoveSubObject(buttons);
                }
                data.previousClassButtons = null;
            }
            orig(self);
        }

        private void MultiplayerMenu_InitiateGameTypeSpecificButtons(On.Menu.MultiplayerMenu.orig_InitiateGameTypeSpecificButtons orig, Menu.MultiplayerMenu self)
        {
            orig(self);

            if (self.playerClassButtons == null) return;

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
                LogError(e);
            }
        }

        private void MultiplayerMenu_Singal(On.Menu.MultiplayerMenu.orig_Singal orig, Menu.MultiplayerMenu self, Menu.MenuObject sender, string message)
        {
            if (message != null && self.currentGameType == ArenaSetup.GameTypeID.Competitive || self.currentGameType == ArenaSetup.GameTypeID.Sandbox)
            {
                for (int num6 = 0; num6 < self.playerClassButtons.Length; num6++)
                {
                    if (message == "PREVIOUSCLASS" + num6.ToString())
                    {
                        self.GetArenaSetup.playerClass[num6] = PreviousClass(self, self.GetArenaSetup.playerClass[num6]);
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

        public static SlugcatStats.Name PreviousClass(Menu.MultiplayerMenu menu, SlugcatStats.Name curClass)
        {
            SlugcatStats.Name name;
            if (curClass == null)
            {
                //LogDebug(data: $"go from random to last({ExtEnum<SlugcatStats.Name>.values.Count - 1})");
                name = new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(ExtEnum<SlugcatStats.Name>.values.Count - 1), false);
            }
            else
            {
                if (curClass.Index < 1)
                {
                    //LogDebug($"go from first({curClass.Index}) to random");
                    return null;
                }
                //LogDebug($"go from {curClass.Index} to {curClass.Index - 1}");
                name = new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(curClass.Index - 1), false);
            }

            if (!SlugcatsUtils.IsSlugcatUnlocked(name) || (!FeaturesManager.GetFeature("keepSlugcatsSelectable").Enabled && !SlugcatsUtils.IsSlugcatEnabled(name)))
            {
                return PreviousClass(menu, name);
            }
            if (name != SlugcatStats.Name.White && name != SlugcatStats.Name.Yellow && !menu.multiplayerUnlocks.ClassUnlocked(name))
            {
                return PreviousClass(menu, name);
            }
            if (name != null && Input.GetKey(KeyCode.LeftControl)) return null;
            return name;
        }
    }

    file class MultiplayerMenuData : CustomData
    {
        public SymbolButton[] nextClassButtons;
        public SymbolButton[] previousClassButtons;

        // Constructeur par défaut
        public MultiplayerMenuData() : base(null) { }

        // Autres constructeurs si nécessaire
        public MultiplayerMenuData(object obj) : base(obj) { }
    }
}
