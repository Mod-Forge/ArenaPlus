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
    [ImmutableFeature]
    internal class KeyboardPause : ImmutableFeature
    {
        protected override void Register()
        {
            On.RWInput.CheckPauseButton_int_bool += RWInput_CheckPauseButton_int_bool;
        }

        private bool RWInput_CheckPauseButton_int_bool(On.RWInput.orig_CheckPauseButton_int_bool orig, int playerNumber, bool inMenu)
        {
            return orig(playerNumber, inMenu) || Input.GetKey(KeyCode.Escape);
        }
    }
}