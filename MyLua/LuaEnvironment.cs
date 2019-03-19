using NLua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyLua
{
    public static class NLuaExtensions
    {
        // TODO: Change (THIS SHIT IS FOR MARKING LUASTATE AS DISPOSED)
        public static void Enable(this Lua lua)  => lua.UseTraceback = true;
        public static void Disable(this Lua lua) => lua.UseTraceback = false;
        public static bool Enabled(this Lua lua) => lua.UseTraceback;
    }

    public class LuaEnvironment : IDisposable
    {
        #region Data

        private Lua Lua;
        private string[] Directories;
        private Dictionary<string, ILuaHookHandler> HookHandlers = new Dictionary<string, ILuaHookHandler>();
        public Exception LastException = null;
        public List<ILuaCommand> LuaCommands = new List<ILuaCommand>();
        private object Locker = new object();
        public Dictionary<string, object> Data = new Dictionary<string, object>();

        public void AddHook(ILuaHookHandler hook) =>
            HookHandlers.Add(hook.Name, hook);

        #endregion

        #region Initialize

        public LuaEnvironment(string[] directories)
        {
            Directories = directories;
        }

        public bool Initialize()
        {
            lock (Locker)
            {
                if (Lua != null)
                {
                    UnhookAll(); // Here enabled is already true since lua != null
                    Dispose();
                }
            }

            Lua = new Lua() { UseTraceback = true };
            Lua.State.Encoding = Encoding.UTF8;
            Lua.LoadCLRPackage();
            Lua["env"] = this;

            if (!ReadEnvironment())
                return false;

            CallFunctionByName("OnLuaInit"); // There might not be such function.
            UpdateHooks();

            return true;
        }

        #endregion
        #region Dispose

        public void Dispose()
        {
            lock (Locker)
            {
                if (!Lua.Enabled())
                    return;
                CallFunctionByName("OnLuaClose"); // There might not be such function.
                ClearCommands();
                Lua.Dispose();
                Lua.Disable();
            }
        }

        #endregion
        #region ClearCommands

        public void ClearCommands()
        {
            foreach (var c in LuaCommands)
                c.Dispose();
            LuaCommands.Clear();
        }

        #endregion
        #region ReadEnvironment

        public bool ReadEnvironment()
        {
            List<string> scripts = new List<string>();
            foreach (string dir in Directories)
                scripts.AddRange(Directory.EnumerateFiles(dir, "*.lua", SearchOption.AllDirectories));

            scripts.Sort(delegate (string script1, string script2)
            {
                if (String.Compare(Path.GetFileNameWithoutExtension(script1), Path.GetFileNameWithoutExtension(script2)) < 0)
                    return -1;
                else if (String.Compare(Path.GetFileNameWithoutExtension(script1), Path.GetFileNameWithoutExtension(script2)) > 0)
                    return 1;
                return 0;
            });

            foreach (string script in scripts)
            {
                string filename = script.Replace(@"\", @"/");
                Console.WriteLine("\t" + filename);
                if (RunScript($"dofile('{filename}', '{filename}')", null, $"ReadLuaEnvironment", true) == null)
                    return false;
            }
            return true;
        }

        #endregion
        #region Get

        public object Get(string name)
        {
            Lua oldLua = Lua;
            lock (Locker)
            {
                if (!oldLua.Enabled())
                    return null;
                return Lua[name];
            }
        }

        #endregion
        #region Set

        public void Set(string name, object o = null)
        {
            Lua oldLua = Lua;
            lock (Locker)
            {
                if (!oldLua.Enabled())
                    return;
                Lua[name] = o;
            }
        }

        #endregion
        #region GenerateDotNETException

        public void GenerateDotNETException()
        {
            throw new Exception("Template lua exception");
        }

        #endregion
        #region RunScript

        public object[] RunScript(string script, params object[] args)
        {
            
            object[] result = null;
            Lua oldLua = Lua;
            lock (Locker)
            {
                if (!oldLua.Enabled())
                    return null;
                using (LuaFunction executeFunction = Get("execute") as LuaFunction)
                {
                    if (executeFunction != null)
                        result = CallFunction(executeFunction, script, args);
                    else
                    {
                        result = Lua.DoString(script);
                        if (result == null)
                            result = new object[0];
                    }
                }
            }
            return result;
        }

        #endregion
        #region CallFunction

        public object[] CallFunction(LuaFunction f, params object[] args)
        {
            Lua oldLua = Lua;
            lock (Locker)
            {
                if (!oldLua.Enabled())
                    return null;
                return f.Call(args) ?? new object[0];
            }
        }

        #endregion
        #region CallFunctionByName

        public object[] CallFunctionByName(string name, params object[] args)
        {
            Lua oldLua = Lua;
            lock (Locker)
            {
                if (!oldLua.Enabled())
                    return null;
                using (LuaFunction f = Get(name) as LuaFunction)
                {
                    if (f != null)
                        return f.Call(args) ?? new object[0];
                    else
                        return null;
                }
            }
        }

        #endregion
        #region UpdateHooks

        public void UpdateHooks()
        {
            Lua oldLua = Lua;
            lock (Locker)
            {
                if (!oldLua.Enabled())
                    return;
                foreach (ILuaHookHandler handler in HookHandlers.Values) // TODO: Lazy hook handler creation
                    handler.Update();
            }
        }

        #endregion
        #region UpdateHook

        public void UpdateHook(string name)
        {
            Lua oldLua = Lua;
            lock (Locker)
            {
                if (!oldLua.Enabled())
                    return;
                HookHandlers[name].Update();
            }
        }

        #endregion
        #region Unhook

        public void Unhook(string name)
        {
            Lua oldLua = Lua;
            lock (Locker)
            {
                if (!oldLua.Enabled())
                    return;
                if (HookHandlers[name].Active)
                    HookHandlers[name].Disable();
                Lua[name] = null;
            }
        }

        #endregion
        #region UnhookAll

        public void UnhookAll()
        {
            foreach (var pair in HookHandlers)
                if (pair.Value.Active)
                    pair.Value.Disable();
        }

        #endregion
        #region LuaDelay

        // DOES NOT ABORT INFINITE LOOPS
        public void LuaDelay(int milliseconds, LuaFunction f, params object[] args)
        {
            Lua oldLua = Lua;
            Task.Delay(milliseconds).ContinueWith(_ =>
            {
                lock (Locker)
                {
                    if (!oldLua.Enabled())
                        return;
                    CallFunction(f, args);
                    f.Dispose(); // BUG: Can cause AccessViolationException (read notes)
                }
            });
        }

        #endregion
        #region ResetFromLua

        public void ResetFromLua()
        {
            // Delay for 1 millisecond to run this code in another thread
            Task.Delay(1).ContinueWith(_ => Initialize());
        }

        #endregion
        #region NativeLuaWrite

        public void NativeLuaWrite(byte[] buffer, int offset, int size, LuaFunction f)
        {
            using (MemoryStream ms = new MemoryStream(buffer, offset, size))
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                f.Call(ms, bw);
            }
        }

        #endregion
        #region NativeLuaRead

        public void NativeLuaRead(byte[] buffer, int offset, int size, LuaFunction f)
        {
            using (MemoryStream ms = new MemoryStream(buffer, offset, size))
            using (BinaryReader br = new BinaryReader(ms))
            {
                f.Call(ms, br);
            }
        }

        #endregion
    }
}
