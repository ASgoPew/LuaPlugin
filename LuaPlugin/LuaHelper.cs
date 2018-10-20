using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLua;
using System.IO;
using TerrariaApi.Server;
using System.Diagnostics;
using TShockAPI;
using Terraria;

namespace LuaPlugin
{
    public class LuaHelper
    {
        public static string statusEnding = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n";

        public static long GetRAMUsage()
        {
            // Works 67928 times per second
            return GC.GetTotalMemory(false);

            // Works 325 times per second
            using (Process currentProcess = Process.GetCurrentProcess())
                return currentProcess.WorkingSet64;
            
            // Works 70 times per second but more accurate
            System.Diagnostics.Process proc = Process.GetCurrentProcess();
            int memsize = 0; // memsize in Megabyte
            PerformanceCounter PC = new PerformanceCounter();
            PC.CategoryName = "Process";
            PC.CounterName = "Working Set - Private";
            PC.InstanceName = proc.ProcessName;
            memsize = Convert.ToInt32(PC.NextValue()) / (int)(1024);
            PC.Close();
            PC.Dispose();
            proc.Dispose();
            return memsize;
        }

        public static string PacketString(byte[] packet, int index, int packetSize)
        {
            return $"{(PacketTypes)packet[index + 2]} {{size={packetSize}}}:      " + string.Join(",", packet.Skip(index).Take(packetSize > 15 ? 15 : packetSize).Select(b => b.ToString()).ToArray());
        }

        public static void PlayersStatus(TSPlayer player, string text)
        {
            player.SendData(PacketTypes.Status, text + statusEnding);
        }

        public static string SharpShow(object o)
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
            {
                foreach (var field in type.GetFields())
                    if (field.IsPublic)
                    {
                        result += "\n   " + field.Name + ": " + new string(' ', 30 - (field.Name.Length % 30)) + field.GetValue(o) ?? "null";
                    }
            }
            return result;
        }
    }
}
