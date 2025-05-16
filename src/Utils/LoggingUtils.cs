using BepInEx.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ArenaPlus.Utils;

internal static class LoggingUtils
{
    public static ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("ArenaPlus");
    private static bool _debugChecked;
    private static bool _debugEnabled;
    public static bool DebugEnabled {
        get
        {
            if (!_debugChecked)
            {
                string[] args = Environment.GetCommandLineArgs();
                if (args.Contains("--debug"))
                {
                    //int index = Array.IndexOf(args, "--debug");
                    //if (args.Length <= index || args[index + 1] == "All" || Environment.GetCommandLineArgs()[index + 1] == "ArenaPlus")
                    //{
                    //    _debugEnabled = true;
                    //}
                    _debugEnabled = true;
                }
                _debugChecked = true;
            }
            return _debugEnabled;
        }
    }
    public static void LogUnity(params object[] data) => UnityEngine.Debug.Log($"[{MOD_ID}] " + string.Join(" ", data));
    public static void LogInfo(params object[] data) => log.LogInfo(string.Join(" ", data));
    public static void LogDebug(params object[] data)
    {
        if (DebugEnabled)
        {
            log.LogDebug(string.Join(" ", data));
        }
    }
    public static void LogMessage(params object[] data) => log.LogMessage(string.Join(" ", data));
    public static void LogError(params object[] data) => log.LogError(string.Join(" ", data));
    public static void LogFatal(params object[] data) => log.LogFatal(string.Join(" ", data));
    public static void LogWarning(params object[] data) => log.LogWarning(string.Join(" ", data));
    public static void Assert(bool check, string message) { if (!check) throw new AssertFailedException(message); }
    public static string FormatEnumarableRecursive(this IEnumerable values, bool addEnumerableType = false)
    {
        return FormatEnumarable(values, addEnumerableType, true);
    }

    public static string FormatEnumarable(this IEnumerable values, bool addEnumerableType = false, bool recursive = false)
    {
        return $"{(addEnumerableType ? values.ToString() + "" : "")}{{ {string.Join(", ", values.Cast<object>().Select(v => FormatObject(v, addEnumerableType, recursive)))} }}";
    }
    public static string FormatObject(object obj, bool addEnumerableType = false, bool recursive = false)
    {
        if (obj == null) return "null";
        if (obj is string) return $"\"{obj}\"";
        if (obj is char) return $"\'{obj}\'";

        Type type = obj.GetType();
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
        {
            PropertyInfo keyProperty = type.GetProperty("Key");
            PropertyInfo valueProperty = type.GetProperty("Value");

            object key = keyProperty?.GetValue(obj);
            object val = valueProperty?.GetValue(obj);

            return $"{FormatObject(key)} : {FormatObject(val ?? "null")}";
        }

        if (recursive && obj is IEnumerable enu)
        {
            return FormatEnumarable(enu, addEnumerableType, true);
        }

        return obj.ToString();
    }
}

