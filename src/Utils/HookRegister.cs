using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static ArenaPlus.Utils.AssemblyUtils;

namespace ArenaPlus.Utils;
internal static class HookRegister
{
    public static void RegisterAllHooks()
    {
        Type[] types = GetLocalAssebly().GetTypesSafe();

        foreach (Type type in types)
        {
            MethodInfo[] methodes = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);


            foreach (MethodInfo methode in methodes)
            {
                if (methode.CustomAttributes.Count() > 0)
                {
                    HookRegisterAttribute attribute = methode.GetCustomAttribute<HookRegisterAttribute>();

                    if (methode.GetCustomAttribute<HookRegisterAttribute>() != null)
                    {
                        methode.Invoke(null, null);
                    }
                }
            }
        }
    }

}
[AttributeUsage(AttributeTargets.Method)]
public class HookRegisterAttribute : Attribute
{
}
