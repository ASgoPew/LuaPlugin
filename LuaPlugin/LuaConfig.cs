using MyLua;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace LuaPlugin
{
    public class LuaConfig
    {
        [JsonProperty("command_specifier")]
        public static string CommandSpecifier = ";";
        [JsonProperty("control_permission")]
        public static string ControlPermission = "lua.control";
        [JsonProperty("execute_permission")]
        public static string ExecutePermission = "lua.execute";
        [JsonProperty("default_lua")]
        public static string DefaultLua;
        [JsonProperty("untrusted_lua")]
        public static string UntrustedLua;
        [JsonProperty("environments")]
        public static Dictionary<string, LuaEnvironment> Environments;

        #region Write

        public static void Save()
        {
            string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "lua_config.json");
            File.WriteAllText(path, JsonConvert.SerializeObject(new LuaConfig(), Formatting.Indented));
        }

        #endregion
        #region Load

        public static void Load()
        {
            string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "lua_config.json");
            if (File.Exists(path))
                JsonConvert.DeserializeObject<LuaConfig>(File.ReadAllText(path));
            else
                Save();

            Update();
        }

        #endregion
        #region Update

        public static void Update()
        {
            //if (DefaultLua != null && Environments.Count > 0 && !Environments.ContainsKey(DefaultLua))

            if (Environments == null)
                Environments = new Dictionary<string, LuaEnvironment>();
            foreach (var pair in Environments)
            {
                pair.Value.Name = pair.Key;
                pair.Value.LuaException += (string name, Exception e) =>
                    LuaPlugin.PrintError(TSPlayer.Server, pair.Value, e);
                LuaHookManager.Initialize(pair.Value);
            }
        }

        #endregion
    }
}
