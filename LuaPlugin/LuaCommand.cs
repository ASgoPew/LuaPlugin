﻿using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using MyLua;

namespace LuaPlugin
{
    // LuaTable can be disposed!
    public class LuaCommand : ILuaCommand
    {
        private bool Disposed = false;
        public LuaFunction Function;
        public LuaEnvironment LuaEnv;
        public Lua Lua;
        public Command Cmd;

        public LuaCommand(LuaEnvironment luaEnv, object namesObject, object permissionObject, LuaTable parameters, LuaFunction function)
        {
            this.Function = function;
            this.LuaEnv = luaEnv;
            this.Lua = luaEnv.GetState(); // TODO: Changing f might crash on Dispose, since new f can have different interpreter

            string[] names;
            if (namesObject.GetType() == typeof(LuaTable))
            {
                LuaTable t = namesObject as LuaTable;
                names = new string[t.Keys.Count];
                int i = 0;
                foreach (var o in t)
                {
                    names[i++] = (string)(((KeyValuePair<Object, Object>)o).Value);
                }
            }
            else if (namesObject.GetType() == typeof(string))
                names = new string[1] { (string)namesObject };
            else
            {
                luaEnv.RaiseLuaException($"Command: <unknown>", new ArgumentException("LuaCommand.LuaCommand(LuaEnvironment luaEnv, object names_object, object permission_object, LuaTable parameters, LuaFunction f): Invalid name parameter"));
                return;
            }
            if (names.Length == 0)
            {
                luaEnv.RaiseLuaException($"Command: <unknown>", new ArgumentException("LuaCommand.LuaCommand(LuaEnvironment luaEnv, object names_object, object permission_object, LuaTable parameters, LuaFunction f): Invalid name parameter"));
                return;
            }

            List<string> permissions = new List<string>();
            if (permissionObject.GetType() == typeof(LuaTable))
            {
                LuaTable t = permissionObject as LuaTable;
                foreach (var o in t)
                    permissions.Add((string)((KeyValuePair<Object, Object>)o).Value);
            }
            else if (permissionObject.GetType() == typeof(string))
                permissions.Add((string)permissionObject);
            else
            {
                luaEnv.RaiseLuaException($"Command: {names[0]}", new ArgumentException("LuaCommand.LuaCommand(LuaEnvironment luaEnv, object names_object, object permission_object, LuaTable parameters, LuaFunction f): Invalid permission parameter"));
                return;
            }
            bool allowServer = (bool)(parameters["AllowServer"] ?? true);
            string helpText = (string)(parameters["HelpText"] ?? "Temporarily command");
            bool doLog = (bool)(parameters["DoLog"] ?? false);
            this.Cmd = new Command(permissions, Invoke, names)
            {
                AllowServer = allowServer,
                HelpText = helpText,
                DoLog = doLog
            };
            Commands.ChatCommands.Add(Cmd);
            luaEnv.LuaCommands.Add(this);
        }

        public void Dispose()
        {
            if (Disposed)
                return;
            Disposed = true;

            Commands.ChatCommands.Remove(Cmd);
            if (Lua.IsEnabled()) // WILL THIS CRASH?
                Function.Dispose();
            Function = null;
            LuaEnv = null;
            Lua = null;
            Cmd = null;
        }

        public void Invoke(CommandArgs args)
        {
            if (Disposed)
            {
                LuaEnv.RaiseLuaException($"Command: {Cmd.Name}", new ArgumentException("LuaCommand is already disposed but trying to invoke it."));
                return;
            }
            if (Lua.IsEnabled())
                LuaEnv.CallFunction(Function, args);
            else
            {
                LuaEnv.RaiseLuaException($"Command: {Cmd.Name}", new ArgumentException("Trying to invoke LuaCommand while corresponding lua instance is already disposed."));
                Dispose();
            }
        }

        public bool HasPermission(string permissions)
        {
            return true;
        }
    }
}