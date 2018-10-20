using Terraria;
using TShockAPI;

namespace LuaPlugin
{
    public static class TSPlayerExtension
    {
        public static LuaEnvironment LuaEnv(this TSPlayer player)
        {
            return LuaPlugin.luas[player.HasPermission("lua.control") ? LuaPlugin.luaEnvIndex[player.Index >= 0 ? player.Index : Main.maxPlayers] : LuaPlugin.untrustedLuaIndex];
        }
    }
}
