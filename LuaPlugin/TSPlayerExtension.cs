using Terraria;
using TShockAPI;

namespace LuaPlugin
{
    public static class TSPlayerExtension
    {
        public static LuaEnvironment LuaEnv(this TSPlayer player)
        {
            return LuaPlugin.Luas[player.HasPermission("lua.control") ? LuaPlugin.LuaEnvIndex[player.Index >= 0 ? player.Index : Main.maxPlayers] : Config.untrusted_lua_index];
        }
    }
}
