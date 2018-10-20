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

namespace LuaPlugin
{
    public class LuaEnvironment : IDisposable
    {
        public static int lastExceptionLuaIndex = -1;

        public int index;
        private Lua lua = null;
        private Dictionary<string, LuaHookHandler> handlers = new Dictionary<string, LuaHookHandler>();
        public string directory = null;
        public Exception lastException = null;
        public List<LuaCommand> luaCommands = new List<LuaCommand>();
        private object locker = new object();
        public Dictionary<string, object> data = new Dictionary<string, object>();
        public bool enabled = false;
        public bool forceStopped = false;
        public bool printErrorStarted = false;

        public LuaEnvironment(int index, string directory = null)
        {
            this.index = index;
            this.directory = directory;
            foreach (string name in LuaHookHandler.HandlerNames) // TODO: Add handlers only on hook registration?
                handlers.Add(name, new LuaHookHandler(this, name));
        }

        public void Dispose()
        {
            lock (locker)
            {
                if (lua.UseTraceback)
                {
                    TShock.Log.ConsoleError("TRYING TO DISPOSE ALREADY DISPOSED LUAENVIRONMENT!");
                    return;
                }
                CallFunction("OnLuaClose", null, "Initialize");
                ClearCommands();
                lua.Dispose();
                lua.UseTraceback = true; // TODO: Change (THIS SHIT IS FOR MARKING LUASTATE AS DISPOSED)
                //lua = null;
            }
        }

        public void ClearCommands()
        {
            foreach (var c in luaCommands)
                c.Dispose(false);
            luaCommands.Clear();
        }

        public Lua GetState()
        {
            return lua;
        }

        public bool HasPermission(string perm)
        {
            return true;
        }

        public bool Initialize(TSPlayer me = null, bool luaInit = false)
        {
            try
            {
                lock (locker)
                {
                    if (lua != null)
                    {
                        UnhookAll(); // Here enabled is already true since lua != null
                        Dispose();
                    }
                    lua = new Lua();
                    lua.LoadCLRPackage();

                    data["source"] = this;

                    lua["env"] = this;
                    lua["plugin"] = LuaPlugin.instance;

                    //lua.RegisterLuaClassType(typeof(Microsoft.Xna.Framework.Color), typeof(Microsoft.Xna.Framework.Color));

                    //lua.RegisterFunction("writesb", typeof(LuaHelper).GetMethod("LuaWriteSendBytes")).Dispose();
                    //lua.RegisterFunction("readsb", typeof(LuaHelper).GetMethod("LuaReadSendBytes")).Dispose();
                    //lua.RegisterFunction("writegd", typeof(LuaHelper).GetMethod("LuaWriteGetData")).Dispose();
                    //lua.RegisterFunction("readgd", typeof(LuaHelper).GetMethod("LuaReadGetData")).Dispose();

                    //lua.RegisterFunction("WriteLine", typeof(Console).GetMethod("WriteLine")); // Ambigous match found
                    //lua.RegisterFunction("LuaCommand", typeof(LuaCommand).GetConstructor(new Type[] { typeof(LuaEnvironment), typeof(object), typeof(object), typeof(LuaTable), typeof(LuaFunction) })).Dispose();
                    //lua.RegisterFunction("lock", GetType().GetMethod("LuaLock"));
                    //lua.RegisterFunction("GetRAMUsage", typeof(LuaHelper).GetMethod("GetRAMUsage")).Dispose();
                    //lua.RegisterFunction("packetstr", typeof(LuaHelper).GetMethod("PacketString")).Dispose();
                    //lua.RegisterFunction("SharpShow", typeof(LuaHelper).GetMethod("SharpShow")).Dispose();
                    lua.RegisterFunction("NetworkText", typeof(NetworkText).GetMethod("FromLiteral")).Dispose();
                    //lua.RegisterFunction("e", typeof(LuaEnvironment).GetMethod("ShowLastException")).Dispose();
                    //lua.RegisterFunction("Test", typeof(LuaPlugin).GetMethod("Test")).Dispose();
                    // TODO: RegisterClass LuaHelper insdead of ^
                }
            } catch (Exception e)
            {
                HandleException(e, "Initialize");
                return false;
            }
            enabled = true;

            //if (me != null)
                //Set("me", me);
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

        public bool ReadLuaEnvironment()
        {
            List<string> scripts = Directory.EnumerateFiles(Path.Combine(Directory.GetCurrentDirectory(), LuaPlugin.scriptsDirectory), "*.lua", SearchOption.TopDirectoryOnly).ToList();
            if (directory != null)
                scripts = scripts.Concat(Directory.EnumerateFiles(directory, "*.lua", SearchOption.AllDirectories).ToList()).ToList();
            scripts.Sort(delegate (string script1, string script2)
            {
                if (String.Compare(Path.GetFileNameWithoutExtension(script1), Path.GetFileNameWithoutExtension(script2)) < 0)
                    return -1;
                else if (String.Compare(Path.GetFileNameWithoutExtension(script1), Path.GetFileNameWithoutExtension(script2)) > 0)
                    return 1;
                return 0;
            });
            /*List<FileInfo> scripts = (new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), LuaPlugin.scriptsDirectory)).GetFiles("*.lua").ToList());
            if (directory != null)
                scripts = scripts.Concat(new DirectoryInfo(directory).GetFiles("*.lua").ToList()).ToList();
            scripts.Sort(delegate (FileInfo script1, FileInfo script2)
            {
                if (String.Compare(script1.Name, script2.Name) < 0)
                    return -1;
                else if (String.Compare(script1.Name, script2.Name) > 0)
                    return 1;
                return 0;
            });*/
            Console.WriteLine("Reading environment: " + index);
            foreach (string script in scripts)
            {
                string filename = script.Replace(@"\", @"/");
                Console.WriteLine("\t" + filename);
                //if (!Execute(File.ReadAllText(script.FullName), null, $"ReadLuaEnvironment (in script {script.Name})", out result))
                //Console.WriteLine(filename);
                if (Execute($"dofile('{filename}', '{filename}')", null, $"ReadLuaEnvironment", true) == null)
                    return false;
            }
            return true;
        }

        //public void Update(Action<Lua> update)
        //{
        //    lock (locker)
        //    {
        //        if (lua == null)
        //            return false;
        //        update?.Invoke(lua);
        //    }
        //}

        public object Get(string name, string exceptionMessage = "")
        {
            if (!enabled)
                return null;
            try
            {
                Lua oldLua = lua;
                lock (locker)
                {
                    if (oldLua.UseTraceback)
                        return false;
                    return lua[name];
                }
            } catch
            {
                HandleException(new Exception("Cannot Get " + name + " member"), "Get: " + exceptionMessage);
                return null;
            }
        }

        public void Set(string name, object o, string exceptionMessage = "")
        {
            if (!enabled)
                return;
            if (o == null)
            {
                PrintError("LuaEnvironment.Set: o is null! " + exceptionMessage);
                throw new Exception("meh");
            }
            try
            {
                Lua oldLua = lua;
                lock (locker)
                {
                    if (oldLua.UseTraceback)
                        return;
                    lua[name] = o;
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
            if (!enabled)
                return;
            try
            {
                Lua oldLua = lua;
                lock (locker)
                {
                    if (oldLua.UseTraceback)
                        return;
                    lua[name] = null;
                }
            }
            catch
            {
                HandleException(new Exception("Cannot SetNull " + name + " member"), "Set: " + exceptionMessage);
                return;
            }
        }

        /*public void UpdateLua(TSPlayer player)
        {
            try
            {
                Lua oldLua = lua;
                lock (locker)
                {
                    if (oldLua.useTraceback)
                        return;
                    lua["me"] = player;
                    lua["defaultColor"] = defaultColor;
                    //lua.DoString("debug.sethook(function() error('Lua script instructions amount has exceeded allowed value.') end, '', " + maxInstructionsCount.ToString() + ")");
                }
            }
            catch (Exception e)
            {
                player.SendErrorMessage("Cannot update lua: " + e.Message);
            }
        }*/

        public void GenerateException()
        {
            throw new Exception("Template lua exception");
        }

        public object[] Execute(string script, Action<Lua, int> update, string exceptionMessage, bool trusted = false, object arg = null)
        {
            if (!enabled)
                return null;
            if (script == null)
            {
                PrintError("Execute: first argument (script) is null");
                return null;
            }

            object[] result = null;
            Lua oldLua = lua;
            lock (locker)
            {
                if (oldLua.UseTraceback)
                    return null;
                try
                {
                    update?.Invoke(lua, 0);
                    using (LuaFunction executeFunction = Get("execute") as LuaFunction)
                    {
                        if (!trusted && executeFunction != null)
                            result = CallFunction(executeFunction, update, "Execute", script, arg);
                        else
                        {
                            result = lua.DoString(script);
                            if (result == null)
                                result = new object[0];
                        }
                    }
                    update?.Invoke(lua, 1);
                }
                catch (Exception e)
                {
                    update?.Invoke(lua, 2);
                    HandleException(e, $"Execute: " + exceptionMessage);
                }
            }
            return result;
        }

        public object[] CallFunction(LuaFunction f, Action<Lua, int> update = null, string exceptionMessage = "", object arg0 = null, object arg1 = null)
        {
            if (!enabled)
                return null;
            if (f == null)
            {
                PrintError("CallFunction: first argument (LuaFunction f) is null");
                return null;
            }

            object[] result = null;
            Lua oldLua = lua;
            lock (locker)
            {
                if (oldLua.UseTraceback)
                    return null;
                try
                {
                    update?.Invoke(lua, 0);
                    result = f.Call(arg0, arg1);
                    if (result == null)
                        result = new object[0];
                    update?.Invoke(lua, 1);
                }
                catch (Exception e)
                {
                    update?.Invoke(lua, 2);
                    HandleException(e, $"CallFunction (no name): " + exceptionMessage);
                }
            }
            return result;
        }

        public object[] CallFunction(string name, Action<Lua, int> update = null, string exceptionMessage = "", object arg0 = null, object arg1 = null)
        {
            object[] result = null;
            if (!enabled)
                return result;
            Lua oldLua = lua;
            lock (locker)
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

        /*public void LuaLock(object o, LuaFunction f)
        {
            lock (o)
                CallFunction(f, null, "LuaLock");
        }*/

        public string GetDebugTraceback()
        {
            try
            {
                Lua oldLua = lua;
                lock (locker)
                {
                    if (oldLua.UseTraceback)
                        return "_GetDebugTracebackException_";
                    return lua.GetDebugTraceback();
                }
            } catch
            {
                return "_GetDebugTracebackException_";
            }
        }

        public string ActiveHooks()
        {
            Lua oldLua = lua;
            lock (locker)
            {
                if (oldLua.UseTraceback)
                    return "LUA INSTANCE IS ALREADY DISPOSED!";
                string result = "Active hook list:";
                foreach (string name in LuaHookHandler.HandlerNames)
                    if (handlers[name].active)
                        result += "\n\t" + name;
                return result;
            }
        }

        public void UpdateHooks()
        {
            Lua oldLua = lua;
            lock (locker)
            {
                if (oldLua.UseTraceback)
                    return;
                foreach (string name in LuaHookHandler.HandlerNames) // TODO: Lazy hook handler creation
                    handlers[name].Update();
            }
        }

        public void UpdateHook(string name)
        {
            Lua oldLua = lua;
            lock (locker)
            {
                if (oldLua.UseTraceback)
                    return;
                handlers[name].Update();
            }
        }

        public void HookAll()
        {
            Lua oldLua = lua;
            lock (locker)
            {
                if (oldLua.UseTraceback)
                    return;
                foreach (var pair in handlers)
                    if (!pair.Value.active && pair.Key != "OnSendBytes" && pair.Key != "OnSendData") // TODO: check pair.Key for validance
                    {
                        lua.DoString("function " + pair.Key + "() print('" + pair.Key + "') end");
                        pair.Value.Hook();
                    }
            }
        }

        public void Unhook(string name)
        {
            Lua oldLua = lua;
            lock (locker)
            {
                if (oldLua.UseTraceback)
                    return;
                if (handlers[name].active)
                    handlers[name].Unhook();
                SetNull(name, $"Unhook ({name})");
            }
        }

        public void UnhookAll()
        {
            Lua oldLua = lua;
            lock (locker)
            {
                if (oldLua.UseTraceback)
                    return;
                foreach (var pair in handlers)
                    if (pair.Value.active)
                    {
                        pair.Value.Unhook();
                        SetNull(pair.Key, "UnhookAll");
                    }
            }
        }

        // DOES NOT ABORT INFINITE LOOPS
        public void LuaTaskDelay(int milliseconds, LuaFunction f)
        {
            Lua oldLua = lua;
            object tmp = data["source"];
            Task.Delay(milliseconds).ContinueWith(_ =>
            {
                lock (locker)
                {
                    if (!oldLua.UseTraceback) // IF LUASTATE ISN'T MARKED AS DISPOSED
                    {
                        object oldSource = data["source"];
                        CallFunction(f, (lua, state) => {
                            if (state == 0)
                                data["source"] = tmp;
                            else if (state > 0)
                                data["source"] = oldSource;
                        }, "LuaTaskDelay");
                        f.Dispose(); // BUG: Can cause AccessViolationException (read notes)
                    }
                }
            });
        }

        public void LuaReset()
        {
            Task.Delay(1).ContinueWith(_ =>
            {
                if (Initialize(LuaPlugin.me, true))
                    LuaPlugin.me.SendSuccessMessage($"Lua[{index}] has been reset.");
                else
                    LuaPlugin.me.SendErrorMessage($"Lua[{index}] reset failed.");
            });
        }

        public void LuaSleep(int milliseconds)
        {
            Thread.Sleep(milliseconds);
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
                object oldSource = data["source"];
                object[] executionResult = Execute("return " + script, (_lua, state) => {
                    if (state == 0)
                    {
                        LuaPlugin.me = player;
                        data["source"] = player;
                    }
                    else if (state > 0)
                        data["source"] = oldSource;
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

        // WARNING: This function is not safe. It can cause AccessViolationException that can't be handled by try or UnhandledException handler. Use at your own risk.
        public void ForceStop()
        {
            if (forceStopped)
                return;
            forceStopped = true;
            //PrintError("Setting ForceStop hook");
            try
            {
                lua.DoString("debug.sethook(function() error('Forced script stop.') end, '', 7)"); // TODO: Maybe resetting debug hook required here
            } catch (Exception e)
            {
                HandleException(e, "ForceStop");
            }
        }

        public void ForceResume()
        {
            forceStopped = false;
            bool unhooked = false;
            int attempt = 1;
            lock (locker)
                while (!unhooked)
                    try
                    {
                        lua.DoString("debug.sethook()");
                        unhooked = true;
                        PrintError("UNHOOKED WITH ATTEMPT NUMBER: " + attempt);
                    }
                    catch (Exception e2)
                    {
                        //HandleException(e2, "ForseResume");
                        attempt++;
                    }
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

        public void PlayersMessage(string text, Color clr, string permission = null, int luaEnvIndex = -1, TSPlayer except = null)
        {
            foreach (var player in GetPlayers(permission, luaEnvIndex, except))
                player.SendMessage(text, clr);
        }

        public void PlayersError(string text, string permission = null, int luaEnvIndex = -1, TSPlayer except = null)
        {
            foreach (var player in GetPlayers(permission, luaEnvIndex, except))
                player.SendErrorMessage(text);
        }

        public void PlayersStatus(string text, string permission = null, int luaEnvIndex = -1, TSPlayer except = null)
        {
            foreach (var player in GetPlayers(permission, luaEnvIndex, except))
                LuaHelper.PlayersStatus(player, text);
        }

        public List<TSPlayer> GetPlayers(string permission = null, int luaEnvIndex = -1, TSPlayer except = null)
        {
            List<TSPlayer> result = new List<TSPlayer>();

            foreach (TSPlayer p in TShock.Players)
                if (p != null && p.Active && (permission == null || p.HasPermission(permission)) && (luaEnvIndex == -1 || p.LuaEnv().index == luaEnvIndex) && p != except)
                    result.Add(p);

            return result;
        }

        public LuaFunction GenerateFunction(string code, string[] names = null)
        {
            code = $"return function({(names != null ? string.Join(",", names) : "")})" + code + ";end";
            object oldSource = data["source"];
            object[] executionResult = Execute(code, null, "GenerateFunction", false, true);
            return executionResult?[0] as LuaFunction;
        }

        public void HandleException(Exception e, string exceptionMessage = "")
        {
            lastException = e;
            lastExceptionLuaIndex = index;
            PrintError($"{exceptionMessage}\n{e}");//\n{GetDebugTraceback()}");
        }

        public void PrintError(string errorMessage)
        {
            if (printErrorStarted)
            {
                if (TShock.Log != null)
                    TShock.Log.ConsoleError($"Lua[{index}]: PrintError already started (ALTHOUGH: {errorMessage})");
                else
                    Console.Error.WriteLine($"Lua[{index}]: PrintError already started (ALTHOUGH: {errorMessage})");
                return;
            }
            printErrorStarted = true;

            errorMessage = $"Lua[{index}]: " + errorMessage;
            if (CallFunction("perror", null, "PrintError", errorMessage) == null)
            {
                if (TShock.Log != null)
                    TShock.Log.ConsoleError(errorMessage);
                else
                    Console.Error.WriteLine(errorMessage);
            }

            printErrorStarted = false;
        }

        public void ShowLastException()
        {
            if (LuaEnvironment.lastExceptionLuaIndex >= 0)
            {
                Exception e = lastException;
                PrintError($"\n> Message: {e.Message}\n> Data: {e.Data}\n> InnerException: {e.InnerException}\n" +
                    $"> StackTrace: {e.StackTrace}\n> Source: {e.Source}");
            }
            else
            {
                PrintError("No last exception found!");
            }
        }
    }
}
