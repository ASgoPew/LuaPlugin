using Microsoft.Xna.Framework;
using NLua;
using NLua.Event;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
//using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Terraria.Localization;
using TerrariaApi.Server;
using TShockAPI;

namespace MyLua
{
    public class LuaEnvironment : IDisposable
    {
        public static int LastExceptionLuaIndex = -1;

        public int Index;
        private Lua Lua = null;
        private Dictionary<string, ILuaHookHandler> Handlers = new Dictionary<string, ILuaHookHandler>();
        public string LuaDirectory = null;
        public Exception LastException = null;
        public List<ILuaCommand> LuaCommands = new List<ILuaCommand>();
        private object Locker = new object();
        public Dictionary<string, object> Data = new Dictionary<string, object>();
        public bool Enabled = false;
        public bool ForceStopped = false;
        public bool PrintErrorStarted = false;

        public LuaEnvironment(int index, string directory = null)
        {
            this.Index = index;
            this.LuaDirectory = directory;
            //foreach (string name in LuaHookHandler.HandlerNames) // TODO: Add handlers only on hook registration?
                //Handlers.Add(name, new LuaHookHandler(this, name));
        }

        public void Dispose()
        {
            lock (Locker)
            {
                if (Lua.UseTraceback)
                    return;
                CallFunction("OnLuaClose", null, "Dispose");
                ClearCommands();
                Lua.Dispose();
                Lua.UseTraceback = true; // TODO: Change (THIS SHIT IS FOR MARKING LUASTATE AS DISPOSED)
                //lua = null;
            }
        }

        public void ClearCommands()
        {
            foreach (var c in LuaCommands)
                c.Dispose();
            LuaCommands.Clear();
        }

        public Lua GetState()
        {
            return Lua;
        }

        public bool Initialize(bool luaInit = false)
        {
            try
            {
                lock (Locker)
                {
                    if (Lua != null)
                    {
                        UnhookAll(); // Here enabled is already true since lua != null
                        Dispose();
                    }
                    Lua = new Lua();
                    Lua.State.Encoding = Encoding.UTF8;
                    Lua.LoadCLRPackage();

                    Data["source"] = this;

                    Lua["env"] = this;

                    //Lua.RegisterFunction("NetworkText", typeof(NetworkText).GetMethod("FromLiteral")).Dispose();
                }
            } catch (Exception e)
            {
                HandleException(e, "Initialize");
                return false;
            }
            Enabled = true;

            if (!ReadLuaEnvironment())
                return false;

            if (luaInit)
                LuaInit();

            UpdateHooks();

            return true;
        }

        public void LuaInit()
        {
            CallFunction("OnLuaInit", null, "LuaInit"); // There might not be such function. It's not an error.
            UpdateHooks();
        }

        public bool ReadLuaEnvironment(string path, string key)
        {
            List<string> scripts = Directory.EnumerateFiles(Path.Combine(path, key), "*.lua", SearchOption.TopDirectoryOnly).ToList();
            if (LuaDirectory != null)
                scripts = scripts.Concat(Directory.EnumerateFiles(LuaDirectory, "*.lua", SearchOption.AllDirectories).ToList()).ToList();
            scripts.Sort(delegate (string script1, string script2)
            {
                if (String.Compare(Path.GetFileNameWithoutExtension(script1), Path.GetFileNameWithoutExtension(script2)) < 0)
                    return -1;
                else if (String.Compare(Path.GetFileNameWithoutExtension(script1), Path.GetFileNameWithoutExtension(script2)) > 0)
                    return 1;
                return 0;
            });
            Console.WriteLine("Reading environment: " + Index);
            foreach (string script in scripts)
            {
                string filename = script.Replace(@"\", @"/");
                Console.WriteLine("\t" + filename);
                if (Execute($"dofile('{filename}', '{filename}')", null, $"ReadLuaEnvironment", true) == null)
                    return false;
            }
            return true;
        }

        public object Get(string name, string exceptionMessage = "")
        {
            if (!Enabled)
                return null;
            try
            {
                Lua oldLua = Lua;
                lock (Locker)
                {
                    if (oldLua.UseTraceback)
                        return false;
                    return Lua[name];
                }
            } catch
            {
                HandleException(new Exception("Cannot Get " + name + " member"), "Get: " + exceptionMessage);
                return null;
            }
        }

        public void Set(string name, object o, string exceptionMessage = "")
        {
            if (!Enabled)
                return;
            if (o == null)
            {
                PrintError("LuaEnvironment.Set: o is null! " + exceptionMessage);
                throw new Exception("meh");
            }
            try
            {
                Lua oldLua = Lua;
                lock (Locker)
                {
                    if (oldLua.UseTraceback)
                        return;
                    Lua[name] = o;
                }
            }
            catch
            {
                HandleException(new Exception("Cannot Set " + name + " member"), "Set: " + exceptionMessage);
                return;
            }
        }

        public void SetNull(string name, string exceptionMessage = "")
        {
            if (!Enabled)
                return;
            try
            {
                Lua oldLua = Lua;
                lock (Locker)
                {
                    if (oldLua.UseTraceback)
                        return;
                    Lua[name] = null;
                }
            }
            catch
            {
                HandleException(new Exception("Cannot SetNull " + name + " member"), "Set: " + exceptionMessage);
                return;
            }
        }

        public void GenerateException()
        {
            throw new Exception("Template lua exception");
        }

        public object[] Execute(string script, Action<Lua, int> update, string exceptionMessage, bool trusted = false, object arg = null)
        {
            if (!Enabled)
                return null;
            if (script == null)
            {
                PrintError("Execute: first argument (script) is null");
                return null;
            }

            object[] result = null;
            Lua oldLua = Lua;
            lock (Locker)
            {
                if (oldLua.UseTraceback)
                    return null;
                try
                {
                    update?.Invoke(Lua, 0);
                    using (LuaFunction executeFunction = Get("execute") as LuaFunction)
                    {
                        if (!trusted && executeFunction != null)
                            result = CallFunction(executeFunction, update, "Execute", script, arg);
                        else
                        {
                            result = Lua.DoString(script);
                            if (result == null)
                                result = new object[0];
                        }
                    }
                    update?.Invoke(Lua, 1);
                }
                catch (Exception e)
                {
                    update?.Invoke(Lua, 2);
                    HandleException(e, $"Execute: " + exceptionMessage);
                }
            }
            return result;
        }

        public object[] CallFunction(LuaFunction f, Action<Lua, int> update = null, string exceptionMessage = "", object arg0 = null, object arg1 = null)
        {
            if (!Enabled)
                return null;
            if (f == null)
            {
                PrintError("CallFunction: first argument (LuaFunction f) is null");
                return null;
            }

            object[] result = null;
            Lua oldLua = Lua;
            lock (Locker)
            {
                if (oldLua.UseTraceback)
                    return null;
                try
                {
                    update?.Invoke(Lua, 0);
                    result = f.Call(arg0, arg1);
                    if (result == null)
                        result = new object[0];
                    update?.Invoke(Lua, 1);
                }
                catch (Exception e)
                {
                    update?.Invoke(Lua, 2);
                    HandleException(e, $"CallFunction (no name): " + exceptionMessage + "\n" + e.InnerException);
                }
            }
            return result;
        }

        public object[] CallFunction(string name, Action<Lua, int> update = null, string exceptionMessage = "", object arg0 = null, object arg1 = null)
        {
            object[] result = null;
            if (!Enabled)
                return result;
            Lua oldLua = Lua;
            lock (Locker)
            {
                if (oldLua.UseTraceback)
                    return result;
                using (LuaFunction f = Get(name) as LuaFunction)
                {
                    if (f != null)
                        return CallFunction(f, update, exceptionMessage, arg0, arg1);
                    else
                        return result;
                }
            }
        }

        public string ActiveHooks()
        {
            Lua oldLua = Lua;
            lock (Locker)
            {
                if (oldLua.UseTraceback)
                    return "LUA INSTANCE IS ALREADY DISPOSED!";
                string result = "Active hook list:";
                foreach (ILuaHookHandler handler in Handlers.Values)
                    if (handler.Active)
                        result += "\n\t" + handler.Name;
                return result;
            }
        }

        public void UpdateHooks()
        {
            Lua oldLua = Lua;
            lock (Locker)
            {
                if (oldLua.UseTraceback)
                    return;
                foreach (ILuaHookHandler handler in Handlers.Values) // TODO: Lazy hook handler creation
                    handler.Update();
            }
        }

        public void UpdateHook(string name)
        {
            Lua oldLua = Lua;
            lock (Locker)
            {
                if (oldLua.UseTraceback)
                    return;
                Handlers[name].Update();
            }
        }

        public void HookAll()
        {
            Lua oldLua = Lua;
            lock (Locker)
            {
                if (oldLua.UseTraceback)
                    return;
                foreach (var pair in Handlers)
                    if (!pair.Value.Active && pair.Key != "OnSendBytes" && pair.Key != "OnSendData") // TODO: check pair.Key for validance
                    {
                        Lua.DoString("function " + pair.Key + "() print('" + pair.Key + "') end");
                        pair.Value.Hook();
                    }
            }
        }

        public void Unhook(string name)
        {
            Lua oldLua = Lua;
            lock (Locker)
            {
                if (oldLua.UseTraceback)
                    return;
                if (Handlers[name].Active)
                    Handlers[name].Unhook();
                SetNull(name, $"Unhook ({name})");
            }
        }

        public void UnhookAll()
        {
            Lua oldLua = Lua;
            lock (Locker)
            {
                if (oldLua.UseTraceback)
                    return;
                foreach (var pair in Handlers)
                    if (pair.Value.Active)
                    {
                        pair.Value.Unhook();
                        SetNull(pair.Key, "UnhookAll");
                    }
            }
        }

        // DOES NOT ABORT INFINITE LOOPS
        public void LuaTaskDelay(int milliseconds, LuaFunction f)
        {
            Lua oldLua = Lua;
            object tmp = Data["source"];
            Task.Delay(milliseconds).ContinueWith(_ =>
            {
                lock (Locker)
                {
                    if (!oldLua.UseTraceback) // IF LUASTATE ISN'T MARKED AS DISPOSED
                    {
                        object oldSource = Data["source"];
                        CallFunction(f, (lua, state) => {
                            if (state == 0)
                                Data["source"] = tmp;
                            else if (state > 0)
                                Data["source"] = oldSource;
                        }, "LuaTaskDelay");
                        f.Dispose(); // BUG: Can cause AccessViolationException (read notes)
                    }
                }
            });
        }

        public void LuaReset()
        {
            // Delay for 1 millisecond to run this code in another thread
            Task.Delay(1).ContinueWith(_ =>
            {
                if (Initialize(true))
                    LuaPlugin.Me.SendSuccessMessage($"Lua[{Index}] has been reset.");
                else
                    LuaPlugin.Me.SendErrorMessage($"Lua[{Index}] reset failed.");
            });
        }

        public string ProcessText(TSPlayer player, string text)
        {
            string pattern = @"{;.*?;}";
            int matchEndIndex = 0;
            string result = "";

            foreach (Match match in Regex.Matches(text, pattern))
            {
                result = result + text.Substring(matchEndIndex, match.Index - matchEndIndex);
                matchEndIndex = match.Index + match.Value.Length;
                string script = match.Value.Substring(2, match.Value.Length - 4);
                object oldSource = Data["source"];
                object[] executionResult = Execute("return " + script, (_lua, state) => {
                    if (state == 0)
                    {
                        LuaPlugin.Me = player;
                        Data["source"] = player;
                    }
                    else if (state > 0)
                        Data["source"] = oldSource;
                }, "ProcessText", false, true);
                if (executionResult != null && executionResult.Length > 0)
                    for (int i = 0; i < executionResult.Length; i++)
                    {
                        if (executionResult[i] != null)
                            result = result + (i > 0 ? ", " : "") + executionResult[i].ToString();
                        else
                            result = result + (i > 0 ? ", " : "") + "nil";
                    }
            }
            return result + text.Substring(matchEndIndex, text.Length - matchEndIndex);
        }

        public void NativeLuaWrite(byte[] buffer, int index, LuaFunction f, object arg0 = null, object arg1 = null)
        {
            if (index < 0 || index >= buffer.Length)
            {
                PrintError("LuaWrite exception: Index out of buffer");
                return;
            }
            using (MemoryStream ms = new MemoryStream(buffer, index, buffer.Length - index))
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                f.Call(bw, arg0, arg1);
            }
        }

        public void NativeLuaRead(byte[] buffer, int index, LuaFunction f, object arg0 = null, object arg1 = null)
        {
            if (index < 0 || index >= buffer.Length)
            {
                PrintError("LuaRead exception: Index out of buffer");
                return;
            }
            using (MemoryStream ms = new MemoryStream(buffer, index, buffer.Length - index))
            using (BinaryReader br = new BinaryReader(ms))
            {
                f.Call(br, arg0, arg1);
            }
        }

        public void LuaWriteSendBytes(SendBytesEventArgs args, PacketTypes type, int index, LuaFunction f)
        {
            if ((byte)type == args.Buffer[args.Offset + 2])
                NativeLuaWrite(args.Buffer, args.Offset + 3 + index, f, args.Count, args.Socket.Id);
            f.Dispose();
        }

        public void LuaReadSendBytes(SendBytesEventArgs args, PacketTypes type, int index, LuaFunction f)
        {
            if ((byte)type == args.Buffer[args.Offset + 2])
                NativeLuaRead(args.Buffer, args.Offset + 3 + index, f, args.Count, args.Socket.Id);
            f.Dispose();
        }

        public void LuaWriteGetData(GetDataEventArgs args, PacketTypes type, int index, LuaFunction f)
        {
            if (args.MsgID == type)
                NativeLuaWrite(args.Msg.readBuffer, args.Index + index, f, args.Length + 2, args.Msg.whoAmI);
            f.Dispose();
        }

        public void LuaReadGetData(GetDataEventArgs args, PacketTypes type, int index, LuaFunction f)
        {
            if (args.MsgID == type)
                NativeLuaRead(args.Msg.readBuffer, args.Index + index, f, args.Length + 2, args.Msg.whoAmI);
            f.Dispose();
        }

        public LuaFunction GenerateFunction(string code, string[] names = null)
        {
            code = $"return function({(names != null ? string.Join(",", names) : "")})" + code + ";end";
            object oldSource = Data["source"];
            object[] executionResult = Execute(code, null, "GenerateFunction", false, true);
            return executionResult?[0] as LuaFunction;
        }

        public void HandleException(Exception e, string exceptionMessage = "")
        {
            LastException = e;
            LastExceptionLuaIndex = Index;
            PrintError($"{exceptionMessage}\n{e}");//\n{GetDebugTraceback()}");
        }

        public void PrintError(string errorMessage)
        {
            if (PrintErrorStarted)
            {
                Console.Error.WriteLine($"Lua[{Index}]: PrintError already started (ALTHOUGH: {errorMessage})");
                return;
            }
            PrintErrorStarted = true;

            errorMessage = $"Lua[{Index}]: " + errorMessage;
            if (CallFunction("perror", null, "PrintError", errorMessage) == null)
                Console.Error.WriteLine(errorMessage);

            PrintErrorStarted = false;
        }

        public void ShowLastException()
        {
            if (LuaEnvironment.LastExceptionLuaIndex >= 0)
            {
                Exception e = LastException;
                PrintError($"\n> Message: {e.Message}\n> Data: {e.Data}\n> InnerException: {e.InnerException}\n" +
                    $"> StackTrace: {e.StackTrace}\n> Source: {e.Source}");
            }
            else
                PrintError("No last exception found!");
        }
    }
}
