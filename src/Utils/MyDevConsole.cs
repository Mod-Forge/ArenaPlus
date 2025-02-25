using BepInEx.Logging;
using DevConsole.Commands;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static ArenaPlus.Utils.AssemblyUtils;

namespace ArenaPlus.Utils;
internal static class MyDevConsole
{
    private static ManualLogSource logSource = BepInEx.Logging.Logger.CreateLogSource("ArenaPlus:ConsoleWrite");
    public static RainWorld RW => UnityEngine.Object.FindObjectOfType<RainWorld>();

    private static Dictionary<string, CommandData> commands = new Dictionary<string, CommandData>();
    private static Dictionary<Type, TypeResolver> resolvers = new Dictionary<Type, TypeResolver>();

    // Register Commands
    internal static void Register()
    {
        RegisterResolvers();
        RegisterCommands();
    }

    internal static void RegisterResolvers()
    {
        resolvers.Add(typeof(Color), new TypeResolver()
        {
            ToText = nullable => $"Color{(nullable ? "?" : "")}(stringHex|colorName)",
            Parse = (text, type) =>
            {
                switch (text.ToLower())
                {
                    case "white":
                        return Color.white;
                    case "black":
                        return Color.black;
                    case "cyan":
                        return Color.cyan;
                    case "red":
                        return Color.red;
                    case "green":
                        return Color.green;
                    case "blue":
                        return Color.blue;
                    case "yellow":
                        return Color.yellow;
                    case "gray":
                        return Color.gray;
                    case "magenta":
                        return Color.magenta;
                    case "clear":
                        return Color.clear;
                }
                if (!ColorUtility.TryParseHtmlString(text, out Color color))
                {
                    ConsoleWrite("Invalid color: " + color, Color.red);
                }
                return color;
            }
        });

        resolvers.Add(typeof(Player), new TypeResolver()
        {
            ToText = nullable => $"Player{(nullable ? "?" : "")}(intID)",
            Parse = (text, type) => (RW.processManager.currentMainLoop as RainWorldGame)?.Players[int.Parse(text)]?.realizedCreature as Player
        });

        resolvers.Add(typeof(RainWorldGame), new TypeResolver()
        {
            ToText = nullable => $"RainWorldGame{(nullable ? "?" : "")}(AUTO)",
            Parse = (text, type) => RW.processManager.currentMainLoop as RainWorldGame,
            GetInstances = (type) => [RW.processManager.currentMainLoop as RainWorldGame]
        });

        resolvers.Add(typeof(Room), new TypeResolver()
        {
            ToText = nullable => $"Room{(nullable ? "?" : "")}(stringName)",
            Parse = (text, type) => (RW.processManager.currentMainLoop as RainWorldGame).world.GetAbstractRoom(text).realizedRoom
        });

        resolvers.Add(typeof(AbstractRoom), new TypeResolver()
        {
            ToText = nullable => $"AbstractRoom{(nullable ? "?" : "")}(stringName)",
            Parse = (text, type) => (RW.processManager.currentMainLoop as RainWorldGame).world.GetAbstractRoom(text)
        });

        Func<string, Type, object> vectorParser = (text, type) =>
        {
            Vector2 vector = Vector2.zero;
            if (text.ToLower() == "mouse")
            {
                vector = new Vector2(Futile.mousePosition.x, Futile.mousePosition.y) + (RW.processManager.currentMainLoop as RainWorldGame).cameras[0].pos;
                if (type == typeof(IntVector2))
                {
                    vector = (RW.processManager.currentMainLoop as RainWorldGame).cameras[0].room.GetTilePosition(vector).ToVector2();
                }
            }
            else
            {
                string[] splitArg = text.Split(';');
                vector.x = float.Parse(splitArg[0]);
                vector.y = float.Parse(splitArg[1]);
            }

            if (type == typeof(IntVector2))
            {
                return new IntVector2((int)vector.x, (int)vector.y);
            }
            else
            {
                return vector;
            }
        };

        resolvers.Add(typeof(Vector2), new TypeResolver()
        {
            ToText = nullable => $"Vector2{(nullable ? "?" : "")}(floatX;floatY)",
            Parse = (text, type) => vectorParser
        });

        resolvers.Add(typeof(IntVector2), new TypeResolver()
        {
            ToText = nullable => $"Vector2{(nullable ? "?" : "")}(intX;intY)",
            Parse = (text, type) => vectorParser
        });

        resolvers.Add(typeof(float), new TypeResolver()
        {
            Parse = (text, type) => float.TryParse(text, out var val) ? val : default,
        });

        resolvers.Add(typeof(double), new TypeResolver()
        {
            Parse = (text, type) => double.TryParse(text, out var val) ? val : default,
        });

        resolvers.Add(typeof(int), new TypeResolver()
        {
            Parse = (text, type) => int.TryParse(text, out var val) ? val : default,
        });

        resolvers.Add(typeof(bool), new TypeResolver()
        {
            Parse = (text, type) => bool.TryParse(text, out var val) ? val : default,
        });

        resolvers.Add(typeof(AbstractWorldEntity), new TypeResolver()
        {
            GetInstances = (type) =>
            {
                return (RW.processManager.currentMainLoop as RainWorldGame).cameras[0].room.abstractRoom.entities.FindAll(e => type.IsAssignableFrom(e.GetType())).ToArray();
            }
        });

        resolvers.Add(typeof(UpdatableAndDeletable), new TypeResolver()
        {
            GetInstances = (type) =>
            {
                return (RW.processManager.currentMainLoop as RainWorldGame).cameras[0].room.updateList.FindAll(e => type.IsAssignableFrom(e.GetType())).ToArray();
            }
        });
    }

    internal static void RegisterCommands()
    {
        commands.Add(key: "debug", new CommandData("debug")
        {
            autoCompleteList = new string[][]
            {
                        new string[] { "1", "2" }
            }
        });

        Type[] types = GetLocalAssebly().GetTypesSafe();

        foreach (Type type in types)
        {
            try
            {
                MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

                foreach (MethodInfo method in methods)
                {
                    if (method.CustomAttributes.Count() > 0 && method.GetCustomAttribute<MyCommandAttribute>() is MyCommandAttribute attribute)
                    {
                        RegisterCommand(attribute, method);
                    }
                }
            } catch (Exception e) { LogError(e); }

            try
            {
                foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    if (prop.GetSetMethod(true) is MethodInfo setMethod && prop.GetCustomAttribute<MyCommandAttribute>() is MyCommandAttribute attribute)
                    {
                        RegisterCommand(attribute, setMethod);
                    }
                }
            }
            catch (Exception e) { LogError(e); }
        }



        new CommandBuilder("ArenaPlus")
        .Run(args =>
        {
            try
            {
                ExecuteCommand(args);
            }
            catch (Exception e) { ConsoleWrite("Error in command: " + e.Message, Color.red); LogError(e); }
        })
        .AutoComplete((x) =>
        {
            if (x.Length == 0)
            {
                return commands.ToList().ConvertAll(c => c.Value.name).ToArray<string>();
            }
            else if (commands.ContainsKey(x[0]))
            {
                //ConsoleWrite($"have custom handler: {MyDevConsole.commands[x[0]].handler != null}");
                //ConsoleWrite($"return value: {string.Join(", ", (MyDevConsole.commands[x[0]].handler != null ? MyDevConsole.commands[x[0]].handler.Invoke(x) : MyDevConsole.commands[x[0]].DefaultHandler(x)))}");
                return commands[x[0]].handler != null ? commands[x[0]].handler.Invoke(x) : commands[x[0]].DefaultHandler(x);
            }
            return new string[] { "Error: no argument found", "Error: no argument found", "Error: no argument found", "Error: no argument found", "Error: no argument found", "Error: no argument found", "Error: no argument found", "Error: no argument found", "Error: no argument found", "Error: no argument found", "Error: no argument found", };
        })
        .Register();
    }

    internal static void RegisterCommand(MyCommandAttribute myCommand, MethodInfo method)
    {
        string[][] autoCompleteList = method.GetParameters().ToList().ConvertAll(param =>
        {
            string strType = param.ParameterType.ToString();

            Type pType = param.ParameterType;
            bool nullable = Nullable.GetUnderlyingType(pType) != null;
            if (nullable)
            {
                pType = Nullable.GetUnderlyingType(pType);
            }

            if (resolvers.ContainsKey(pType) && resolvers[pType].ToText != null)
            {
                strType = resolvers[pType].ToText.Invoke(nullable);
            }

            string text = $"{strType} {param.Name}";

            if (param.HasDefaultValue && param.DefaultValue != null)
            {
                string defaultVal = param.DefaultValue.ToString();
                if (param.DefaultValue is string)
                {
                    defaultVal = $"\"{defaultVal}\"";
                }
                if (param.DefaultValue is char)
                {
                    defaultVal = $"'{defaultVal}'";
                }
                text += $" = {defaultVal}";
            }

            return new string[] { text };
        }).ToArray();


        commands.Add(myCommand.name, new CommandData(myCommand.name)
        {
            autoCompleteList = autoCompleteList,
            method = method,
            ownerType = !(method.IsStatic || method.IsAbstract) ? method.DeclaringType : null,
        });
    }

    internal static void ExecuteCommand(string[] args)
    {
        ConsoleWrite("");
        if (args.Length == 0)
        {
            ConsoleWrite("Error: invalid parameter", Color.red);
            return;
        }

        if (commands.ContainsKey(args[0]) && commands[args[0]].method != null)
        {
            //var argList = args.ToList();
            //argList.RemoveAt(0);
            //ConsoleWrite($"execute command {args[0]}({string.Join(", ", argList)})");

            List<string> argsList = args.ToList();
            argsList.RemoveAt(0);

            Type[] types = commands[args[0]].method.GetParameters().ToList().ConvertAll(p => p.ParameterType).ToArray();
            object[] typedArgs = new object[types.Length];
            for (int i = 0; i < typedArgs.Length; i++)
            {
                if (i > argsList.Count() - 1)
                {
                    typedArgs[i] = commands[args[0]].method.GetParameters()[i].DefaultValue;
                    continue;
                }

                //ConsoleWrite($"arg[{i}]: {argsList[i]}");

                if (argsList[i] == "null")
                {
                    typedArgs[i] = null;
                    continue;
                }

                Type type = Nullable.GetUnderlyingType(types[i]) != null ? Nullable.GetUnderlyingType(types[i]) : types[i];

                if (type == typeof(string))
                {
                    typedArgs[i] = argsList[i];
                    continue;
                }

                if (resolvers.ContainsKey(type) && resolvers[type].Parse != null)
                {
                    typedArgs[i] = resolvers[type].Parse.Invoke(argsList[i], type);
                    continue;
                }

                typedArgs[i] = argsList[i];
            }
            //ConsoleWrite($"types: {types.Length}, typedArgs: {typedArgs.Length}, typecount: {types.Count()}");
            object instace = null;

            if (commands[args[0]].ownerType != null)
            {
                int instanceIvoked = 0;

                try
                {
                    foreach (var resolver in resolvers)
                    {
                        if (resolver.Value.GetInstances != null && resolver.Key.IsAssignableFrom(commands[args[0]].ownerType))
                        {
                            foreach (var inst in resolver.Value.GetInstances(commands[args[0]].ownerType))
                            {
                                commands[args[0]].method.Invoke(inst, typedArgs);
                                instanceIvoked++;
                            }
                        }
                    }
                }
                catch (Exception e) { LogError(e); }

                if (instanceIvoked == 0)
                {
                    ConsoleWrite($"Error: {commands[args[0]].method.Name}({string.Join(", ", commands[args[0]].method.GetParameters().ToList().ConvertAll(p => $"{p.ParameterType} {p.Name}"))}) needs an {commands[args[0]].ownerType} instance to be Invoke", Color.red);
                }
                else
                {
                    ConsoleWrite($"Fonction called for {instanceIvoked} instances", Color.gray);
                }
                return;
            }
            commands[args[0]].method.Invoke(instace, typedArgs);
        }

        if (args[0] == "debug")
        {
            if (args[1] == "1")
            {
                ConsoleWrite("DebugCommand[1] : " + "hello world", Color.green);
            }
            else if (args[1] == "2")
            {
                ConsoleWrite("DebugCommand[2] : " + "hello world", Color.yellow);
            }
        }
    }

    // Debugs commands

    public static void ConsoleWrite(object message = null, Color? color = null)
    {
        try
        {
            GameConsoleWriteLine(message != null ? message : "", color.HasValue ? color.Value : Color.white);
        }
        catch { }
    }

    public struct CommandData
    {
        public CommandData(string name)
        {
            this.name = name;
        }

        public readonly string name;
        public string[][] autoCompleteList = new string[][] { };
        public Func<string[], IEnumerable<string>> handler = null;
        public MethodInfo method = null;
        public Type ownerType = null;

        public IEnumerable<string> DefaultHandler(string[] x)
        {
            if (autoCompleteList.GetLength(0) >= x.Length)
            {
                return autoCompleteList[x.Length - 1];
            }
            return new string[] { };
        }

    }

    public class TypeResolver()
    {
        public Func<bool, string> ToText = null;
        public Func<string, Type, object> Parse = null;
        public Func<Type, object[]> GetInstances = null;
    }

    private static void GameConsoleWriteLine(object message, Color color)
    {
        DevConsole.GameConsole.WriteLine(message.ToString(), color);
        if (color == Color.red)
        {
            LogError(message);
        }
        else if (color == Color.yellow)
        {
            LogWarning(message);
        }
        LogMessage(message);
    }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public class MyCommandAttribute : Attribute
{
    public readonly string name;
    public int count;
    public MyCommandAttribute(string name)
    {
        this.name = name;
    }
}
