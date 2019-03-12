using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLua
{
    public class Config
    {
        [JsonProperty("path")]
        public static string Path = "lua";
        [JsonProperty("key")]
        public static string Key = "server1";
        [JsonProperty("command_specifier")]
        public static string CommandSpecifier = ";";
        [JsonProperty("control_permission")]
        public static string ControlPermission = "lua.control";
        [JsonProperty("execute_permission")]
        public static string ExecutePermission = "lua.execute";
        [JsonProperty("untrusted_lua_index")]
        public static int UntrustedLuaIndex = 0;

        #region Write

        public static void Save()
        {
            string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "lua_config.json");
            File.WriteAllText(path, JsonConvert.SerializeObject(new Config(), Formatting.Indented));
        }

        #endregion
        #region Load

        public static void Load()
        {
            string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "lua_config.json");
            if (File.Exists(path))
                JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
            else
                Save();
        }

        #endregion

    }
}
