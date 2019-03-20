using MyLua;
using Terraria;
using TShockAPI;

namespace LuaPlugin
{
    public static class TSPlayerExtension
    {
        public static LuaEnvironment LuaEnv(this TSPlayer player)
        {
            if (player.HasPermission(Config.ControlPermission))
            {
                string env = LuaPlugin.LuaEnv[player.Index >= 0 ? player.Index : Main.maxPlayers];
                if (env != null)
                    return Config.Environments[env];
            }
            else if (Config.UntrustedLua != null)
                return Config.Environments[Config.UntrustedLua];
            return null;
        }
    }
}
