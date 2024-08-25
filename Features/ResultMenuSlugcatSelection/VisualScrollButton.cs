using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Menu;
using RWCustom;

namespace ArenaSlugcatsConfigurator.Features.ResultMenuSlugcatSelection
{
    internal static class Hooks
    {
        internal static void Register()
        {
            On.Menu.SymbolButton.Update += SymbolButton_Update;
        }

        private static void SymbolButton_Update(On.Menu.SymbolButton.orig_Update orig, SymbolButton self)
        {
;

            VisualScrollButton button = self as VisualScrollButton;
            if (button != null)
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
    public class VisualScrollButton : SymbolButton
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
