﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaPlugin
{
    public class Config
    {
        [JsonProperty]
        public static string path = "lua";
        [JsonProperty]
        public static string key = "server1";

        #region Write

        public static void Save()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "lua_config.json");
            File.WriteAllText(path, JsonConvert.SerializeObject(new Config(), Formatting.Indented));
        }

        #endregion
        #region Load

        public static void Load()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "lua_config.json");
            if (File.Exists(path))
                JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
            else
                Save();
        }

        #endregion

    }
}
