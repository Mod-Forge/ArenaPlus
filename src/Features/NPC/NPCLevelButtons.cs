using ArenaPlus.Lib;
using ArenaPlus.Utils;
using Menu;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArenaPlus.Features.NPC
{
    [ImmutableFeature]
    internal class NPCLevelButtons : ImmutableFeature
    {
        public static int[] NPCLevels = [4, 4, 4, 4];

        protected override void Register()
        {
            if (!ModManager.MSC)
                return;
            On.Menu.MultiplayerMenu.Singal += MultiplayerMenu_Singal;
            On.Menu.MultiplayerMenu.ClearGameTypeSpecificButtons += MultiplayerMenu_ClearGameTypeSpecificButtons;
            On.Menu.MultiplayerMenu.InitiateGameTypeSpecificButtons += MultiplayerMenu_InitiateGameTypeSpecificButtons;
        }

        private void MultiplayerMenu_InitiateGameTypeSpecificButtons(On.Menu.MultiplayerMenu.orig_InitiateGameTypeSpecificButtons orig, MultiplayerMenu self)
        {
            orig(self);

            if (!ModManager.NewSlugcatsModule) return;

            if (self.playerClassButtons == null) return;

            MultiplayerMenuData data = self.GetCustomData<MultiplayerMenuData>();
            data.levelChangeButtons = new SimpleButton[4];
            for (int i = 0; i < 4; i++)
            {
                if (self.GetArenaSetup.playerClass[i] == NPCFeature.NPCName)
                    InitNPCLevelButton(self, i);
            }
        }

        private void MultiplayerMenu_ClearGameTypeSpecificButtons(On.Menu.MultiplayerMenu.orig_ClearGameTypeSpecificButtons orig, MultiplayerMenu self)
        {
            if (!ModManager.NewSlugcatsModule)
            {
                orig(self);
                return;
            }
            try
            {
                if (self.playerClassButtons != null)
                {
                    MultiplayerMenuData data = self.GetCustomData<MultiplayerMenuData>();

                    foreach (var buttons in data.levelChangeButtons)
                    {
                        if (buttons == null)
                            continue;
                        buttons.RemoveSprites();
                        self.pages[0].RemoveSubObject(buttons);
                    }
                    data.levelChangeButtons = null;
                }
            }
            catch (Exception e)
            {
                LogError(e);
            }
            orig(self);
        }

        private void MultiplayerMenu_Singal(On.Menu.MultiplayerMenu.orig_Singal orig, Menu.MultiplayerMenu self, Menu.MenuObject sender, string message)
        {
            //data.levelChangeButtons[i] = new SimpleButton(self, self.pages[0], $"LV {NPCLevels[i] + 1}", "NPCLEVELCHANGE" + i.ToString(), self.playerClassButtons[i].pos + new Vector2(self.playerClassButtons[i].size.x - 79, 145f), new Vector2(45f, 25f));

            //self.pages[0].subObjects.AddRange([data.levelChangeButtons[i]]);
            //self.ResetSelection();
            orig(self, sender, message);

            if (!ModManager.NewSlugcatsModule)
            {
                return;
            }
            try
            {
                if (message != null && self.currentGameType == ArenaSetup.GameTypeID.Competitive || self.currentGameType == ArenaSetup.GameTypeID.Sandbox)
                {
                    for (int i = 0; i < self.playerClassButtons.Length; i++)
                    {
                        if (message == "PREVIOUSCLASS" + i.ToString() || message == "CLASSCHANGE" + i.ToString())
                        {
                            float x = (self.CurrLang == InGameTranslator.LanguageID.German ? 140f : 120f) - 20f;
                            if (self.GetArenaSetup.playerClass[i] != NPCFeature.NPCName && self.playerClassButtons[i].size.x != x)
                            {
                                RemoveNPCLevelButton(self, i);
                            }
                            else if (self.GetArenaSetup.playerClass[i] == NPCFeature.NPCName && self.playerClassButtons[i].size.x == x)
                            {

                                InitNPCLevelButton(self, i);

                            }
                        }

                        if (message == "NPCLEVELCHANGE" + i.ToString())
                        {
                            MultiplayerMenuData data = self.GetCustomData<MultiplayerMenuData>();
                            NPCLevels[i]++;
                            if (NPCLevels[i] > 8)
                                NPCLevels[i] = 0;
                            data.levelChangeButtons[i].menuLabel.text = $"LV {NPCLevels[i] + 1}";
                            self.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }


        private void InitNPCLevelButton(MultiplayerMenu menu, int index)
        {
            MultiplayerMenuData data = menu.GetCustomData<MultiplayerMenuData>();
            float x = (menu.CurrLang == InGameTranslator.LanguageID.German ? 140f : 120f) - 20f;

            menu.playerClassButtons[index].RemoveSprites();
            menu.pages[0].RemoveSubObject(menu.playerClassButtons[index]);

            menu.playerClassButtons[index] = new SimpleButton(menu, menu.pages[0], menu.playerClassButtons[index].menuLabel.text, menu.playerClassButtons[index].signalText, menu.playerClassButtons[index].pos, new Vector2((x / 2f), menu.playerClassButtons[index].size.y));
            menu.pages[0].subObjects.Add(menu.playerClassButtons[index]);


            data.levelChangeButtons[index] = new SimpleButton(menu, menu.pages[0], $"LV {NPCLevels[index] + 1}", "NPCLEVELCHANGE" + index.ToString(), menu.playerClassButtons[index].pos + new Vector2(menu.playerClassButtons[index].size.x + 5f, 0f), menu.playerClassButtons[index].size);

            menu.pages[0].subObjects.AddRange([data.levelChangeButtons[index]]);
            menu.ResetSelection();
        }

        private void RemoveNPCLevelButton(MultiplayerMenu menu, int index)
        {
            MultiplayerMenuData data = menu.GetCustomData<MultiplayerMenuData>();
            float x = (menu.CurrLang == InGameTranslator.LanguageID.German ? 140f : 120f) - 20f;


            data.levelChangeButtons ??= new SimpleButton[4];
            menu.playerClassButtons[index].RemoveSprites();
            menu.pages[0].RemoveSubObject(menu.playerClassButtons[index]);

            menu.playerClassButtons[index] = new SimpleButton(menu, menu.pages[0], menu.playerClassButtons[index].menuLabel.text, menu.playerClassButtons[index].signalText, menu.playerClassButtons[index].pos, new Vector2(x, menu.playerClassButtons[index].size.y));
            menu.pages[0].subObjects.Add(menu.playerClassButtons[index]);

            if (data.levelChangeButtons[index] != null)
            {
                data.levelChangeButtons[index].RemoveSprites();
                menu.pages[0].RemoveSubObject(data.levelChangeButtons[index]);
                data.levelChangeButtons[index] = null;
            }
        }
    }


}
