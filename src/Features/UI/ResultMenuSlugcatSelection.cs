using ArenaPlus.Lib;
using ArenaPlus.Utils;
using Menu;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArenaPlus.Features.UI
{
    [FeatureInfo(
        id: "resultMenuSlugcatSelector",
        name: "Slugcat selector in result menu",
        description: "Add a slugcat selector in the result menu with two other choices",
        enabledByDefault: false
    )]
    file class ResultMenuSlugcatSelection(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
    {
        private static readonly int _selectionCount = 3;
        public static int selectionCount => SlugcatsUtils.GetActiveSlugcats().Count < _selectionCount ? SlugcatsUtils.GetActiveSlugcats().Count : _selectionCount;

        public static Player.InputPackage[] lastInput = new Player.InputPackage[4];
        //public static int[] randomSeeds = new int[4];
        public static SlugcatStats.Name[][] nameList = new SlugcatStats.Name[4][];
        public static SlugcatStats.Name[] lastCharactersNames = new SlugcatStats.Name[4];
        public static int[] cooldowns = new int[4];

        protected override void Register()
        {
            On.Menu.SymbolButton.Update += SymbolButton_Update;
            On.Menu.PlayerResultBox.Update += PlayerResultBox_Update;
            On.Menu.PlayerResultBox.ctor += PlayerResultBox_ctor;
        }

        protected override void Unregister()
        {
            On.Menu.SymbolButton.Update -= SymbolButton_Update;
            On.Menu.PlayerResultBox.Update -= PlayerResultBox_Update;
            On.Menu.PlayerResultBox.ctor -= PlayerResultBox_ctor;
        }

        private SlugcatStats.Name[] GenerateSlugcatList(SlugcatStats.Name lastChar)
        {
            List<SlugcatStats.Name> allSlugcats = SlugcatsUtils.GetActiveSlugcats();
            SlugcatStats.Name[] slugcats = new SlugcatStats.Name[selectionCount];
            if (allSlugcats.Count < selectionCount)
            {
                LogError($"Not enough slugcat activated requires minimum {selectionCount} got {allSlugcats.Count}");
                return slugcats.Select(e => SlugcatStats.Name.Night).ToArray();
            }

            slugcats[0] = lastChar;
            for (int i = 1; i < selectionCount; i++)
            {
                int emergencyCountdown = 100;
                SlugcatStats.Name newName = lastChar;
                while (slugcats.Contains(newName))
                {
                    newName = allSlugcats[Random.Range(0, allSlugcats.Count)];

                    if (emergencyCountdown-- <= 0)
                    {
                        throw new Exception("no new slugcat found");
                    }
                }
                slugcats[i] = newName;
            }

            return slugcats;
        }

        private void PlayerResultBox_ctor(On.Menu.PlayerResultBox.orig_ctor orig, PlayerResultBox self, Menu.Menu menu, MenuObject owner, Vector2 pos, Vector2 size, ArenaSitting.ArenaPlayer player, int index)
        {
            orig(self, menu, owner, pos, size, player, index);
            try
            {
                PlayerResultBoxCustomData data = self.GetCustomData<PlayerResultBoxCustomData>();
                data.scrollUpButton = new VisualScrollButton(menu, self, "UP", new Vector2(0.01f - 30f, (0.01f + self.size.y) - 40), 0);
                data.scrollUpButton.inactive = false;
                self.subObjects.Add(data.scrollUpButton);
                data.scrollDownButton = new VisualScrollButton(menu, self, "DOWN", new Vector2(0.01f - 30f, -25.99f + 40), 2);
                data.scrollUpButton.inactive = false;
                self.subObjects.Add(data.scrollDownButton);

                lastCharactersNames[player.playerNumber] = player.playerClass;
                nameList[player.playerNumber] = GenerateSlugcatList(lastCharactersNames[player.playerNumber]);
                cooldowns[player.playerNumber] = 20;
                //randomSeeds[player.playerNumber] = (int)(Random.value * 100);
            }
            catch (Exception e) { logSource.LogError(e); }
        }

        private void PlayerResultBox_Update(On.Menu.PlayerResultBox.orig_Update orig, PlayerResultBox self)
        {
            orig(self);
            PlayerResultBoxCustomData data = self.GetCustomData<PlayerResultBoxCustomData>();
            if (GameUtils.RainWorldInstance.processManager.arenaSetup.playerClass[self.player.playerNumber] != null && self.DeadPortraint && self is not FinalResultbox)
            {
                data.scrollUpButton.greyedOut = self.player.readyForNextRound;
                data.scrollDownButton.greyedOut = self.player.readyForNextRound;

                cooldowns[self.player.playerNumber]--;

                if (!self.player.readyForNextRound && cooldowns[self.player.playerNumber] <= 0)
                {
                    Player.InputPackage inputPackage = RWInput.PlayerInput(self.player.playerNumber);
                    //logSource.LogInfo($"input y: {inputPackage.y}");
                    if (inputPackage.y != 0 && lastInput[self.player.playerNumber].y == 0)
                    {


                        int index = 0;
                        if (nameList[self.player.playerNumber].Contains(self.player.playerClass))
                        {
                            index = nameList[self.player.playerNumber].IndexOf(self.player.playerClass);
                            logSource.LogInfo($"found index of {self.player.playerClass}: " + index);
                        }
                        index += inputPackage.y;
                        if (index >= nameList[self.player.playerNumber].Count())
                            index = 0;
                        if (index < 0)
                            index = nameList[self.player.playerNumber].Count() - 1;
                        logSource.LogInfo("index: " + index);


                        SlugcatStats.Name newName = nameList[self.player.playerNumber][index];

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

        private void SymbolButton_Update(On.Menu.SymbolButton.orig_Update orig, global::Menu.SymbolButton self)
        {
            if (self is VisualScrollButton button)
            {
                if (self.Selected) self.menu.selectedObject = null;
                for (int i = 0; i < self.subObjects.Count; i++)
                {
                    self.subObjects[i].Update();
                }
                self.lastSize = self.size;
                if (button.greyedOut || button.bump)
                {
                    button.UpdateButtonBehav();
                }
                self.roundedRect.fillAlpha = Mathf.Lerp(0.3f, 0.6f, self.buttonBehav.col);
                self.roundedRect.addSize = new Vector2(4f, 4f) * (self.buttonBehav.sizeBump + 0.5f * Mathf.Sin(self.buttonBehav.extraSizeBump * 3.1415927f)) * (self.buttonBehav.clicked ? 0f : 1f);
            }
            else
            {
                orig(self);
            }
        }
    }

    internal class VisualScrollButton : SymbolButton
    {
        // Token: 0x0600437D RID: 17277 RVA: 0x0049F0E5 File Offset: 0x0049D2E5
        public VisualScrollButton(Menu.Menu menu, MenuObject owner, string singalText, Vector2 pos, int direction) : base(menu, owner, "Menu_Symbol_Arrow", singalText, pos)
        {
            this.direction = direction;
        }

        // Token: 0x0600437E RID: 17278 RVA: 0x0049F100 File Offset: 0x0049D300
        public override void Update()
        {
            base.Update();
            if (this.buttonBehav.clicked && !this.buttonBehav.greyedOut)
            {
                this.heldCounter++;
                if (this.heldCounter > 20 && this.heldCounter % 4 == 0)
                {
                    //this.menu.PlaySound(SoundID.MENU_Scroll_Tick);
                    //this.Singal(this, message: this.signalText);
                    this.buttonBehav.sin = 0.5f;
                    return;
                }
            }
            else
            {
                this.heldCounter = 0;
            }
            this.roundedRect.fillAlpha = Mathf.Lerp(0.3f, 0.6f, this.buttonBehav.col);
            this.roundedRect.addSize = new Vector2(4f, 4f) * (this.buttonBehav.sizeBump + 0.5f * Mathf.Sin(this.buttonBehav.extraSizeBump * 3.1415927f)) * (this.buttonBehav.clicked ? 0f : 1f);
        }

        // Token: 0x0600437F RID: 17279 RVA: 0x0049F184 File Offset: 0x0049D384
        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            this.symbolSprite.rotation = 90f * (float)this.direction;
        }

        public void Bump()
        {
            bump = true;
            this.menu.selectedObject = this;
            //this.Clicked();
            //this.heldCounter = 24;
        }

        // Token: 0x06004380 RID: 17280 RVA: 0x0049F1A5 File Offset: 0x0049D3A5
        public override void Clicked()
        {
            if (this.heldCounter < 20)
            {
                this.menu.PlaySound(SoundID.MENU_First_Scroll_Tick);
                this.Singal(this, this.signalText);
            }
        }

        public void UpdateButtonBehav()
        {
            this.buttonBehav.greyedOut = this.greyedOut;
            this.buttonBehav.lastFlash = this.buttonBehav.flash;
            this.buttonBehav.lastSin = this.buttonBehav.sin;
            this.buttonBehav.flash = Custom.LerpAndTick(this.buttonBehav.flash, 0f, 0.03f, 0.16666667f);
            if (this.buttonBehav.owner.Selected && (!this.buttonBehav.greyedOut || !this.buttonBehav.owner.menu.manager.menuesMouseMode))
            {
                if (!this.buttonBehav.bump)
                {
                    this.buttonBehav.bump = true;
                }
                this.buttonBehav.sizeBump = Custom.LerpAndTick(this.buttonBehav.sizeBump, 1f, 0.1f, 0.1f);
                this.buttonBehav.sin += 1f;
                if (!this.buttonBehav.flashBool)
                {
                    this.buttonBehav.flashBool = true;
                    this.buttonBehav.flash = 1f;
                }
                if (!this.buttonBehav.greyedOut)
                {
                    if (this.buttonBehav.owner.menu.pressButton)
                    {
                        if (!this.buttonBehav.clicked)
                        {
                            this.buttonBehav.owner.menu.PlaySound(SoundID.MENU_Button_Press_Init);
                        }
                        this.buttonBehav.clicked = true;
                    }
                    if (!this.buttonBehav.owner.menu.holdButton)
                    {
                        if (this.buttonBehav.clicked)
                        {
                            (this.buttonBehav.owner as ButtonMenuObject).Clicked();
                        }
                        this.buttonBehav.clicked = false;
                    }
                    this.buttonBehav.col = Mathf.Min(1f, this.buttonBehav.col + 0.1f);
                }
            }
            else
            {
                this.buttonBehav.clicked = false;
                this.buttonBehav.bump = false;
                this.buttonBehav.flashBool = false;
                this.buttonBehav.sizeBump = Custom.LerpAndTick(this.buttonBehav.sizeBump, 0f, 0.1f, 0.05f);
                this.buttonBehav.col = Mathf.Max(0f, this.buttonBehav.col - 0.033333335f);
            }
            if (this.buttonBehav.owner.toggled)
            {
                this.buttonBehav.sizeBump = Custom.LerpAndTick(this.buttonBehav.sizeBump, 1f, 0.1f, 0.1f);
                this.buttonBehav.sin = 7.5f;
                this.buttonBehav.bump = true;
                if (this.buttonBehav.flash < 0.75f)
                {
                    this.buttonBehav.flash = 0.75f;
                }
            }
            this.buttonBehav.lastExtraSizeBump = this.buttonBehav.extraSizeBump;
            if (this.buttonBehav.bump)
            {
                this.buttonBehav.extraSizeBump = Mathf.Min(1f, this.buttonBehav.extraSizeBump + 0.1f);
                return;
            }
            this.buttonBehav.extraSizeBump = 0f;
        }

        // Token: 0x04004651 RID: 18001
        public int direction;

        // Token: 0x04004652 RID: 18002
        private int heldCounter;

        public bool greyedOut;

        public bool bump;
    }
}