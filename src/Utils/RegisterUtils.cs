using ArenaPlus.Lib;
using BepInEx.Bootstrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaPlus.Utils
{
    internal static class RegisterUtils
    {
        internal static void RegisterAllUtils()
        {
            HookRegister.RegisterAllHooks();
        }

        internal static void RegisterAllUtilsPostInit()
        {
            ConsoleWrite("test my dev console");
            try { Utils.MyDevConsoleImplementation.Register(); } catch (System.IO.FileLoadException) { } catch (Exception e) { LogWarning(e); }
            FeaturesManager.LoadFeatures();
        }
    }
}
