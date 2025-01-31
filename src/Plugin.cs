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
using System.Linq;

namespace ArenaPlus
{
    [BepInPlugin(MOD_ID, "ArenaPlus", "2.0.1")]
    class Plugin : BaseUnityPlugin
    {
        private const string MOD_ID = "modforge.ArenaPlus";

        public static ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("ArenaPlus");
        public static void LogUnity(params object[] data) => UnityEngine.Debug.Log($"[{MOD_ID}] " + string.Join(" ", data));
        public static void LogInfo(params object[] data) => log.LogInfo(string.Join(" ", data));
        public static void LogDebug(params object[] data)
        {
            if (Environment.GetCommandLineArgs().Contains("--debug"))
            {
                log.LogDebug(string.Join(" ", data));
            }
        }
        public static void LogMessage(params object[] data) => log.LogMessage(string.Join(" ", data));
        public static void LogError(params object[] data) => log.LogError(string.Join(" ", data));
        public static void LogFatal(params object[] data) => log.LogFatal(string.Join(" ", data));
        public static void Assert(bool check, string message) { if (!check) throw new Exception(message); }


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