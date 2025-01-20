global using static ArenaPlus.Utils.MyDevConsole;
global using static ArenaPlus.Plugin;
global using Random = UnityEngine.Random;
using BepInEx;
using BepInEx.Logging;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using MoreSlugcats;
using RWCustom;
using ArenaPlus.Utils;
using ArenaPlus.Options;
using ArenaPlus.Lib;
using System.Reflection;
using UnityEngine.Assertions;

namespace ArenaPlus
{
    [BepInPlugin(MOD_ID, "ArenaPlus", "0.1.0")]
    class Plugin : BaseUnityPlugin
    {
        private const string MOD_ID = "modforge.ArenaPlus";

        public static ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("ArenaPlus");
        public static void Log(object msg) => log.LogInfo(msg);
        public static void LogError(object msg) => log.LogError(msg);

        public static void Assert(bool check, string message) { if (!check) LogError(message); }


        // Add hooks
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            try { RegisterUtils.RegisterAllUtils(); } catch (Exception e) { Plugin.LogError(e); }
        }

        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
            Futile.atlasManager.LoadAtlas("atlases/huntersprites");
            MachineConnector.SetRegisteredOI("modforge.ArenaPlus", OptionsInterface.instance);
            FeaturesManager.LoadFeatures();
        }

        public void FixedUpdate()
        {
            OnFixedUpdate?.Invoke();
        }

        internal static event Action OnFixedUpdate;
    }
}