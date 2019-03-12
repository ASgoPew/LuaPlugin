using Terraria;
using TShockAPI;

namespace MyLua
{
    public static class TSPlayerExtension
    {
        public static LuaEnvironment LuaEnv(this TSPlayer player)
        {
            return LuaPlugin.Luas[player.HasPermission("lua.control") ? LuaPlugin.LuaEnvIndex[player.Index >= 0 ? player.Index : Main.maxPlayers] : Config.UntrustedLuaIndex];
        }
    }
}
