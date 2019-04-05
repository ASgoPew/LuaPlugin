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
        public static string[] LuaEnv = new string[Main.maxPlayers + 1];
        public static Dictionary<string, object> Data = new Dictionary<string, object>();

        #endregion

        #region Initialize

        public LuaPlugin(Main game) : base(game)
        {
            Instance = this;

            LuaConfig.Load();
        }

        public override void Initialize()
        {
            ServerApi.Hooks.ServerChat.Register(this, OnServerChat);
            ServerApi.Hooks.ServerCommand.Register(this, OnServerCommand);

            Commands.ChatCommands.Add(new Command(new List<string> { LuaConfig.ControlPermission }, LuaCommand, "lua")
            {
                AllowServer = true,
                HelpText = "Lua control"
            });

            InitializeEnvironments(TSPlayer.Server);
        }

        #endregion
        #region Dispose

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.ServerChat.Deregister(this, OnServerChat);
                ServerApi.Hooks.ServerCommand.Deregister(this, OnServerCommand);
            }
            DisposeEnvironments(TSPlayer.Server);
            base.Dispose(disposing);
        }

        #endregion
        #region Hook handlers

        public static void OnServerChat(ServerChatEventArgs args)
        {
            TSPlayer player = TShock.Players[args.Who];
            if (!player.HasPermission(LuaConfig.ExecutePermission) || args.Handled)
                return;
            args.Handled = CheckLuaInput(player, args.Text);
        }

        public static void OnServerCommand(CommandEventArgs args)
        {
            if (args.Handled)
                return;
            args.Handled = CheckLuaInput(TSPlayer.Server, args.Command);
        }

        public static bool CheckLuaInput(TSPlayer player, string text)
        {
            LuaEnvironment luaEnv = player.LuaEnv();
            if (text.StartsWith(LuaConfig.CommandSpecifier) && luaEnv != null)
            {
                RunLua(player, luaEnv, text.Substring(LuaConfig.CommandSpecifier.Length));
                return true;
            }
            return false;
        }

        public static void RunLua(TSPlayer player, LuaEnvironment luaEnv, string command)
        {
            Task.Run(() => RunLuaThread(player, luaEnv, command));
        }

        public static void RunLuaThread(TSPlayer player, LuaEnvironment luaEnv, string command)
        {
            try
            {
                luaEnv.RunScript(command, player);
            }
            catch (Exception e)
            {
                PrintError(player, luaEnv, e);
            }

            if (player.HasPermission(LuaConfig.ControlPermission))
                luaEnv.UpdateHooks();
        }

        public static void InitializeEnvironments(TSPlayer player)
        {
            foreach (var pair in LuaConfig.Environments)
                if (!InitializeEnvironment(pair.Value, player))
                    break;
        }

        public static bool InitializeEnvironment(LuaEnvironment luaEnv, TSPlayer player)
        {
            try
            {
                luaEnv.Initialize(player);
                return true;
            }
            catch (Exception e)
            {
                PrintError(player, luaEnv, e);
                return false;
            }
        }

        public static void DisposeEnvironments(TSPlayer player)
        {
            foreach (var pair in LuaConfig.Environments)
                DisposeEnvironment(pair.Value, player);
        }

        public static bool DisposeEnvironment(LuaEnvironment luaEnv, TSPlayer player)
        {
            try
            {
                luaEnv.Dispose();
                return true;
            }
            catch (Exception e)
            {
                PrintError(player, luaEnv, e);
                return false;
            }
        }

        #endregion
        #region LuaCommand

        public static void LuaCommand(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                HelpLuaCommand(args);
                return;
            }
            switch (args.Parameters[0].ToLower())
            {
                case "reset":
                    ResetLuaCommand(args);
                    break;
                case "select":
                    SelectLuaCommand(args);
                    break;
                case "reload":
                    ReloadLuaCommand(args);
                    break;
                case "help":
                    HelpLuaCommand(args);
                    break;
                default:
                    HelpLuaCommand(args);
                    break;
            }
        }

        public static void ResetLuaCommand(CommandArgs args)
        {
            if (args.Parameters.Count == 1)
            {
                LuaEnvironment lua = args.Player.LuaEnv();
                if (!DisposeEnvironment(lua, args.Player))
                    return;
                if (!InitializeEnvironment(lua, args.Player))
                    return;
                args.Player.SendSuccessMessage($"Lua[{lua.Name}] has been reset.");
            }
            else if (args.Parameters.Count == 2)
            {
                if (args.Parameters[1].ToLower() == "all")
                {
                    foreach (var pair in LuaConfig.Environments)
                    {
                        if (!DisposeEnvironment(pair.Value, args.Player))
                            return;
                        if (!InitializeEnvironment(pair.Value, args.Player))
                            return;
                    }
                    args.Player.SendSuccessMessage($"All Luas have been reset.");
                    return;
                }

                if (!LuaConfig.Environments.ContainsKey(args.Parameters[1]))
                {
                    args.Player.SendErrorMessage("No such environment.");
                    return;
                }
                LuaEnvironment lua = LuaConfig.Environments[args.Parameters[1]];
                if (!DisposeEnvironment(lua, args.Player))
                    return;
                if (!InitializeEnvironment(lua, args.Player))
                    return;
                args.Player.SendSuccessMessage($"Lua[{lua.Name}] has been reset.");
            }
        }

        public static void SelectLuaCommand(CommandArgs args)
        {
            if (args.Parameters.Count != 2)
            {
                args.Player.SendErrorMessage("Usage: /lua select <lua index>");
                return;
            }
            else if (!LuaConfig.Environments.ContainsKey(args.Parameters[1]))
            {
                args.Player.SendErrorMessage("No such environment.");
                return;
            }
            LuaEnv[args.Player.Index >= 0 ? args.Player.Index : Main.maxPlayers] = args.Parameters[1];
            args.Player.SendSuccessMessage($"Shifting to lua[{args.Player.LuaEnv().Name}]");
        }

        public static void ReloadLuaCommand(CommandArgs args)
        {

        }

        public static void HelpLuaCommand(CommandArgs args)
        {
            args.Player.SendInfoMessage("Usage: /lua <reset/select/reload/help>");
        }

        #endregion
        #region GetData

        public static object GetData(string key)
        {
            object result;
            lock (Data)
            {
                Data.TryGetValue(key, out result);
                return result;
            }
        }

        #endregion
        #region SetData

        public static void SetData(string key, object value)
        {
            lock (Data)
                Data[key] = value;
        }

        #endregion
        #region PrintError

        public static void PrintError(TSPlayer player, LuaEnvironment luaEnv, Exception e)
        {
            try
            {
                if (luaEnv.CallFunctionByName("perror", e) == null)
                    player.SendErrorMessage(e.ToString());
            }
            catch (Exception e2)
            {
                player.SendErrorMessage(e.ToString());
                player.SendErrorMessage($"Error at perror: {e2}");
            }
        }

        #endregion
    }
}