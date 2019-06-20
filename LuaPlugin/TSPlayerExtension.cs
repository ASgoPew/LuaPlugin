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
                else if (LuaConfig.DefaultEnvironment != null)
                    return LuaConfig.Environments[LuaConfig.DefaultEnvironment];
            }
            else if (LuaConfig.UntrustedEnvironment != null && player.HasPermission(LuaConfig.ExecutePermission))
                return LuaConfig.Environments[LuaConfig.UntrustedEnvironment];
            return null;
        }
    }
}
