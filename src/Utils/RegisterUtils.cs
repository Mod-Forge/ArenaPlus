﻿using System;
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
            try { Utils.MyDevConsole.Register(); } catch { }
            HookRegister.RegisterAllHooks();
        }
    }
}
