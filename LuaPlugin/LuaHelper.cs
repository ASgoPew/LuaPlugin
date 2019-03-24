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
        public static string StatusEnding = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n";

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
            player.SendData(PacketTypes.Status, text + StatusEnding);
        }
    }
}
