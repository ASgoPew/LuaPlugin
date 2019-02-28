using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace LuaPlugin
{
    public class LuaCommand
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
                luaEnv.PrintError("LuaCommand.LuaCommand(LuaEnvironment luaEnv, object names_object, object permission_object, LuaTable parameters, LuaFunction f): Invalid parameters");
                return;
            }
            List<string> permissions = new List<string>();
            if (permissionObject.GetType() == typeof(LuaTable))
            {
                LuaTable t = permissionObject as LuaTable;
                foreach (var o in t)
                    permissions.Add((string)((KeyValuePair < Object, Object >)o).Value);
            }
            else if (permissionObject.GetType() == typeof(string))
                permissions.Add((string)permissionObject);
            else
            {
                luaEnv.PrintError("LuaCommand.LuaCommand(LuaEnvironment luaEnv, object names_object, object permission_object, LuaTable parameters, LuaFunction f): Invalid parameters");
                return;
            }
            bool allowServer = (bool)(parameters["AllowServer"] ?? false);
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

        public void Dispose(bool removeFromList = true)
        {
            if (Disposed)
                return;
            Disposed = true;

            Commands.ChatCommands.Remove(Cmd);
            if (removeFromList)
                LuaEnv.LuaCommands.Remove(this);
            if (!Lua.UseTraceback) // WILL THIS CRASH?
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
                LuaEnv.PrintError("LuaCommand " + Cmd.Name + " is already disposed but trying to invoke it.");
                return;
            }
            if (!Lua.UseTraceback)
            {
                object oldSource = LuaEnv.Data["source"];
                LuaEnv.Data["sourse"] = this;
                LuaEnv.CallFunction(Function, null, "LuaCommand.Invoke", args);
                LuaEnv.Data["source"] = oldSource;
            }
            else
            {
                LuaEnv.PrintError("Trying to invoke LuaCommand " + Cmd.Name + " while corresponding lua instance is already disposed.");
                Dispose();
            }
        }

        public bool HasPermission(string permissions)
        {
            return true;
        }
    }
}
