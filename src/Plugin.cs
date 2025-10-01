global using static ArenaPlus.Utils.MyDevConsoleImplementation;
global using static ArenaPlus.Plugin;
global using Random = UnityEngine.Random;
global using static ArenaPlus.Utils.LoggingUtils;
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
using System.Security.Permissions;
using System.Runtime.CompilerServices;
using ArenaPlus.Features;

// Allows access to private members
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace ArenaPlus
{
    [BepInPlugin(MOD_ID, "ArenaPlus", "2.2.8")]
    class Plugin : BaseUnityPlugin
    {
        internal const string MOD_ID = "modforge.ArenaPlus";

        private void RainWorldGameOnShutDownProcess(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame self) { orig(self); ClearGameMemory(); }
        private void GameSessionOnctor(On.GameSession.orig_ctor orig, GameSession self, RainWorldGame game) { ClearGameMemory(); orig(self, game); }

        // Add hooks
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources, OnModInit);
            try { RegisterUtils.RegisterAllUtils(); } catch (Exception e) { LogError(e); }
        }

        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
            MachineConnector.SetRegisteredOI("modforge.ArenaPlus", OptionsInterface.instance);
            try { RegisterUtils.RegisterAllUtilsPostInit(); } catch (Exception e) { LogError(e); }
            Sounds.Initialize();

            Futile.atlasManager.LoadAtlas("atlases/huntersprites");
            Futile.atlasManager.LoadImage("atlases/huntersprites");

            var bundle = AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/arenaplus"));
            LogInfo(bundle.name, "loaded with:", bundle.GetAllAssetNames().FormatEnumarable());
            var shader = bundle.LoadAsset<Shader>("assets/arenaplus/verticalslice.shader");
            Custom.rainWorld.Shaders.Add("VerticalSlice", FShader.CreateShader("VerticalSlice", shader));
        }

        private void OnModInit(RainWorld rainWorld)
        {
            FeaturesManager.EnableFeatures();
        }

        [MyCommand("log_players")]
        private static void CheckPlayers()
        {
            ConsoleWrite("players: " + string.Join(", ", GameUtils.rainWorldGame.Players.ConvertAll(p => p.realizedCreature is Player player ? player.SlugCatClass.value : "null")));
        }

        private void ClearGameMemory()
        {
            //If you have any collections (lists, dictionaries, etc.)
            //Clear them here to prevent a memory leak
            //YourList.Clear();
            OnGameMemoryClear?.Invoke();
        }

        internal static event Action OnGameMemoryClear;

        public void FixedUpdate()
        {
            OnFixedUpdate?.Invoke();
        }

        internal static event Action OnFixedUpdate;
    }
}