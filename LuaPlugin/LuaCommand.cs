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
        private bool disposed = false;
        public LuaFunction f;
        public LuaEnvironment luaEnv;
        public Lua lua;
        public Command cmd;

        public LuaCommand(LuaEnvironment luaEnv, object names_object, object permission_object, LuaTable parameters, LuaFunction f)
        {
            this.f = f;
            this.luaEnv = luaEnv;
            this.lua = luaEnv.GetState(); // TODO: Changing f might crash on Dispose, since new f can have different interpreter
            string[] names;
            if (names_object.GetType() == typeof(LuaTable))
            {
                LuaTable t = names_object as LuaTable;
                names = new string[t.Keys.Count];
                int i = 0;
                foreach (var o in t)
                {
                    names[i++] = (string)(((KeyValuePair<Object, Object>)o).Value);
                }
            } else if (names_object.GetType() == typeof(string))
            {
                names = new string[1] { (string)names_object };
            } else
            {
                luaEnv.PrintError("LuaCommand.LuaCommand(LuaEnvironment luaEnv, object names_object, object permission_object, LuaTable parameters, LuaFunction f): Invalid parameters");
                return;
            }
            List<string> permissions = new List<string>();
            if (permission_object.GetType() == typeof(LuaTable))
            {
                LuaTable t = permission_object as LuaTable;
                foreach (var o in t)
                {
                    permissions.Add((string)((KeyValuePair < Object, Object >)o).Value);
                }
            }
            else if (permission_object.GetType() == typeof(string))
            {
                permissions.Add((string)permission_object);
            }
            else
            {
                luaEnv.PrintError("LuaCommand.LuaCommand(LuaEnvironment luaEnv, object names_object, object permission_object, LuaTable parameters, LuaFunction f): Invalid parameters");
                return;
            }
            bool allowServer = (bool)(parameters["AllowServer"] ?? false);
            string helpText = (string)(parameters["HelpText"] ?? "Temporarily command");
            bool doLog = (bool)(parameters["DoLog"] ?? false);
            this.cmd = new Command(permissions, Invoke, names)
            {
                AllowServer = allowServer,
                HelpText = helpText,
                DoLog = doLog
            };
            Commands.ChatCommands.Add(cmd);
            luaEnv.luaCommands.Add(this);
        }

        public void Dispose(bool removeFromList = true)
        {
            if (disposed)
                return;
            disposed = true;

            Commands.ChatCommands.Remove(cmd);
            if (removeFromList)
                luaEnv.luaCommands.Remove(this);
            if (!lua.UseTraceback) // WILL THIS CRASH????????????????????????????????????????????????????
                f.Dispose();
            f = null;
            luaEnv = null;
            lua = null;
            cmd = null;
        }

        public void Invoke(CommandArgs args)
        {
            if (disposed)
            {
                luaEnv.PrintError("LuaCommand " + cmd.Name + " is already disposed but trying to invoke it.");
                return;
            }
            if (!lua.UseTraceback)
            {
                object oldSource = luaEnv.data["source"];
                luaEnv.data["sourse"] = this;
                luaEnv.CallFunction(f, null, "LuaCommand.Invoke", args);
                luaEnv.data["source"] = oldSource;
            }
            else
            {
                luaEnv.PrintError("Trying to invoke LuaCommand " + cmd.Name + " while corresponding lua instance is already disposed.");
                Dispose();
            }
        }

        public bool HasPermission(string permissions)
        {
            return true;
        }
    }
}
