using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ArenaPlus.Utils
{
    public static class HookRegister
    {
        public static void RegisterAllHooks()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types;
                }

                foreach (Type type in types.Where(t => t != null))
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
    }
}
