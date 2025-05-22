using ArenaPlus.Lib;
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
            try { Utils.MyDevConsole.Register(); } catch { }
            FeaturesManager.LoadFeatures();
        }
    }
}
