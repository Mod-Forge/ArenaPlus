using ArenaPlus.Utils;
using BepInEx.Logging;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ZeldackLib.MyDevConsole;

namespace ArenaPlus.Utils;

internal static class MyDevConsoleImplementation
{
    internal static void Register()
    {
        MyDevConsoleCommandImplementation.Register();
    }

    [MyCommand("console_write")]
    public static void ConsoleWrite(object message = null, Color? color = null)
    {
        try
        {
            GameConsoleWriteLine(message != null ? message : "", color.HasValue ? color.Value : Color.white);
        }
        catch (System.IO.FileLoadException) { }
        catch (System.IO.FileNotFoundException) { }
        catch (Exception ex) { LogError(ex); }

        if (color == Color.red)
        {
            LogError(message);
        }
        else if (color == Color.yellow)
        {
            LogWarning(message);
        }
        else
        {
            LogMessage(message);
        }
    }

    private static void GameConsoleWriteLine(object message, Color color)
    {
        DevConsole.GameConsole.WriteLine(message.ToString(), color);
    }
}

file static class MyDevConsoleCommandImplementation
{
    private static MyModCommand myModCommand;

    internal static void Register()
    {
        myModCommand = new MyModCommand("ArenaPlus");
        CreateCommands();
    }

    private static void CreateCommands()
    {
        Type[] types = AssemblyUtils.GetLocalAssebly().GetTypesSafe();

        foreach (Type type in types)
        {
            try
            {
                MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

                foreach (MethodInfo method in methods)
                {
                    if (method.CustomAttributes.Count() > 0 && method.GetCustomAttribute<MyCommandAttribute>() is MyCommandAttribute attribute)
                    {
                        var command = myModCommand.CreateMethodCommand(attribute.name, method);
                        if (command.HasValue)
                        {
                            foreach (var parameter in method.GetParameters())
                            {
                                if (parameter.GetCustomAttribute<ValuesAttribute>() is ValuesAttribute paramValues)
                                {
                                    if (paramValues.values != null)
                                    {
                                        command.Value.SetParameterValues(parameter.Name, paramValues.values);
                                    }
                                    else if (paramValues.AutocompleteMethod != null)
                                    {
                                        command.Value.SetParameterValuesMethod(parameter.Name, paramValues.AutocompleteMethod);
                                    }
                                    else
                                    {
                                        LogError("Failed to add parameter value for", command.Value.name, parameter.Name);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e) { myModCommand.logSource.LogError(e); }

            try
            {
                foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    if (prop.GetSetMethod(true) is MethodInfo setMethod && prop.GetCustomAttribute<MyCommandAttribute>() is MyCommandAttribute attribute)
                    {
                        var command = myModCommand.CreateMethodCommand(attribute.name, setMethod);
                        if (command.HasValue && prop.GetCustomAttribute<ValuesAttribute>() is ValuesAttribute paramValues)
                        {
                            if (paramValues.values != null)
                            {
                                command.Value.SetParameterValues("value", paramValues.values);
                            }
                            else if (paramValues.AutocompleteMethod != null)
                            {
                                command.Value.SetParameterValuesMethod("value", paramValues.AutocompleteMethod);
                            }
                            else
                            {
                                LogError("Failed to add parameter value for", command.Value.name, "value");
                            }
                        }
                    }
                }
            }
            catch (Exception e) { myModCommand.logSource.LogError(e); }

            try
            {
                foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    if (field.GetCustomAttribute<MyCommandAttribute>() is MyCommandAttribute attribute)
                    {
                        var command = myModCommand.CreateFieldCommand(attribute.name, field);
                        if (command.HasValue && field.GetCustomAttribute<ValuesAttribute>() is ValuesAttribute paramValues)
                        {
                            if (paramValues.values != null)
                            {
                                command.Value.SetParameterValues("value", paramValues.values);
                            }
                            else if (paramValues.AutocompleteMethod != null)
                            {
                                command.Value.SetParameterValuesMethod("value", paramValues.AutocompleteMethod);
                            }
                            else
                            {
                                LogError("Failed to add parameter value for", command.Value.name, "value");
                            }
                        }
                    }
                }
            }
            catch (Exception e) { myModCommand.logSource.LogError(e); }
        }
    }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
internal class MyCommandAttribute : Attribute
{
    public readonly string name;
    public MyCommandAttribute(string name)
    {
        this.name = name;
    }
}

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
internal class ValuesAttribute : Attribute
{

    public readonly string[] values = null;
    private readonly Type autocompleteMethodType;
    private readonly string autocompleteMethodName;
    public MethodInfo AutocompleteMethod => autocompleteMethodType.GetMethod(autocompleteMethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);


    public ValuesAttribute(params string[] values)
    {
        this.values = values;
    }

    public ValuesAttribute(Type autocompleteMethodType, string autocompleteMethodName)
    {
        this.autocompleteMethodType = autocompleteMethodType;
        this.autocompleteMethodName = autocompleteMethodName;
    }
}

