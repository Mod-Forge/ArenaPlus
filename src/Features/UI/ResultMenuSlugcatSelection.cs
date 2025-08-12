using ArenaPlus.Lib;
using ArenaPlus.Options;
using ArenaPlus.Options.Tabs;
using ArenaPlus.Utils;
using Menu;
using Menu.Remix.MixedUI;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;

namespace ArenaPlus.Features.UI
{
    [FeatureInfo(
        id: "resultMenuSlugcatSelector",
        name: "Slugcat selector",
        description: "Add a slugcat selector in the result menu with two other choices",
        enabledByDefault: false
    )]
    file class ResultMenuSlugcatSelection : Feature
    {
        public static int selectionCount => SlugcatsUtils.GetActiveSlugcats().Count < selectionCountConfig.Value ? SlugcatsUtils.GetActiveSlugcats().Count : selectionCountConfig.Value;

        public static Player.InputPackage[] lastInput = new Player.InputPackage[4];
        //public static int[] randomSeeds = new int[4];
        public static SlugcatStats.Name[][] nameList = new SlugcatStats.Name[4][];
        public static SlugcatStats.Name[] lastCharactersNames = new SlugcatStats.Name[4];
        public static int[] cooldowns = new int[4];
        public static Stack<ResultMenuIlustrationAnimation>[] menuIlustrationQue = new Stack<ResultMenuIlustrationAnimation>[4];


        public static readonly Configurable<int> selectionCountConfig = OptionsInterface.instance.config.Bind("selectionCount", 3, new ConfigurableInfo("The number of slugcat choices in the selector, also counts the current slugcat", new ConfigAcceptableRange<int>(2, 10), "", []));

        public class ResultMenuIlustrationAnimation : MenuIllustration
        {
            public FSprite sprite2;
            public string fileName2;
            public Texture2D texture2;
            public bool reverseAnimation;

            public float animation = 0f;

            public FLabel nameLabel;

            public PlayerResultBox playerResultBox => owner as PlayerResultBox;
            public ResultMenuIlustrationAnimation(Menu.Menu menu, PlayerResultBox owner, string newFileName, string lastFileName, Vector2 pos, bool reverseAnimation, bool crispPixels, bool anchorCenter) : base(menu, owner, "", newFileName, pos, crispPixels, anchorCenter)
            {
                fileName2 = lastFileName;
                this.reverseAnimation = reverseAnimation;
                LoadFile2();

                sprite2 = new FSprite(fileName2);
                if (!anchorCenter)
                {
                    sprite2.anchorX = 0f;
                    sprite2.anchorY = 0f;
                }
                sprite2.alpha = 0f;
                Container.AddChild(sprite2);

                nameLabel = new FLabel(Custom.GetFont(), "scugcat");
                nameLabel.isVisible = false;
                Container.AddChild(nameLabel);
            }

            public void LoadFile2()
            {
                if (Futile.atlasManager.GetAtlasWithName(fileName2) != null)
                {
                    FAtlas atlasWithName = Futile.atlasManager.GetAtlasWithName(fileName2);
                    texture = (Texture2D)atlasWithName.texture;
                    return;
                }

                texture2 = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
                string text = AssetManager.ResolveFilePath("Illustrations" + Path.DirectorySeparatorChar + fileName2 + ".png");
                string text2 = "file:///";
                try
                {
                    AssetManager.SafeWWWLoadTexture(ref texture2, text2 + text, clampWrapMode: true, crispPixels);
                }
                catch (FileLoadException arg)
                {
                    Custom.LogWarning($"Error loading file: {arg}");
                }

                HeavyTexturesCache.LoadAndCacheAtlasFromTexture(fileName2, texture2, textureFromAsset: false);
            }

            public override void Update()
            {
                base.Update();
                if (animation >= 1f && menuIlustrationQue[playerResultBox.player.playerNumber].Count > 0)
                {
                    var ilustration = menuIlustrationQue[playerResultBox.player.playerNumber].Pop();
                    playerResultBox.portrait.RemoveSprites();
                    playerResultBox.subObjects.Remove(playerResultBox.portrait);
                    playerResultBox.portrait = ilustration;
                    playerResultBox.subObjects.Add(playerResultBox.portrait);
                }
            }

            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);

                animation = Mathf.Clamp01(animation + timeStacker * 0.25f);

                nameLabel.isVisible = sprite.width < 2;
                nameLabel.text = fileName.Split('-')[1];
                nameLabel.SetPosition(sprite.GetPosition());

                if (playerResultBox.player.readyForNextRound)
                {
                    sprite.shader = GameUtils.RainWorldInstance.Shaders["Basic"];
                    sprite2.isVisible = false;
                    return;
                }

                sprite2.isVisible = true;
                sprite2.shader = GameUtils.RainWorldInstance.Shaders["VerticalSlice"];
                sprite2.color = sprite.color;

                Vector2 pos = new Vector2(sprite.x, sprite.y);

                float height = Mathf.Max(sprite.height, 84);
                if (reverseAnimation)
                {

                    sprite.SetPosition(pos - new Vector2(0, height * (1f - animation)));
                    sprite.alpha = ((1f - animation) * 100f) / 255f;


                    sprite2.SetPosition(pos + new Vector2(0, height * animation));


                    sprite2.alpha = (((1f - animation) * 100f) + 100f) / 255f;

                    sprite.shader = GameUtils.RainWorldInstance.Shaders["VerticalSlice"];
                }
                else
                {
                    sprite.SetPosition(pos + new Vector2(0, height * (1f - animation)));
                    sprite2.SetPosition(pos - new Vector2(0, height * animation));


                    sprite2.alpha = (animation * 100f) / 255f;

                    sprite.shader = GameUtils.RainWorldInstance.Shaders["VerticalSlice"];
                    sprite.alpha = ((animation * 100f) + 100f) / 255f;
                }
            }

            public override void RemoveSprites()
            {
                base.RemoveSprites();
                sprite2.RemoveFromContainer();
                Container.RemoveChild(nameLabel);
                texture2 = null;
            }

            public void UnloadFile2()
            {
                UnityEngine.Object.Destroy(texture2);
            }
        }


        public ResultMenuSlugcatSelection(FeatureInfoAttribute featureInfo) : base(featureInfo)
        {
            SetComplementaryElement((expandable, startPos) =>
            {
                OpUpdown updown = expandable.AddItem(
                    new OpUpdown(selectionCountConfig, startPos, 60f)
                );
                updown.pos -= new Vector2(0, (updown.size.y - FeaturesTab.CHECKBOX_SIZE) / 2);
                updown.description = selectionCountConfig.info.description;

                if (HexColor != "None" && ColorUtility.TryParseHtmlString("#" + HexColor, out Color color))
                {
                    updown.colorEdge = color;
                }
            });
        }

        protected override void Unregister()
        {
            On.Menu.SymbolButton.Update -= SymbolButton_Update;
            On.Menu.PlayerResultBox.Update -= PlayerResultBox_Update;
            On.Menu.PlayerResultBox.ctor -= PlayerResultBox_ctor;
            On.Menu.MenuIllustration.UnloadFile -= MenuIllustration_UnloadFile;
        }

        protected override void Register()
        {
            On.Menu.SymbolButton.Update += SymbolButton_Update;
            On.Menu.PlayerResultBox.Update += PlayerResultBox_Update;
            On.Menu.PlayerResultBox.ctor += PlayerResultBox_ctor;
            On.Menu.MenuIllustration.UnloadFile += MenuIllustration_UnloadFile;
        }

        private void MenuIllustration_UnloadFile(On.Menu.MenuIllustration.orig_UnloadFile orig, MenuIllustration self)
        {
            orig(self);
            if (self is ResultMenuIlustrationAnimation result)
            {
                result.UnloadFile2();
            }
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
                menuIlustrationQue[player.playerNumber] = new();
                cooldowns[player.playerNumber] = 40;

                var newIlustration = string.Concat("MultiplayerPortrait", self.player.playerNumber.ToString(), "0", "-", player.playerClass.value);
                var ilustration = new ResultMenuIlustrationAnimation(menu, self, newIlustration, newIlustration, new Vector2(self.originalSize.y / 2f, self.originalSize.y / 2f), false, true, true)
                {
                    animation = 1f,
                };
                self.portrait.RemoveSprites();
                self.subObjects.Remove(self.portrait);
                self.portrait = ilustration;
                self.subObjects.Add(self.portrait);

                //randomSeeds[player.playerNumber] = (int)(Random.value * 100);
            }
            catch (Exception e) { LogError(e); }
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
                    //LogDebug($"input y: {inputPackage.y}");
                    if (inputPackage.y != 0 && lastInput[self.player.playerNumber].y == 0)
                    {


                        int index = 0;
                        if (nameList[self.player.playerNumber].Contains(self.player.playerClass))
                        {
                            index = nameList[self.player.playerNumber].IndexOf(self.player.playerClass);
                            LogDebug($"found index of {self.player.playerClass}: " + index);
                        }

                        index += inputPackage.y;
                        if (index >= nameList[self.player.playerNumber].Count())
                            index = 0;
                        if (index < 0)
                            index = nameList[self.player.playerNumber].Count() - 1;
                        LogDebug("index: " + index);


                        SlugcatStats.Name newName = nameList[self.player.playerNumber][index];
                        SlugcatStats.Name lastName = self.player.playerClass;
                        self.player.playerClass = newName;
                        //self.portrait.sprite.SetElementByName(string.Concat(new string[]
                        //{
                        //    "MultiplayerPortrait",
                        //    self.player.playerNumber.ToString(),
                        //    "1",
                        //    "-",
                        //    newName.value
                        //}));

                        if (menuIlustrationQue[self.player.playerNumber].Count > 0)
                        {
                            if (menuIlustrationQue[self.player.playerNumber].Peek().reverseAnimation != (inputPackage.y < 0))
                            {
                                menuIlustrationQue[self.player.playerNumber].Clear();
                            }
                        }

                        Menu.Menu menu = self.portrait.menu;
                        var newIlustration = string.Concat("MultiplayerPortrait", self.player.playerNumber.ToString(), "1", "-", newName.value);
                        var lastIlustration = string.Concat("MultiplayerPortrait", self.player.playerNumber.ToString(), "1", "-", lastName.value);
                        var ilustration = new ResultMenuIlustrationAnimation(menu, self, newIlustration, lastIlustration, new Vector2(self.originalSize.y / 2f, self.originalSize.y / 2f), inputPackage.y < 0, true, true);

                        menuIlustrationQue[self.player.playerNumber].Push(ilustration);
                        //if (self.portrait is ResultMenuIlustrationAnimation)
                        //{
                        //}
                        //else
                        //{
                        //    self.portrait.RemoveSprites();
                        //    self.subObjects.Remove(self.portrait);
                        //    self.portrait = ilustration;
                        //    self.subObjects.Add(self.portrait);
                        //}
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