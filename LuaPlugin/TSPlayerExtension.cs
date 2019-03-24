using MyLua;
using Terraria;
using TShockAPI;

namespace LuaPlugin
{
    public static class TSPlayerExtension
    {
        public static LuaEnvironment LuaEnv(this TSPlayer player)
        {
            if (player.HasPermission(LuaConfig.ControlPermission))
            {
                string env = LuaPlugin.LuaEnv[player.Index >= 0 ? player.Index : Main.maxPlayers];
                if (env != null)
                    return LuaConfig.Environments[env];
                else if (LuaConfig.DefaultLua != null)
                    return LuaConfig.Environments[LuaConfig.DefaultLua];
            }
            else if (LuaConfig.UntrustedLua != null && player.HasPermission(LuaConfig.ExecutePermission))
                return LuaConfig.Environments[LuaConfig.UntrustedLua];
            return null;
        }
    }
}
