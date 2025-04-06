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
            // Uncomment when DevConsole is fixed
            //try { Utils.MyDevConsole.Register(); } catch { }
            LogWarning("ArenaPlus commands are temporarily removed until Dev Console is fixed");
            HookRegister.RegisterAllHooks();
        }
    }
}
