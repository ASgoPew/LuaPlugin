using Newtonsoft.Json;
using NLua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyLua
{
    public static class NLuaExtensions
    {
        // TODO: Change (THIS SHIT IS FOR MARKING LUASTATE AS DISPOSED)
        public static void Enable(this Lua lua) => lua.UseTraceback = LuaEnvironment.UseTraceback;
        public static void Disable(this Lua lua) => lua.UseTraceback = !LuaEnvironment.UseTraceback;
        public static bool IsEnabled(this Lua lua) => lua.UseTraceback == LuaEnvironment.UseTraceback;
    }

    public class LuaEnvironment : IDisposable
    {
        #region Data

        public static bool UseTraceback = true;

        [JsonProperty("name")]
        public string Name;
        [JsonProperty("directories")]
        public string[] Directories;

        private Lua Lua;
        private Dictionary<string, ILuaHookHandler> HookHandlers = new Dictionary<string, ILuaHookHandler>();
        public Exception LastException = null;
        public List<ILuaCommand> LuaCommands = new List<ILuaCommand>();
        private object Locker = new object();
        public Dictionary<string, object> Data = new Dictionary<string, object>();

        public delegate void LuaExceptionD(string name, Exception e);
        public event LuaExceptionD LuaException;

        public Lua GetState() => Lua;

        #endregion

        #region Initialize

        public bool Initialize()
        {
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
                if (!Lua.IsEnabled())
                    throw new ObjectDisposedException("Trying to dispose already disposed LuaEnivronment");
                UnhookAll();
                CallFunctionByName("OnLuaClose"); // There might not be such function.
                ClearCommands();
                Lua.Dispose();
                Lua.Disable();
            }
        }

        #endregion
        #region Hooks

        public void AddHook(ILuaHookHandler hook) =>
            HookHandlers.Add(hook.Name, hook);

        public void RemoveHook(ILuaHookHandler hook) =>
            HookHandlers.Remove(hook.Name);

        public void AddEventHook(string name, Type eventType, string eventName, object eventInstance = null)
        {
            EventInfo hookEvent = eventType.GetEvent(eventName);
            MethodInfo add = hookEvent.GetAddMethod();
            MethodInfo remove = hookEvent.GetRemoveMethod();
            AddHook(new LuaHookHandler<Delegate>(this, name, (hook, state) =>
            {
                if (state == true) add.Invoke(eventInstance, new object[] { hook.Handler });
                else if (state == false) remove.Invoke(eventInstance, new object[] { hook.Handler });
                else
                {
                    ParameterInfo[] parameters = hookEvent.EventHandlerType.GetMethod("Invoke").GetParameters();
                    MethodInfo invoke = hook.GetType().GetMethods().Where(mi => mi.Name == "InvokeGeneric" && mi.GetParameters().Length == parameters.Length).First();
                    if (parameters.Length > 0)
                        hook.Handler = Delegate.CreateDelegate(hookEvent.EventHandlerType, hook,
                            invoke.MakeGenericMethod(parameters.Select(p => p.ParameterType).ToArray()));
                    else
                        hook.Handler = Delegate.CreateDelegate(hookEvent.EventHandlerType, hook, invoke);
                }
            }));
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
                if (!oldLua.IsEnabled())
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
                if (!oldLua.IsEnabled())
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
                if (!oldLua.IsEnabled())
                    return null;
                using (LuaFunction executeFunction = Get("execute") as LuaFunction)
                {
                    if (executeFunction != null)
                    {
                        result = CallFunction(executeFunction, (new object[] { script }).Concat(args).ToArray());
                    }
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
                if (!oldLua.IsEnabled())
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
                if (!oldLua.IsEnabled())
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
        #region RaiseLuaException

        public void RaiseLuaException(string name, Exception e)
        {
            LuaException(name, e);
        }

        #endregion
        #region UpdateHooks

        public void UpdateHooks()
        {
            Lua oldLua = Lua;
            lock (Locker)
            {
                if (!oldLua.IsEnabled())
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
                if (!oldLua.IsEnabled())
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
                if (!oldLua.IsEnabled())
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
                    if (!oldLua.IsEnabled())
                        return;
                    CallFunction(f, args);
                    f.Dispose(); // BUG: Can cause AccessViolationException (read notes)
                }
            });
        }

        #endregion
        #region ProcessText

        public string ProcessText(string text, string pattern, params object[] args)
        {
            int matchEndIndex = 0;
            string result = "";

            foreach (Match match in Regex.Matches(text, pattern))
            {
                result = result + text.Substring(matchEndIndex, match.Index - matchEndIndex);
                matchEndIndex = match.Index + match.Value.Length;
                string script = match.Groups[1].Value;
                object[] executionResult = RunScript("return " + script, args);
                if (executionResult != null && executionResult.Length > 0)
                    for (int i = 0; i < executionResult.Length; i++)
                        result = result + (i > 0 ? ", " : "") + (executionResult[i]?.ToString() ?? "nil");
            }
            return result + text.Substring(matchEndIndex, text.Length - matchEndIndex);
        }

        #endregion
        #region GenerateFunction

        public LuaFunction GenerateFunction(string code, string[] parameterNames = null, params object[] args)
        {
            code = $"return function({(parameterNames != null ? string.Join(",", parameterNames) : "")})" + code + ";end";
            object[] executionResult = RunScript(code, args);
            return executionResult?[0] as LuaFunction;
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
                f.Dispose();
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
                f.Dispose();
            }
        }

        #endregion
        #region SharpShow

        public string SharpShow(object o)
        {
            Type type = o.GetType();
            bool typeFlag = false;
            if (type == typeof(ProxyType))
            {
                typeFlag = true;
                type = ((ProxyType)o).UnderlyingSystemType;
            }
            string result = typeFlag ? type.Name + " CLASS:" : type.Name + " OBJECT:";

            if (type.GetConstructors().Length > 0)
                result += "\nCONSTRUCTORS:";
            foreach (var constructor in type.GetConstructors())
                if (constructor.IsPublic)
                {
                    string parameters = "";
                    var ps = constructor.GetParameters();
                    for (int i = 0; i < ps.Length; i++)
                    {
                        parameters += ps[i].ParameterType.Name + " " + ps[i].Name;
                        if (i != ps.Length - 1)
                            parameters += ", ";
                    }
                    result += constructor.IsStatic ? "\n   (static) " : "\n   ";
                    result += constructor.Name + " (" + parameters + ")";
                }

            if (type.GetMethods().Length > 0)
                result += "\nMETHODS:";
            foreach (var method in type.GetMethods())
                if (method.IsPublic)
                {
                    string parameters = "";
                    var ps = method.GetParameters();
                    for (int i = 0; i < ps.Length; i++)
                    {
                        parameters += ps[i].ParameterType.Name + " " + ps[i].Name;
                        if (i != ps.Length - 1)
                            parameters += ", ";
                    }
                    result += method.IsStatic ? "\n   (static) " : "\n   ";
                    result += method.ReturnType.Name + new string(' ', 10 - (method.ReturnType.Name.Length % 10)) + method.Name + " (" + parameters + ")";
                }

            if (type.GetEvents().Length > 0)
                result += "\nEVENTS:";
            foreach (var e in type.GetEvents())
                result += "\n   " + e.Name;

            if (type.GetFields().Length > 0)
                result += "\nFIELDS:";
            if (typeFlag)
            {
                foreach (var field in type.GetFields())
                    if (field.IsPublic)
                    {
                        if (field.IsStatic)
                            result += "\n   (static) " + field.Name + ": " + new string(' ', 30 - (field.Name.Length % 30)) + field.GetValue(null) ?? "null";
                        else
                            result += "\n   " + field.Name;
                    }
            }
            else
                foreach (var field in type.GetFields())
                    if (field.IsPublic)
                        result += "\n   " + field.Name + ": " + new string(' ', 30 - (field.Name.Length % 30)) + field.GetValue(o) ?? "null";
            return result;
        }

        #endregion
    }
}
