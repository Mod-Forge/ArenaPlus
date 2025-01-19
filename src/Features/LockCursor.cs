using ArenaPlus.Lib;
using ArenaPlus.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArenaPlus.Features
{
    [FeatureInfo(
        id: "lockCursor",
        name: "Lock cursor",
        description: "Whether the cursor is locked to the game window",
        enabledByDefault: false
    )]
    internal class LockCursor(FeatureInfoAttribute featureInfo) : Feature(featureInfo)
    {
        protected override void Register()
        {
            OnFixedUpdate += Plugin_OnFixedUpdate;
        }

        protected override void Unregister()
        {
            OnFixedUpdate -= Plugin_OnFixedUpdate;
        }

        private void Plugin_OnFixedUpdate()
        {
            if (GameUtils.RainWorldInstance?.processManager.currentMainLoop is RainWorldGame game && !game.GamePaused)
            {
                Cursor.lockState = CursorLockMode.Confined;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }
}
