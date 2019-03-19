using MyLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace LuaPlugin
{
    public class LuaHookManager
    {
        public List<ILuaHookHandler> Hooks = new List<ILuaHookHandler>
        {
            new LuaHookHandler<Action>(null, "OnTick", (hook, state) =>
            {
                if      (state ==  true) Main.OnTick += hook.Handler;
                else if (state == false) Main.OnTick -= hook.Handler;
                else hook.Handler = () => hook.Invoke();
            }),
            new LuaHookHandler<HookHandler<EventArgs>>(null, "OnTick", (hook, state) =>
            {
                if      (state ==  true) ServerApi.Hooks.GameInitialize.Register(null, hook.Handler);
                else if (state == false) ServerApi.Hooks.GameInitialize.Deregister(null, hook.Handler);
                else hook.Handler = (args) => hook.Invoke(args);
            }),
            new LuaHookHandler<AccountHooks.AccountCreateD>(null, "OnTick", (hook, state) =>
            {
                if (state == true) AccountHooks.AccountCreate += hook.Handler;
                else if (state == false) AccountHooks.AccountCreate -= hook.Handler;
                else hook.Handler = (args) => hook.Invoke(args);
                typeof(AccountHooks).GetEvent("AccountCreate").AddEventHandler(hook, hook.Handler);
            }),
            new LuaHookHandler<EventHandler<GetDataHandlers.NewProjectileEventArgs>>(null, "asd", (hook, state) =>
            {
                if (state == true) GetDataHandlers.NewProjectile += hook.Handler;
                else if (state == false) GetDataHandlers.NewProjectile -= hook.Handler;
                else hook.Handler = (sender, args) => hook.Invoke(args);
            })
    };

        public void Initialize(LuaEnvironment luaEnv)
        {
            luaEnv.AddHook(null);
        }

        public void AddEventHook<T>(LuaEnvironment luaEnv, string name, Type type, string eventName)
            where T : Delegate
        {
            luaEnv.AddHook(new LuaHookHandler<T>(luaEnv, name, (hook, state) =>
            {
                if (state == true) { }
                else if (state == false) { }
                else hook.Handler = (args) => hook.Invoke(args);
            }));
        }
    }
}
