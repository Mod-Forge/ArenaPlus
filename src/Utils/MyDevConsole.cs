using BepInEx.Logging;
using DevConsole.Commands;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using static System.Net.Mime.MediaTypeNames;

namespace ArenaPlus.Utils
{
    internal static class MyDevConsole
    {
        public static ManualLogSource logSource = BepInEx.Logging.Logger.CreateLogSource("ArenaPlus:ConsoleWrite");
        public static RainWorld RW => UnityEngine.Object.FindObjectOfType<RainWorld>();

        public static Dictionary<string, CommandData> commands = new Dictionary<string, CommandData>();

        // Register Commands
        internal static void RegisterCommands()
        {
            commands.Add("debug", new CommandData("debug")
            {
                autoCompleteList = new string[][]
                {
                    new string[] { "1", "2" }
                }
            });

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
                    MethodInfo[] methodes = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);


                    foreach (MethodInfo methode in methodes)
                    {
                        if (methode.CustomAttributes.Count() > 0)
                        {
                            MyCommandAttribute attribute = methode.GetCustomAttribute<MyCommandAttribute>();

                            if (attribute != null)
                            {

                                string[][] autoCompleteList = methode.GetParameters().ToList().ConvertAll(param =>
                                {
                                    string type = param.ParameterType.ToString();

                                    Type pType = param.ParameterType;
                                    bool nullable = Nullable.GetUnderlyingType(pType) != null;
                                    if (nullable)
                                    {
                                        pType = Nullable.GetUnderlyingType(pType);
                                    }

                                    if (pType == typeof(Color))
                                    {
                                        type = $"Color{(nullable ? "?" : "")}(stringHex)";
                                    }

                                    if (pType == typeof(Player))
                                    {
                                        type = $"Player{(nullable ? "?" : "")}(intID)";
                                    }

                                    if (pType == typeof(RainWorldGame))
                                    {
                                        type = $"RainWorldGame{(nullable ? "?" : "")}(AUTO)";
                                    }

                                    if (pType == typeof(Room))
                                    {
                                        type = $"Room{(nullable ? "?" : "")}(stringName)";
                                    }

                                    if (pType == typeof(AbstractRoom))
                                    {
                                        type = $"AbstractRoom{(nullable ? "?" : "")}(stringName)";
                                    }

                                    if (pType == typeof(Vector2))
                                    {
                                        type = $"Vector2{(nullable ? "?" : "")}(floatX;floatY)";
                                    }

                                    if (pType == typeof(IntVector2))
                                    {
                                        type = $"Vector2{(nullable ? "?" : "")}(intX;intY)";
                                    }

                                    string text = $"{type} {param.Name}";

                                    if (param.HasDefaultValue && param.DefaultValue != null)
                                    {
                                        text += $" = {(param.DefaultValue is string ? $"'{param.DefaultValue}'" : param.DefaultValue.ToString())}";
                                    }

                                    return new string[] { text };
                                }).ToArray();


                                commands.Add(attribute.name, new CommandData(attribute.name)
                                {
                                    autoCompleteList = autoCompleteList,
                                    methode = methode,
                                    ownerType = !(methode.IsStatic || methode.IsAbstract) ? type : null,
                                });
                            }
                        }
                    }
                }
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
                return new string[] { "arg2_1", "arg2_2" };
            })
            .Register();
        }

        // Debugs commands
        internal static void ExecuteCommand(string[] args)
        {
            ConsoleWrite("");
            if (args.Length == 0)
            {
                ConsoleWrite("Error: invalid parameter", Color.red);
                return;
            }

            if (commands.ContainsKey(args[0]) && commands[args[0]].methode != null)
            {
                //var argList = args.ToList();
                //argList.RemoveAt(0);
                //ConsoleWrite($"execute command {args[0]}({string.Join(", ", argList)})");

                List<string> argsList = args.ToList();
                argsList.RemoveAt(0);

                Type[] types = commands[args[0]].methode.GetParameters().ToList().ConvertAll(p => p.ParameterType).ToArray();
                object[] typedArgs = new object[types.Length];
                for (int i = 0; i < typedArgs.Length; i++)
                {
                    if (i > argsList.Count() - 1)
                    {
                        typedArgs[i] = commands[args[0]].methode.GetParameters()[i].DefaultValue;
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

                    if (type == typeof(Color))
                    {
                        switch (argsList[i].ToLower())
                        {
                            case "white":
                                typedArgs[i] = Color.white;
                                continue;
                            case "black":
                                typedArgs[i] = Color.black;
                                continue;
                            case "cyan":
                                typedArgs[i] = Color.cyan;
                                continue;
                            case "red":
                                typedArgs[i] = Color.red;
                                continue;
                            case "green":
                                typedArgs[i] = Color.green;
                                continue;
                            case "blue":
                                typedArgs[i] = Color.blue;
                                continue;
                            case "yellow":
                                typedArgs[i] = Color.yellow;
                                continue;
                            case "gray":
                                typedArgs[i] = Color.gray;
                                continue;
                            case "magenta":
                                typedArgs[i] = Color.magenta;
                                continue;
                            case "clear":
                                typedArgs[i] = Color.clear;
                                continue;
                        }
                        if (!ColorUtility.TryParseHtmlString(argsList[i], out Color color))
                        {
                            ConsoleWrite("Invalid color: " + color, Color.red);
                        }
                        typedArgs[i] = color;
                        continue;
                    }

                    if (type == typeof(Player))
                    {
                        typedArgs[i] = (RW.processManager.currentMainLoop as RainWorldGame)?.Players[int.Parse(argsList[i])]?.realizedCreature as Player;
                        continue;
                    }

                    if (type == typeof(RainWorldGame))
                    {
                        typedArgs[i] = RW.processManager.currentMainLoop as RainWorldGame;
                        continue;
                    }

                    if (type == typeof(Room) || type == typeof(AbstractRoom))
                    {
                        typedArgs[i] = (RW.processManager.currentMainLoop as RainWorldGame).world.GetAbstractRoom(argsList[i]);
                        if (type == typeof(Room))
                            typedArgs[i] = (typedArgs[i] as AbstractRoom).realizedRoom;
                        continue;
                    }

                    if (type == typeof(Vector2) || type == typeof(IntVector2))
                    {
                        Vector2 vector = Vector2.zero;
                        if (argsList[i].ToLower() == "mouse")
                        {
                            vector = new Vector2(Futile.mousePosition.x, Futile.mousePosition.y) + (RW.processManager.currentMainLoop as RainWorldGame).cameras[0].pos;
                            if (type == typeof(IntVector2))
                            {
                                vector = (RW.processManager.currentMainLoop as RainWorldGame).cameras[0].room.GetTilePosition(vector).ToVector2();
                            }
                        }
                        else
                        {
                            string[] splitArg = argsList[i].Split(';');
                            vector.x = float.Parse(splitArg[0]);
                            vector.y = float.Parse(splitArg[1]);
                        }

                        if (type == typeof(IntVector2))
                        {
                            typedArgs[i] = new IntVector2((int)vector.x, (int)vector.y);
                        }
                        else
                        {
                            typedArgs[i] = vector;
                        }

                        continue;
                    }

                    if (type == typeof(float) && float.TryParse(argsList[i], out float fValue))
                    {
                        typedArgs[i] = fValue;
                        continue;
                    }

                    if (type == typeof(double) && double.TryParse(argsList[i], out double dValue))
                    {
                        typedArgs[i] = dValue;
                        continue;
                    }

                    if (type == typeof(Single) && Single.TryParse(argsList[i], out Single sValue))
                    {
                        typedArgs[i] = sValue;
                        continue;
                    }

                    if (type == typeof(int) && int.TryParse(argsList[i], out int iValue))
                    {
                        typedArgs[i] = iValue;
                        continue;
                    }

                    if (type == typeof(bool) && bool.TryParse(argsList[i], out bool bValue))
                    {
                        typedArgs[i] = bValue;
                        continue;
                    }

                    typedArgs[i] = argsList[i];
                }
                //ConsoleWrite($"types: {types.Length}, typedArgs: {typedArgs.Length}, typecount: {types.Count()}");
                object instace = null;

                if (commands[args[0]].ownerType != null)
                {
                    if (commands[args[0]].ownerType == typeof(RainWorldGame))
                    {
                        instace = RW.processManager.currentMainLoop as RainWorldGame;
                    }
                    else
                    {
                        int instanceIvoked = 0;
                        try
                        {
                            if (commands[args[0]].ownerType.IsInstanceOfType(typeof(AbstractWorldEntity)) || commands[args[0]].ownerType.IsSubclassOf(typeof(AbstractWorldEntity)))
                            {
                                List<AbstractWorldEntity> entities = (RW.processManager.currentMainLoop as RainWorldGame).cameras[0].room.abstractRoom.entities;
                                for (int i = 0; i < entities.Count(); i++)
                                {
                                    if (entities[i].GetType() == commands[args[0]].ownerType)
                                    {
                                        commands[args[0]].methode.Invoke(entities[i], typedArgs);
                                        instanceIvoked++;
                                    }
                                }
                            }
                            else if (commands[args[0]].ownerType.IsInstanceOfType(typeof(UpdatableAndDeletable)) || commands[args[0]].ownerType.IsSubclassOf(typeof(UpdatableAndDeletable)))
                            {
                                List<UpdatableAndDeletable> updateList = (RW.processManager.currentMainLoop as RainWorldGame).cameras[0].room.updateList;
                                for (int i = 0; i < updateList.Count(); i++)
                                {
                                    if (updateList[i].GetType() == commands[args[0]].ownerType)
                                    {
                                        commands[args[0]].methode.Invoke(updateList[i], typedArgs);
                                        instanceIvoked++;
                                    }
                                }
                            }
                        }
                        catch { }


                        if (instanceIvoked == 0)
                        {
                            ConsoleWrite($"Error: {commands[args[0]].methode.Name}({string.Join(", ", commands[args[0]].methode.GetParameters().ToList().ConvertAll(p => $"{p.ParameterType} {p.Name}"))}) needs an {commands[args[0]].ownerType} instance to be Invoke", Color.red);
                        }
                        else
                        {
                            ConsoleWrite($"Fonction called for {instanceIvoked} instances", Color.gray);
                        }
                        return;
                    }
                }
                commands[args[0]].methode.Invoke(instace, typedArgs);
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

        [MyCommand("consolewrite")]
        public static void ConsoleWrite(string message = "", Color? color = null)
        {
            try
            {
                GameConsoleWriteLine(message, color ?? Color.white);
            }
            catch { }
        }

        [MyCommand("killplayer")]
        public static void KillPlayer(Player player)
        {
            player.Die();
        }

        [MyCommand("beatgame")]
        public static void BeatGame(this RainWorldGame game)
        {
            RainWorldGame.BeatGameMode(game, true);
        }

        [MyCommand("teleport_player")]
        public static void TeleportPlayer(Room room, Player player, IntVector2 vector)
        {
            ConsoleWrite($"Imagine that i teleport '{player}' in room '{room.abstractRoom.name}' at '{vector}'");
        }

        public class CommandData
        {
            public CommandData(string name)
            {
                this.name = name;
            }

            public readonly string name;
            public string[][] autoCompleteList = new string[][] { };
            public Func<string[], IEnumerable<string>> handler = null;
            public MethodInfo methode = null;
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

        [AttributeUsage(AttributeTargets.Method)]
        public class MyCommandAttribute : Attribute
        {
            public readonly string name;
            public int count;
            public MyCommandAttribute(string name)
            {
                this.name = name;
            }
        }

        private static void GameConsoleWriteLine(string message, Color color)
        {
            DevConsole.GameConsole.WriteLine(message, color);
            logSource.LogMessage(message);
        }
    }
}
