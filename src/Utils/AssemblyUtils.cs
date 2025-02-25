using System;
using System.Linq;
using System.Reflection;

namespace ArenaPlus.Utils;
internal static class AssemblyUtils
{
    internal static Assembly GetLocalAssebly() => typeof(Plugin).Assembly;
    internal static Type[] GetTypesSafe(this Assembly assembly)
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
        return types.Where(t => t != null).ToArray();
    }
}
