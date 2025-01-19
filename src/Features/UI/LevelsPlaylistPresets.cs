using ArenaPlus.Lib;
using Menu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArenaPlus.Features.UI
{
    [ImmutableFeature]
    file class LevelsPlaylistPresets : ImmutableFeature
    {
        private static string Separator { get; } = Path.DirectorySeparatorChar.ToString();
        public static string ConfigFolerPath { get; } = Application.persistentDataPath + Path.DirectorySeparatorChar.ToString() + "ModConfigs" + Separator + "ArenaPlus";
        public string PresetsFilePath { get; } = ConfigFolerPath + Separator + "arena_presets.txt";

        private int currentPreset = 0;
        public SymbolButton presetButton;
        public MenuLabel presetLabel;

        protected override void Register()
        {
            On.Menu.LevelSelector.LevelsList.ctor += LevelsList_ctor;
            On.Menu.LevelSelector.LevelsPlaylist.ctor += LevelsPlaylist_ctor;
            On.Menu.LevelSelector.LevelsPlaylist.Singal += LevelsPlaylist_Singal;
            On.Menu.LevelSelector.LevelsPlaylist.Update += LevelsPlaylist_Update;
            On.Menu.MultiplayerMenu.UpdateInfoText += MultiplayerMenu_UpdateInfoText;
        }

        private string MultiplayerMenu_UpdateInfoText(On.Menu.MultiplayerMenu.orig_UpdateInfoText orig, MultiplayerMenu self)
        {
            if (self.selectedObject is SymbolButton)
            {
                string text = (self.selectedObject as SymbolButton).signalText;

                if (text == "CHANGE_PRESET")
                {
                    switch (currentPreset)
                    {
                        case 0: return "Preset A levels selected";
                        case 1: return "Preset B levels selected";
                        case 2: return "Preset C levels selected";
                    }
                }
            }

            return orig(self);
        }

        private void LevelsPlaylist_Update(On.Menu.LevelSelector.LevelsPlaylist.orig_Update orig, LevelSelector.LevelsPlaylist self)
        {
            orig(self);

            switch (currentPreset)
            {
                case 0:
                    presetLabel.text = "Preset A";
                    break;
                case 1:
                    presetLabel.text = "Preset B";
                    break;
                case 2:
                    presetLabel.text = "Preset C";
                    break;
            }
        }

        private void LevelsPlaylist_Singal(On.Menu.LevelSelector.LevelsPlaylist.orig_Singal orig, LevelSelector.LevelsPlaylist self, MenuObject sender, string message)
        {
            if (message != null)
            {
                if (message == "CHANGE_PRESET")
                {
                    UpdatePresetsFile(currentPreset, string.Join(",", self.PlayList));
                    currentPreset = (currentPreset + 1) % 3;

                    (self.owner as LevelSelector).GetMultiplayerMenu.GetGameTypeSetup.playList = (from level in ReadPresetsFile()[currentPreset].Split(',') where level != "" select level).ToList();

                    UpdatePresetsFile(3, currentPreset.ToString());

                    self.ResolveMismatch();

                    presetButton.UpdateSymbol(GetPresetIcon());

                    self.menu.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
                    return;
                }
            }

            orig(self, sender, message);
        }

        private void LevelsPlaylist_ctor(On.Menu.LevelSelector.LevelsPlaylist.orig_ctor orig, LevelSelector.LevelsPlaylist self, Menu.Menu menu, LevelSelector owner, UnityEngine.Vector2 pos)
        {
            orig(self, menu, owner, pos);

            currentPreset = int.Parse(ReadPresetsFile()[3]);

            // Right hand line
            FSprite[] rightHandLines = new FSprite[self.rightHandLines.Length + 1];

            Array.Copy(self.rightHandLines, rightHandLines, self.rightHandLines.Length);

            FSprite rightHandLine = new FSprite("pixel", true);
            rightHandLine.anchorX = 0f;
            rightHandLine.anchorY = 0f;
            rightHandLine.scaleX = 2f;
            self.Container.AddChild(rightHandLine);

            rightHandLines[rightHandLines.Length - 1] = rightHandLine;

            self.rightHandLines = rightHandLines;

            // Button
            presetButton = new SymbolButton(menu, self, GetPresetIcon(), "CHANGE_PRESET", self.sideButtons[self.sideButtons.Length - 2].pos + new Vector2(0f, 30f));
            presetButton.symbolSprite.scale = 0.7f;

            self.sideButtons[self.sideButtons.Length - 1] = presetButton;

            self.subObjects.Add(presetButton);

            MenuLabel[] labels = new MenuLabel[self.labels.Length + 1];

            Array.Copy(self.labels, labels, self.labels.Length);

            // Label
            presetLabel = new MenuLabel(menu, self, "", presetButton.pos + new Vector2(10f, -3f), new Vector2(50f, 30f), false, null);
            presetLabel.text = menu.Translate("Clear playlist");
            presetLabel.label.alignment = FLabelAlignment.Left;

            labels[labels.Length - 1] = presetLabel;

            self.labels = labels;

            self.subObjects.Add(presetLabel);

            float[,] labelsFade = new float[self.labelsFade.Length + 1, 2];

            Array.Copy(self.labelsFade, labelsFade, self.labelsFade.Length);

            self.labelsFade = labelsFade;
        }

        private void LevelsList_ctor(On.Menu.LevelSelector.LevelsList.orig_ctor orig, LevelSelector.LevelsList self, Menu.Menu menu, MenuObject owner, UnityEngine.Vector2 pos, int extraSideButtons, bool shortList)
        {
            orig(self, menu, owner, pos, extraSideButtons, shortList);

            SymbolButton[] sideButtons = new SymbolButton[self.sideButtons.Length + 1];

            Array.Copy(self.sideButtons, sideButtons, self.sideButtons.Length);

            self.sideButtons = sideButtons;

            Directory.CreateDirectory(ConfigFolerPath);
        }

        string GetPresetIcon()
        {
            return currentPreset switch
            {
                0 => "Sandbox_A",
                1 => "Sandbox_B",
                2 => "Sandbox_C",
                _ => throw new Exception("Invalid preset"),
            };
        }

        string[] ReadPresetsFile()
        {
            if (!File.Exists(PresetsFilePath))
            {
                string[] lines = new string[4];
                lines[3] = currentPreset.ToString();
                File.WriteAllLines(PresetsFilePath, lines);
                return lines;
            }

            return File.ReadAllLines(PresetsFilePath);
        }

        void UpdatePresetsFile(int line, string content)
        {
            string[] lines = ReadPresetsFile();

            lines[line] = content;

            File.WriteAllLines(PresetsFilePath, lines);
        }
    }
}
