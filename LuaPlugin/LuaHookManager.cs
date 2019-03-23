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
using System.ComponentModel;
using System.Reflection;

namespace LuaPlugin
{
    public class LuaHookManager
    {
        public static void AddHookHandlerHook<T>(LuaEnvironment luaEnv, string name, HandlerCollection<T> handlerCollection)
            where T : EventArgs
        {
            luaEnv.AddHook(new LuaHookHandler<HookHandler<T>>(luaEnv, name, (hook, state) =>
            {
                if      (state ==  true) handlerCollection.Register(LuaPlugin.Instance, hook.Handler);
                else if (state == false) handlerCollection.Deregister(LuaPlugin.Instance, hook.Handler);
                else hook.Handler = (args) => hook.Invoke(args);
            }));
        }

        public static void AddEventHandlerHook<T>(LuaEnvironment luaEnv, string name, ref HandlerList<T> handlerList)
            where T : EventArgs
        {
            if (handlerList == null)
                handlerList = new HandlerList<T>();
            HandlerList<T> nonRefHandlerList = handlerList;
            luaEnv.AddHook(new LuaHookHandler<EventHandler<T>>(luaEnv, name, (hook, state) =>
            {
                if (state == true) nonRefHandlerList += hook.Handler;
                else if (state == false) nonRefHandlerList -= hook.Handler;
                else hook.Handler = (sender, args) => hook.Invoke(args);
            }));
        }

        public static void Initialize(LuaEnvironment luaEnv)
        {
            AddHookHandlerHook(luaEnv, "OnGameUpdate", ServerApi.Hooks.GameUpdate);
            AddEventHandlerHook(luaEnv, "OnPacketNewProjectile", ref GetDataHandlers.NewProjectile);
            luaEnv.AddEventHook("OnPlayerChat", typeof(PlayerHooks), "PlayerChat");

            /*List<ILuaHookHandler> hooks = new List<ILuaHookHandler>
            {
                new LuaHookHandler<Action>(luaEnv, "OnTick", (hook, state) =>
                {
                    if      (state ==  true) Main.OnTick += hook.Handler;
                    else if (state == false) Main.OnTick -= hook.Handler;
                    else hook.Handler = () => hook.Invoke();
                }),

                // ServerApi.Hooks hooks
                new LuaHookHandler<HookHandler<DropBossBagEventArgs>>(luaEnv, "OnDropBossBag", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.DropBossBag.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.DropBossBag.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<HardmodeTileUpdateEventArgs>>(luaEnv, "OnHardmodeTileUpdate", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.GameHardmodeTileUpdate.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.GameHardmodeTileUpdate.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<EventArgs>>(luaEnv, "OnGameInitialize", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.GameInitialize.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.GameInitialize.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<EventArgs>>(luaEnv, "OnGamePostInitialize", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.GamePostInitialize.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.GamePostInitialize.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<EventArgs>>(luaEnv, "OnGamePostUpdate", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.GamePostUpdate.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.GamePostUpdate.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<StatueSpawnEventArgs>>(luaEnv, "OnStatueSpawn", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.GameStatueSpawn.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.GameStatueSpawn.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<EventArgs>>(luaEnv, "OnGameUpdate", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.GameUpdate.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.GameUpdate.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<EventArgs>>(luaEnv, "OnGameWorldConnect", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.GameWorldConnect.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.GameWorldConnect.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<EventArgs>>(luaEnv, "OnGameWorldDisconnect", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.GameWorldDisconnect.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.GameWorldDisconnect.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<ForceItemIntoChestEventArgs>>(luaEnv, "OnForceItemIntoChest", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.ItemForceIntoChest.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.ItemForceIntoChest.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<SetDefaultsEventArgs<Item, int>>>(luaEnv, "OnItemNetDefaults", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.ItemNetDefaults.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.ItemNetDefaults.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<SetDefaultsEventArgs<Item, int>>>(luaEnv, "OnItemSetDefualtsInt", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.ItemSetDefaultsInt.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.ItemSetDefaultsInt.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<SetDefaultsEventArgs<Item, string>>>(luaEnv, "OnItemSetDefaultsString", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.ItemSetDefaultsString.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.ItemSetDefaultsString.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<GetDataEventArgs>>(luaEnv, "OnGetData", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.NetGetData.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.NetGetData.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<GreetPlayerEventArgs>>(luaEnv, "OnGreetPlayer", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.NetGreetPlayer.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.NetGreetPlayer.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<NameCollisionEventArgs>>(luaEnv, "OnNameCollision", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.NetNameCollision.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.NetNameCollision.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<SendBytesEventArgs>>(luaEnv, "OnSendBytes", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.NetSendBytes.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.NetSendBytes.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<SendDataEventArgs>>(luaEnv, "OnSendData", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.NetSendData.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.NetSendData.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<NpcAiUpdateEventArgs>>(luaEnv, "OnNpcAiUpdate", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.NpcAIUpdate.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.NpcAIUpdate.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<NpcKilledEventArgs>>(luaEnv, "OnNpcKilled", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.NpcKilled.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.NpcKilled.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<NpcLootDropEventArgs>>(luaEnv, "OnNpcLootDrop", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.NpcLootDrop.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.NpcLootDrop.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<SetDefaultsEventArgs<NPC, int>>>(luaEnv, "OnNpcNetDefaults", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.NpcNetDefaults.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.NpcNetDefaults.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<SetDefaultsEventArgs<NPC, int>>>(luaEnv, "OnNpcSetDefaultsInt", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.NpcSetDefaultsInt.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.NpcSetDefaultsInt.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<SetDefaultsEventArgs<NPC, string>>>(luaEnv, "OnNpcSetDefaultsString", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.NpcSetDefaultsString.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.NpcSetDefaultsString.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<NpcSpawnEventArgs>>(luaEnv, "OnNpcSpawn", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.NpcSpawn.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.NpcSpawn.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<NpcStrikeEventArgs>>(luaEnv, "OnNpcStrike", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.NpcStrike.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.NpcStrike.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<NpcTransformationEventArgs>>(luaEnv, "OnNpcTransform", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.NpcTransform.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.NpcTransform.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<TriggerPressurePlateEventArgs<NPC>>>(luaEnv, "OnNpcTriggerPressurePlate", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.NpcTriggerPressurePlate.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.NpcTriggerPressurePlate.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<TriggerPressurePlateEventArgs<Player>>>(luaEnv, "OnPlayerTriggerPressurePlate", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.PlayerTriggerPressurePlate.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.PlayerTriggerPressurePlate.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<UpdatePhysicsEventArgs>>(luaEnv, "OnPlayerUpdatePhysics", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.PlayerUpdatePhysics.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.PlayerUpdatePhysics.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<ProjectileAiUpdateEventArgs>>(luaEnv, "OnProjectileAiUpdate", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.ProjectileAIUpdate.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.ProjectileAIUpdate.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<SetDefaultsEventArgs<Projectile, int>>>(luaEnv, "OnProjectileSetDefaults", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.ProjectileSetDefaults.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.ProjectileSetDefaults.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<TriggerPressurePlateEventArgs<Projectile>>>(luaEnv, "OnProjectileTriggerPressurePlate", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.ProjectileTriggerPressurePlate.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.ProjectileTriggerPressurePlate.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<ServerBroadcastEventArgs>>(luaEnv, "OnServerBroadcast", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.ServerBroadcast.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.ServerBroadcast.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<ServerChatEventArgs>>(luaEnv, "OnServerChat", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.ServerChat.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.ServerChat.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<CommandEventArgs>>(luaEnv, "OnServerCommand", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.ServerCommand.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.ServerCommand.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<ConnectEventArgs>>(luaEnv, "OnServerConnect", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.ServerConnect.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.ServerConnect.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<JoinEventArgs>>(luaEnv, "OnServerJoin", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.ServerJoin.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.ServerJoin.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<LeaveEventArgs>>(luaEnv, "OnServerLeave", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.ServerLeave.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.ServerLeave.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<SocketResetEventArgs>>(luaEnv, "OnServerSocketReset", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.ServerSocketReset.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.ServerSocketReset.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<TriggerAnnouncementBoxEventArgs>>(luaEnv, "OnWireTriggerAnnouncementBox", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.WireTriggerAnnouncementBox.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.WireTriggerAnnouncementBox.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<ChristmasCheckEventArgs>>(luaEnv, "OnWorldChristmasCheck", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.WorldChristmasCheck.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.WorldChristmasCheck.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<HalloweenCheckEventArgs>>(luaEnv, "OnWorldHalloweenCheck", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.WorldHalloweenCheck.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.WorldHalloweenCheck.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<MeteorDropEventArgs>>(luaEnv, "OnWorldMeteorDrop", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.WorldMeteorDrop.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.WorldMeteorDrop.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<WorldSaveEventArgs>>(luaEnv, "OnWorldSave", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.WorldSave.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.WorldSave.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<HookHandler<HandledEventArgs>>(luaEnv, "OnWorldStartHardMode", (hook, state) =>
                {
                    if      (state ==  true) ServerApi.Hooks.WorldStartHardMode.Register(LuaPlugin.Instance, hook.Handler);
                    else if (state == false) ServerApi.Hooks.WorldStartHardMode.Deregister(LuaPlugin.Instance, hook.Handler);
                    else hook.Handler = (args) => hook.Invoke(args);
                }),

                // TShockAPI.Hooks.AccountHooks
                
                new LuaHookHandler<AccountHooks.AccountCreateD>(luaEnv, "OnAccountCreate", (hook, state) =>
                {
                    if      (state ==  true) AccountHooks.AccountCreate += hook.Handler;
                    else if (state == false) AccountHooks.AccountCreate -= hook.Handler;
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<AccountHooks.AccountDeleteD>(luaEnv, "OnAccountDelete", (hook, state) =>
                {
                    if      (state ==  true) AccountHooks.AccountDelete += hook.Handler;
                    else if (state == false) AccountHooks.AccountDelete -= hook.Handler;
                    else hook.Handler = (args) => hook.Invoke(args);
                }),

                // TShockAPI.Hooks.GeneralHooks
                
                new LuaHookHandler<GeneralHooks.ReloadEventD>(luaEnv, "OnReloadEvent", (hook, state) =>
                {
                    if      (state ==  true) GeneralHooks.ReloadEvent += hook.Handler;
                    else if (state == false) GeneralHooks.ReloadEvent -= hook.Handler;
                    else hook.Handler = (args) => hook.Invoke(args);
                }),

                // TShockAPI.Hooks.PlayerHooks
                new LuaHookHandler<PlayerHooks.PlayerChatD>(luaEnv, "OnPlayerChat", (hook, state) =>
                {
                    if      (state ==  true) PlayerHooks.PlayerChat += hook.Handler;
                    else if (state == false) PlayerHooks.PlayerChat -= hook.Handler;
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<PlayerHooks.PlayerCommandD>(luaEnv, "OnPlayerCommand", (hook, state) =>
                {
                    if      (state ==  true) PlayerHooks.PlayerCommand += hook.Handler;
                    else if (state == false) PlayerHooks.PlayerCommand -= hook.Handler;
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<PlayerHooks.PlayerLogoutD>(luaEnv, "OnPlayerLogout", (hook, state) =>
                {
                    if      (state ==  true) PlayerHooks.PlayerLogout += hook.Handler;
                    else if (state == false) PlayerHooks.PlayerLogout -= hook.Handler;
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<PlayerHooks.PlayerPermissionD>(luaEnv, "OnPlayerPermission", (hook, state) =>
                {
                    if      (state ==  true) PlayerHooks.PlayerPermission += hook.Handler;
                    else if (state == false) PlayerHooks.PlayerPermission -= hook.Handler;
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<PlayerHooks.PlayerPostLoginD>(luaEnv, "OnPlayerPostLogin", (hook, state) =>
                {
                    if      (state ==  true) PlayerHooks.PlayerPostLogin += hook.Handler;
                    else if (state == false) PlayerHooks.PlayerPostLogin -= hook.Handler;
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<PlayerHooks.PlayerPreLoginD>(luaEnv, "OnPlayerPreLogin", (hook, state) =>
                {
                    if      (state ==  true) PlayerHooks.PlayerPreLogin += hook.Handler;
                    else if (state == false) PlayerHooks.PlayerPreLogin -= hook.Handler;
                    else hook.Handler = (args) => hook.Invoke(args);
                }),

                // TShockAPI.Hooks.RegionHooks
                new LuaHookHandler<RegionHooks.RegionCreatedD>(luaEnv, "OnRegionCreated", (hook, state) =>
                {
                    if      (state ==  true) RegionHooks.RegionCreated += hook.Handler;
                    else if (state == false) RegionHooks.RegionCreated -= hook.Handler;
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<RegionHooks.RegionDeletedD>(luaEnv, "OnRegionDeleted", (hook, state) =>
                {
                    if      (state ==  true) RegionHooks.RegionDeleted += hook.Handler;
                    else if (state == false) RegionHooks.RegionDeleted -= hook.Handler;
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<RegionHooks.RegionEnteredD>(luaEnv, "OnRegionEntered", (hook, state) =>
                {
                    if      (state ==  true) RegionHooks.RegionEntered += hook.Handler;
                    else if (state == false) RegionHooks.RegionEntered -= hook.Handler;
                    else hook.Handler = (args) => hook.Invoke(args);
                }),
                new LuaHookHandler<RegionHooks.RegionLeftD>(luaEnv, "OnRegionLeft", (hook, state) =>
                {
                    if      (state ==  true) RegionHooks.RegionLeft += hook.Handler;
                    else if (state == false) RegionHooks.RegionLeft -= hook.Handler;
                    else hook.Handler = (args) => hook.Invoke(args);
                }),

                // TShockAPI.TShock
                new LuaHookHandler<Action>(luaEnv, "OnTShockInitialize", (hook, state) =>
                {
                    if      (state ==  true) TShock.Initialized += hook.Handler;
                    else if (state == false) TShock.Initialized -= hook.Handler;
                    else hook.Handler = () => hook.Invoke();
                }),

                // TShockAPI.GetDataHandlers
                new LuaHookHandler<EventHandler<GetDataHandlers.ChestItemEventArgs>>(luaEnv, "OnPacketChestItemChange", (hook, state) =>
                {
                    if      (state ==  true) GetDataHandlers.ChestItemChange += hook.Handler;
                    else if (state == false) GetDataHandlers.ChestItemChange -= hook.Handler;
                    else hook.Handler = (sender, args) => hook.Invoke(args);
                }),
                new LuaHookHandler<EventHandler<GetDataHandlers.ChestOpenEventArgs>>(luaEnv, "OnPacketChestOpen", (hook, state) =>
                {
                    if      (state ==  true) GetDataHandlers.ChestOpen += hook.Handler;
                    else if (state == false) GetDataHandlers.ChestOpen -= hook.Handler;
                    else hook.Handler = (sender, args) => hook.Invoke(args);
                }),
                new LuaHookHandler<EventHandler<GetDataHandlers.GemLockToggleEventArgs>>(luaEnv, "OnPacketGemLockToggle", (hook, state) =>
                {
                    if      (state ==  true) GetDataHandlers.GemLockToggle += hook.Handler;
                    else if (state == false) GetDataHandlers.GemLockToggle -= hook.Handler;
                    else hook.Handler = (sender, args) => hook.Invoke(args);
                }),
                new LuaHookHandler<EventHandler<GetDataHandlers.ItemDropEventArgs>>(luaEnv, "OnPacketItemDrop", (hook, state) =>
                {
                    if      (state ==  true) GetDataHandlers.ItemDrop += hook.Handler;
                    else if (state == false) GetDataHandlers.ItemDrop -= hook.Handler;
                    else hook.Handler = (sender, args) => hook.Invoke(args);
                }),
                new LuaHookHandler<EventHandler<GetDataHandlers.KillMeEventArgs>>(luaEnv, "OnPacketKillMe", (hook, state) =>
                {
                    if      (state ==  true) GetDataHandlers.KillMe += hook.Handler;
                    else if (state == false) GetDataHandlers.KillMe -= hook.Handler;
                    else hook.Handler = (sender, args) => hook.Invoke(args);
                }),
                new LuaHookHandler<EventHandler<GetDataHandlers.LiquidSetEventArgs>>(luaEnv, "OnPacketLiquidSet", (hook, state) =>
                {
                    if      (state ==  true) GetDataHandlers.LiquidSet += hook.Handler;
                    else if (state == false) GetDataHandlers.LiquidSet -= hook.Handler;
                    else hook.Handler = (sender, args) => hook.Invoke(args);
                }),
                new LuaHookHandler<EventHandler<GetDataHandlers.NewProjectileEventArgs>>(luaEnv, "OnPacketNewProjectile", (hook, state) =>
                {
                    if      (state ==  true) GetDataHandlers.NewProjectile += hook.Handler;
                    else if (state == false) GetDataHandlers.NewProjectile -= hook.Handler;
                    else hook.Handler = (sender, args) => hook.Invoke(args);
                }),
                new LuaHookHandler<EventHandler<GetDataHandlers.NPCHomeChangeEventArgs>>(luaEnv, "OnPacketNPCHome", (hook, state) =>
                {
                    if      (state ==  true) GetDataHandlers.NPCHome += hook.Handler;
                    else if (state == false) GetDataHandlers.NPCHome -= hook.Handler;
                    else hook.Handler = (sender, args) => hook.Invoke(args);
                }),
                new LuaHookHandler<EventHandler<GetDataHandlers.NPCSpecialEventArgs>>(luaEnv, "OnPacketNPCSpecial", (hook, state) =>
                {
                    if      (state ==  true) GetDataHandlers.NPCSpecial += hook.Handler;
                    else if (state == false) GetDataHandlers.NPCSpecial -= hook.Handler;
                    else hook.Handler = (sender, args) => hook.Invoke(args);
                }),
                new LuaHookHandler<EventHandler<GetDataHandlers.NPCStrikeEventArgs>>(luaEnv, "OnPacketNPCStrike", (hook, state) =>
                {
                    if      (state ==  true) GetDataHandlers.NPCStrike += hook.Handler;
                    else if (state == false) GetDataHandlers.NPCStrike -= hook.Handler;
                    else hook.Handler = (sender, args) => hook.Invoke(args);
                }),
                new LuaHookHandler<EventHandler<GetDataHandlers.PaintTileEventArgs>>(luaEnv, "OnPacketPaintTile", (hook, state) =>
                {
                    if      (state ==  true) GetDataHandlers.PaintTile += hook.Handler;
                    else if (state == false) GetDataHandlers.PaintTile -= hook.Handler;
                    else hook.Handler = (sender, args) => hook.Invoke(args);
                }),
                new LuaHookHandler<EventHandler<GetDataHandlers.PaintWallEventArgs>>(luaEnv, "OnPacketPaintWall", (hook, state) =>
                {
                    if      (state ==  true) GetDataHandlers.PaintWall += hook.Handler;
                    else if (state == false) GetDataHandlers.PaintWall -= hook.Handler;
                    else hook.Handler = (sender, args) => hook.Invoke(args);
                }),
                new LuaHookHandler<EventHandler<GetDataHandlers.PlayerAnimationEventArgs>>(luaEnv, "OnPacketPlayerAnimation", (hook, state) =>
                {
                    if      (state ==  true) GetDataHandlers.PlayerAnimation += hook.Handler;
                    else if (state == false) GetDataHandlers.PlayerAnimation -= hook.Handler;
                    else hook.Handler = (sender, args) => hook.Invoke(args);
                }),
                new LuaHookHandler<EventHandler<GetDataHandlers.PlayerBuffEventArgs>>(luaEnv, "OnPacketPlayerBuff", (hook, state) =>
                {
                    if      (state ==  true) GetDataHandlers.PlayerBuff += hook.Handler;
                    else if (state == false) GetDataHandlers.PlayerBuff -= hook.Handler;
                    else hook.Handler = (sender, args) => hook.Invoke(args);
                }),
                new LuaHookHandler<EventHandler<GetDataHandlers.PlayerBuffUpdateEventArgs>>(luaEnv, "OnPacketPlayerBuffUpdate", (hook, state) =>
                {
                    if      (state ==  true) GetDataHandlers.PlayerBuffUpdate += hook.Handler;
                    else if (state == false) GetDataHandlers.PlayerBuffUpdate -= hook.Handler;
                    else hook.Handler = (sender, args) => hook.Invoke(args);
                }),
                new LuaHookHandler<EventHandler<GetDataHandlers.PlayerDamageEventArgs>>(luaEnv, "OnPacketPlayerDamage", (hook, state) =>
                {
                    if      (state ==  true) GetDataHandlers.PlayerDamage += hook.Handler;
                    else if (state == false) GetDataHandlers.PlayerDamage -= hook.Handler;
                    else hook.Handler = (sender, args) => hook.Invoke(args);
                }),
                new LuaHookHandler<EventHandler<GetDataHandlers.PlayerHPEventArgs>>(luaEnv, "OnPacketPlayerHP", (hook, state) =>
                {
                    if      (state ==  true) GetDataHandlers.PlayerHP += hook.Handler;
                    else if (state == false) GetDataHandlers.PlayerHP -= hook.Handler;
                    else hook.Handler = (sender, args) => hook.Invoke(args);
                }),
                new LuaHookHandler<EventHandler<GetDataHandlers.PlayerInfoEventArgs>>(luaEnv, "OnPacketPlayerInfo", (hook, state) =>
                {
                    if      (state ==  true) GetDataHandlers.PlayerInfo += hook.Handler;
                    else if (state == false) GetDataHandlers.PlayerInfo -= hook.Handler;
                    else hook.Handler = (sender, args) => hook.Invoke(args);
                }),
                new LuaHookHandler<EventHandler<GetDataHandlers.PlayerManaEventArgs>>(luaEnv, "OnPacketPlayerMana", (hook, state) =>
                {
                    if      (state ==  true) GetDataHandlers.PlayerMana += hook.Handler;
                    else if (state == false) GetDataHandlers.PlayerMana -= hook.Handler;
                    else hook.Handler = (sender, args) => hook.Invoke(args);
                }),
                new LuaHookHandler<EventHandler<GetDataHandlers.PlayerSlotEventArgs>>(luaEnv, "OnPacketPlayerSlot", (hook, state) =>
                {
                    if      (state ==  true) GetDataHandlers.PlayerSlot += hook.Handler;
                    else if (state == false) GetDataHandlers.PlayerSlot -= hook.Handler;
                    else hook.Handler = (sender, args) => hook.Invoke(args);
                }),
                new LuaHookHandler<EventHandler<GetDataHandlers.SpawnEventArgs>>(luaEnv, "OnPacketPlayerSpawn", (hook, state) =>
                {
                    if      (state ==  true) GetDataHandlers.PlayerSpawn += hook.Handler;
                    else if (state == false) GetDataHandlers.PlayerSpawn -= hook.Handler;
                    else hook.Handler = (sender, args) => hook.Invoke(args);
                }),
                new LuaHookHandler<EventHandler<GetDataHandlers.PlayerTeamEventArgs>>(luaEnv, "OnPacketPlayerTeam", (hook, state) =>
                {
                    if      (state ==  true) GetDataHandlers.PlayerTeam += hook.Handler;
                    else if (state == false) GetDataHandlers.PlayerTeam -= hook.Handler;
                    else hook.Handler = (sender, args) => hook.Invoke(args);
                }),
                new LuaHookHandler<EventHandler<GetDataHandlers.PlayerUpdateEventArgs>>(luaEnv, "OnPacketPlayerUpdate", (hook, state) =>
                {
                    if      (state ==  true) GetDataHandlers.PlayerUpdate += hook.Handler;
                    else if (state == false) GetDataHandlers.PlayerUpdate -= hook.Handler;
                    else hook.Handler = (sender, args) => hook.Invoke(args);
                }),
                new LuaHookHandler<EventHandler<GetDataHandlers.SendTileSquareEventArgs>>(luaEnv, "OnPacketSendTileSquare", (hook, state) =>
                {
                    if      (state ==  true) GetDataHandlers.SendTileSquare += hook.Handler;
                    else if (state == false) GetDataHandlers.SendTileSquare -= hook.Handler;
                    else hook.Handler = (sender, args) => hook.Invoke(args);
                }),
                new LuaHookHandler<EventHandler<GetDataHandlers.SignEventArgs>>(luaEnv, "OnPacketSign", (hook, state) =>
                {
                    if      (state ==  true) GetDataHandlers.Sign += hook.Handler;
                    else if (state == false) GetDataHandlers.Sign -= hook.Handler;
                    else hook.Handler = (sender, args) => hook.Invoke(args);
                }),
                new LuaHookHandler<EventHandler<GetDataHandlers.TeleportEventArgs>>(luaEnv, "OnPacketTeleport", (hook, state) =>
                {
                    if      (state ==  true) GetDataHandlers.Teleport += hook.Handler;
                    else if (state == false) GetDataHandlers.Teleport -= hook.Handler;
                    else hook.Handler = (sender, args) => hook.Invoke(args);
                }),
                new LuaHookHandler<EventHandler<GetDataHandlers.TileEditEventArgs>>(luaEnv, "OnPacketTileEdit", (hook, state) =>
                {
                    if      (state ==  true) GetDataHandlers.TileEdit += hook.Handler;
                    else if (state == false) GetDataHandlers.TileEdit -= hook.Handler;
                    else hook.Handler = (sender, args) => hook.Invoke(args);
                }),
                new LuaHookHandler<EventHandler<GetDataHandlers.TileKillEventArgs>>(luaEnv, "OnPacketTileKill", (hook, state) =>
                {
                    if      (state ==  true) GetDataHandlers.TileKill += hook.Handler;
                    else if (state == false) GetDataHandlers.TileKill -= hook.Handler;
                    else hook.Handler = (sender, args) => hook.Invoke(args);
                }),
                new LuaHookHandler<EventHandler<GetDataHandlers.TogglePvpEventArgs>>(luaEnv, "OnPacketTogglePvp", (hook, state) =>
                {
                    if      (state ==  true) GetDataHandlers.TogglePvp += hook.Handler;
                    else if (state == false) GetDataHandlers.TogglePvp -= hook.Handler;
                    else hook.Handler = (sender, args) => hook.Invoke(args);
                })
            };

            foreach (var hook in hooks)
                luaEnv.AddHook(hook);*/
        }

        /*public void AddEventHook<T>(LuaEnvironment luaEnv, string name, Type type, string eventName)
            where T : Delegate
        {
            luaEnv.AddHook(new LuaHookHandler<T>(luaEnv, name, (hook, state) =>
            {
                if (state == true) { }
                else if (state == false) { }
                else hook.Handler = (args) => hook.Invoke(args);
            }));
        }*/
    }
}
