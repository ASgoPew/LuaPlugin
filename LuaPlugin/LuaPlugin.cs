using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using NLua;
using System.IO;
using Terraria.ID;
using System.Threading;
using System.Threading.Tasks;
using TShockAPI.Hooks;
using System.ComponentModel;
using System.Diagnostics;
using MyLua;

namespace LuaPlugin
{
    [ApiVersion(2, 1)]
    public class LuaPlugin : TerrariaPlugin
    {
        #region Data

        public override string Author => "ASgo";
        public override string Description => "Plugin that provides lua to server development";
        public override string Name => "LuaPlugin";
        public override Version Version => new Version(1, 0, 0, 0);
        
        public static LuaPlugin Instance = null;
        public static string[] LuaEnvIndex = new string[Main.maxPlayers + 1];
        public static TSPlayer Me = null;
        public static bool GameInitialized = false;
        public static Dictionary<string, object> Data = new Dictionary<string, object>();

        #endregion

        #region Initialize

        public LuaPlugin(Main game) : base(game)
        {
            Instance = this;

            Config.Load();

            if (!ReadLuaEnvironments())
            {
                Console.ReadKey();
                Environment.Exit(1);
                //TShockAPI.Utils.Instance.StopServer(false, "Lua cannot initialize."); // TShock won't be initialized at this point
            }

            for (int i = 0; i < LuaEnvIndex.Length; i++)
                LuaEnvIndex[i] = Config.DefaultLua;
        }

        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, OnGameInitialize);
            ServerApi.Hooks.GamePostInitialize.Register(this, OnGamePostInitialize, Int32.MinValue);
            ServerApi.Hooks.ServerChat.Register(this, OnServerChat);
            ServerApi.Hooks.ServerCommand.Register(this, OnServerCommand);
            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
            ServerApi.Hooks.ServerConnect.Register(this, OnServerConnect); // DEBUG SHIT

            Commands.ChatCommands.Add(new Command(new List<string> { "lua.contol" }, LuaChatCommand, "lua")
            {
                AllowServer = true,
                HelpText = "Lua control."
            });
        }

        #endregion
        #region Dispose

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameInitialize.Deregister(this, OnGameInitialize);
                ServerApi.Hooks.GamePostInitialize.Deregister(this, OnGamePostInitialize);
                ServerApi.Hooks.ServerChat.Deregister(this, OnServerChat);
                ServerApi.Hooks.ServerCommand.Deregister(this, OnServerCommand);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
            }
            foreach (var pair in Config.Environments)
                pair.Value.Dispose();
            base.Dispose(disposing);
        }

        #endregion
        #region Helper data and methods

        //public delegate void handler0();
        //public delegate void handler1(EventArgs args);
        //public delegate void handler2(object sender, EventArgs args);

        //public static handler0 CreateDelegate0(LuaFunction f)
        //{
        //    return () => { f.Call(); };
        //}
        //public static handler1 CreateDelegate1(LuaFunction f)
        //{
        //    return (arg1) => { f.Call(arg1); };
        //}
        //public static handler2 CreateDelegate2(LuaFunction f)
        //{
        //    return (arg1, arg2) => { f.Call(arg1, arg2); };
        //    //lua.RegisterLuaDelegateType
        //}

        //public static HookHandler<EventArgs> CreateHookHandler(LuaFunction f)
        //{
        //    return (arg1) => { f.Call(arg1); };
        //}

        // Requires dynamic generic Action generation
        /*public static object CreateAction(LuaFunction f, Type[] args)
        {
            switch(args.Length)
            {
                case 0:
                    return new Action(() => { f.Call(); });
                    break;
                case 1:
                    return new Action<object>((arg0) => { f.Call(arg0); });
                    break;
                case 2:
                    return new Action<object>((arg0) => { f.Call(arg0); });
                    break;
            }
            return null;
        }*/

        //public static Action CreateAction0(LuaFunction f)
        //{
        //    return () => { f.Call(); };
        //}
        //public static Action<object> CreateAction1(LuaFunction f)
        //{
        //    return (arg1) => { f.Call(arg1); };
        //}
        /*public static Action<BinaryWriter> CreateWriter(LuaFunction f)
        {
            return (bw) => { f.Call(bw); };
        }*/
        #endregion
        #region Hook handlers
        
        public void OnServerLeave(LeaveEventArgs args)
        {
            if (Me != null && args.Who == Me.Index)
                Me = TSPlayer.Server;
        }

        public bool ReadLuaEnvironments()
        {
            if (!Directory.Exists(Path.Combine(Path.Combine(Config.Path, Config.Key), "0")))
            {
                // Here generating initial lua scripts directory structure
                // TODO
                string path = Path.Combine(Config.Path, Config.Key);
                Directory.CreateDirectory(Path.Combine(path, "0"));
                File.WriteAllText(Path.Combine(path, "init.lua"),
@"import ('mscorlib', 'System')
import('TShockAPI', 'TShockAPI')

function print(o)
    TSPlayer.All:SendInfoMessage(tostring(o))
    Console.WriteLine(tostring(o))
end");
            }
            List<string> folders = Directory.GetDirectories(Path.Combine(Config.Path, Config.Key)).ToList();
            folders.Sort(delegate (string folder1, string folder2)
            {
                int index1, index2;
                if (int.TryParse(Path.GetFileName(folder1), out index1) && int.TryParse(Path.GetFileName(folder2), out index2))
                {
                    if (index1 < index2)
                        return -1;
                    else
                        return 1;
                }
                return -1;
            });
            int index = 0;
            foreach (string envDir in folders)
            {
                if (Path.GetFileName(envDir) == index.ToString())
                {
                    Luas.Add(new LuaEnvironment2(index, envDir));
                    if (!Luas[index++].Initialize(null, false))
                        return false;
                }
            }
            return true;
        }

        public void OnServerChat(ServerChatEventArgs args)
        {
            TSPlayer player = TShock.Players[args.Who];
            if (!player.HasPermission(Config.ExecutePermission))
                return;
            args.Handled = args.Handled || CheckLuaInput(player, args.Text);
        }

        public void OnServerCommand(CommandEventArgs args)
        {
            args.Handled = args.Handled || CheckLuaInput(TSPlayer.Server, args.Command);
        }

        // TODO: Refactor it and make ability to customize every alias and aliases count
        public static bool CheckLuaInput(TSPlayer player, string text)
        {
            if (text.StartsWith(OnTickCommandSpecifier))
            {
                string onTickBody = text.Substring(OnTickCommandSpecifier.Length);
                LuaEnvironment2 lua = player.LuaEnv();
                if (onTickBody.Length > 0)
                    RunLua(player, "OnTick = function(frame) " + onTickBody + " end"); // OnTick or OnUpdate?
                else
                    lua.Unhook("OnTick");
                return true;
            }
            else if (text.StartsWith(PrintCommandSpecifier))
            {
                RunLua(player, "print(" + text.Substring(PrintCommandSpecifier.Length) + ")");
                return true;
            }
            else if (text.StartsWith(ShowCommandSpecifier) && text.Length > ShowCommandSpecifier.Length)
            {
                RunLua(player, "show(" + text.Substring(ShowCommandSpecifier.Length) + ")");
                return true;
            }
            else if (text.StartsWith(SharpShowCommandSpecifier) && text.Length > SharpShowCommandSpecifier.Length)
            {
                RunLua(player, "print(SharpShow(" + text.Substring(SharpShowCommandSpecifier.Length) + "))");
                return true;
            }
            else if (text.StartsWith(Config.CommandSpecifier) && text.Length > Config.CommandSpecifier.Length)
            {
                RunLua(player, text.Substring(Config.CommandSpecifier.Length));
                return true;
            }
            return false;
        }
        #endregion

        #region Commands
        /*public bool HasPermission(string perm)
        {
            return true;
        }*/

        public void LuaChatCommand(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                HelpLuaCommand(args);
            }
            else
            {
                switch (args.Parameters[0].ToLower())
                {
                    case "reset":
                        InitializeLuaCommand(args);
                        break;
                    case "add":
                        AddLuaCommand(args);
                        break;
                    case "remove":
                        RemoveLuaCommand(args);
                        break;
                    case "select":
                        SelectLuaCommand(args);
                        break;
                    case "help":
                        HelpLuaCommand(args);
                        break;
                    case "executing":
                        args.Player.SendInfoMessage(args.Player.LuaEnv().GetState().IsExecuting.ToString());
                        break;
                    case "forcestop": // WARNING: This command is not safe. It can cause AccessViolationException that can't be handled by try-catch or UnhandledException handler. Use at your own risk.
                        LuaEnvironment2 lua = args.Player.LuaEnv();
                        lua.ForceStop();
                        Task.Delay(1000).ContinueWith(_ => lua.ForceResume());
                        break;
                    default:
                        args.Player.SendErrorMessage("Invalid /lua syntax. Try /lua help");
                        break;
                }
            }
        }

        public void InitializeLuaCommand(CommandArgs args)
        {
            Me = args.Player;
            if (args.Parameters.Count == 1)
            {
                LuaEnvironment2 lua = args.Player.LuaEnv();
                if (lua.Initialize(Me, true))
                    Me.SendSuccessMessage($"Lua[{lua.Index}] has been reset.");
                else
                    Me.SendErrorMessage($"Lua[{lua.Index}] reset failed.");
            } else if (args.Parameters.Count == 2)
            {
                if (args.Parameters[1].ToLower() == "all")
                {
                    for (int i = 0; i < Luas.Count; i++)
                    {
                        if (!Luas[i].Initialize(Me, true))
                        {
                            Me.SendErrorMessage($"Lua[{i}] reset failed.");
                            return;
                        }
                    }
                    Me.SendSuccessMessage($"All Luas have been reset.");
                    return;
                }
                int index;
                if (!Int32.TryParse(args.Parameters[1], out index) || index < 0 || index >= Luas.Count)
                {
                    Me.SendErrorMessage("Index must be a number and in array bounds.");
                    return;
                }
                if (Luas[index].Initialize(Me, true))
                    Me.SendSuccessMessage($"Lua[{index}] has been reset.");
                else
                    Me.SendErrorMessage($"Lua[{index}] reset failed.");
            }
        }

        public void AddLuaCommand(CommandArgs args)
        {
            //me = args.Player;
            LuaEnvironment2 lua = new LuaEnvironment2(Luas.Count);
            Luas.Add(lua);
            if (!args.Player.LuaEnv().Initialize(Me, true))
                return;
            LuaEnvIndex[args.Player.Index >= 0 ? args.Player.Index : Main.maxPlayers] = lua.Index;
            args.Player.SendSuccessMessage($"New lua[{lua.Index}] added.");
            args.Player.SendInfoMessage($"Shifting to lua[{lua.Index}]");
        }

        public void SelectLuaCommand(CommandArgs args)
        {
            if (args.Parameters.Count != 2)
            {
                args.Player.SendErrorMessage("Usage: /lua select <lua index>");
                return;
            }
            int index;
            if (!Int32.TryParse(args.Parameters[1], out index) || index < 0 || index >= Luas.Count)
            {
                args.Player.SendErrorMessage("Index must be a number and in array bounds.");
                return;
            }
            LuaEnvIndex[args.Player.Index >= 0 ? args.Player.Index : Main.maxPlayers] = index;
            args.Player.SendSuccessMessage($"Shifting to lua[{index}]");
        }

        public void RemoveLuaCommand(CommandArgs args)
        {
            if (args.Parameters.Count != 2)
            {
                args.Player.SendErrorMessage("Usage: /lua remove <lua index>");
                return;
            }
            int index;
            if (!Int32.TryParse(args.Parameters[1], out index) || index <= 0 || index >= Luas.Count)
            {
                args.Player.SendErrorMessage("Index must be a number, > 0 (you can't remove lua[0]) and in array bounds.");
                return;
            }
            Luas[index].Dispose();
            Luas.RemoveAt(index);
            for (int i = index; i < Luas.Count; i++)
                Luas[i].Index = i;
            for (int i = 0; i < 255; i++)
            {
                if (LuaEnvIndex[i] == index)
                {
                    LuaEnvIndex[i] = 0;
                    if (TShock.Players[i] != null && TShock.Players[i].Active)
                        TShock.Players[i].SendSuccessMessage($"Removed lua[{index}], shifting to lua[0].");
                } else
                    args.Player.SendSuccessMessage($"Removed lua[{index}]");
            }
            if (LuaEnvIndex[Main.maxPlayers] == index)
                LuaEnvIndex[Main.maxPlayers] = 0;
        }

        public void HelpLuaCommand(CommandArgs args)
        {
            args.Player.SendInfoMessage("Usage: /lua <reset/add/remove/select/help>");
        }
        #endregion

        public static void RunLua(TSPlayer player, string command)
        {
            //LuaThreadFunction(player, command);
            //ThreadPool.QueueUserWorkItem(_ => LuaThreadFunction(player, command));
            try
            {
                Thread t = new Thread(new ParameterizedThreadStart(LuaThreadFunction));
                t.Start(new Tuple<TSPlayer, string>(player, command));
                /*Task.Delay(untrusted ? maxUntrustedTimeAmount : maxTimeAmount).ContinueWith(_ =>
                {
                    if (t.IsAlive)
                    {
                        //t.Abort();
                        //player.SendErrorMessage($"Lua[{luaIndex}] script execution time has exceeded allowed value.");
                        lua.ForceStop();
                    }
                });*/
            } catch (Exception e)
            {
                TShock.Log.ConsoleError(e.ToString());
            }
        }

        public static void LuaThreadFunction(object data)
        {
            Tuple<TSPlayer, string> tuple = (Tuple <TSPlayer, string>)data;
            TSPlayer player = tuple.Item1;
            string command = tuple.Item2;

            LuaEnvironment2 lua = player.LuaEnv(); // luaIndex can be modified during DoString(...)

            // TODO: Make unsafe userscript run ability (since currently all hooks wait for all other scripts to end)
            // Just let hooks run in another LuaEnvironment
            object oldSource = lua.Data["source"];
            if (lua.Execute(command, (_lua, state) => {
                if (state == 0)
                {
                    Me = player;
                    lua.Data["source"] = Me;
                }
                else if (state > 0)
                    lua.Data["source"] = oldSource;
            }, $"LuaThreadFunction") == null)
                return;
            if (player.HasPermission("lua.control"))
                lua.UpdateHooks();
        }

        public object GetData(string key)
        {
            object result;
            lock (Data)
            {
                Data.TryGetValue(key, out result);
                return result;
            }
        }

        public void SetData(string key, object value)
        {
            lock (Data)
                Data[key] = value;
        }

        public static string Test(string s)
        {
            /*PacketTypes.Status
            OTAPI.Hooks.Npc.
            TSPlayer.All.
            TSPlayer.Server.;
            Main.projectile[0].;
            TShock.Players[0];
            NetMessage.SendData(PacketTypes.ProjectileDestroy, -1, -1, )*/

            Console.WriteLine("Русский text");
            //luaEnv.Execute("p('ну и чо такое то')");
            //luaEnv.Execute("print('ну и чо такое то')");
            TSPlayer.All.Teleport(0, 0, 2);
            return "абвгд " + s;
        }

        public static void kek()
        {
            lol(new object[] {1, 2, 3, 4, 5});
        }

        public static void lol(params object[] args)
        {
            Console.WriteLine(args.Length);
        }
    }
}