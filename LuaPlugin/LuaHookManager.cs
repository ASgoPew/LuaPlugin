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
        

        public void Initialize(LuaEnvironment luaEnv)
        {
            List<ILuaHookHandler> hooks = new List<ILuaHookHandler>
            {
                /*new LuaHookHandler<Action>(null, "OnTick", (hook, state) =>
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
                }),*/

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
                /*case "OnSendData":
                    if (on) ServerApi.Hooks.NetSendData.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.NetSendData.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnNpcAiUpdate":
                    if (on) ServerApi.Hooks.NpcAIUpdate.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.NpcAIUpdate.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnNpcKilled":
                    if (on) ServerApi.Hooks.NpcKilled.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.NpcKilled.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnNpcLootDrop":
                    if (on) ServerApi.Hooks.NpcLootDrop.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.NpcLootDrop.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnNpcNetDefaults":
                    if (on) ServerApi.Hooks.NpcNetDefaults.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.NpcNetDefaults.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnNpcSetDefaultsInt":
                    if (on) ServerApi.Hooks.NpcSetDefaultsInt.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.NpcSetDefaultsInt.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnNpcSetDefaultsString":
                    if (on) ServerApi.Hooks.NpcSetDefaultsString.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.NpcSetDefaultsString.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnNpcSpawn":
                    if (on) ServerApi.Hooks.NpcSpawn.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.NpcSpawn.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnNpcStrike":
                    if (on) ServerApi.Hooks.NpcStrike.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.NpcStrike.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnNpcTransform":
                    if (on) ServerApi.Hooks.NpcTransform.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.NpcTransform.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnNpcTriggerPressurePlate":
                    if (on) ServerApi.Hooks.NpcTriggerPressurePlate.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.NpcTriggerPressurePlate.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnPlayerTriggerPressurePlate":
                    if (on) ServerApi.Hooks.PlayerTriggerPressurePlate.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.PlayerTriggerPressurePlate.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnPlayerUpdatePhysics":
                    if (on) ServerApi.Hooks.PlayerUpdatePhysics.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.PlayerUpdatePhysics.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnProjectileAiUpdate":
                    if (on) ServerApi.Hooks.ProjectileAIUpdate.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.ProjectileAIUpdate.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnProjectileSetDefaults":
                    if (on) ServerApi.Hooks.ProjectileSetDefaults.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.ProjectileSetDefaults.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnProjectileTriggerPressurePlate":
                    if (on) ServerApi.Hooks.ProjectileTriggerPressurePlate.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.ProjectileTriggerPressurePlate.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnServerBroadcast":
                    if (on) ServerApi.Hooks.ServerBroadcast.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.ServerBroadcast.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnServerChat":
                    if (on) ServerApi.Hooks.ServerChat.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.ServerChat.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnServerCommand":
                    if (on) ServerApi.Hooks.ServerCommand.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.ServerCommand.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnConnect":
                    if (on) ServerApi.Hooks.ServerConnect.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.ServerConnect.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnJoin":
                    if (on) ServerApi.Hooks.ServerJoin.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.ServerJoin.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnLeave":
                    if (on) ServerApi.Hooks.ServerLeave.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.ServerLeave.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnServerSocketReset":
                    if (on) ServerApi.Hooks.ServerSocketReset.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.ServerSocketReset.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnAnnouncementBox":
                    if (on) ServerApi.Hooks.WireTriggerAnnouncementBox.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.WireTriggerAnnouncementBox.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnChristmasCheck":
                    if (on) ServerApi.Hooks.WorldChristmasCheck.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.WorldChristmasCheck.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnHalloweenCheck":
                    if (on) ServerApi.Hooks.WorldHalloweenCheck.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.WorldHalloweenCheck.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnMeteorDrop":
                    if (on) ServerApi.Hooks.WorldMeteorDrop.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.WorldMeteorDrop.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnWorldSave":
                    if (on) ServerApi.Hooks.WorldSave.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.WorldSave.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnStartHardMode":
                    if (on) ServerApi.Hooks.WorldStartHardMode.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.WorldStartHardMode.Deregister(LuaPlugin.Instance, Handler);
                    break;

                // TShockAPI.Hooks.AccountHooks
                case "OnAccountCreate":
                    if (on) AccountHooks.AccountCreate += Handler; else AccountHooks.AccountCreate -= Handler;
                    break;
                case "OnAccountDelete":
                    if (on) AccountHooks.AccountDelete += Handler; else AccountHooks.AccountDelete -= Handler;
                    break;

                // TShockAPI.Hooks.GeneralHooks
                case "OnReloadEvent":
                    if (on) GeneralHooks.ReloadEvent += Handler; else GeneralHooks.ReloadEvent -= Handler;
                    break;

                // TShockAPI.Hooks.PlayerHooks
                case "OnPlayerChat":
                    if (on) PlayerHooks.PlayerChat += Handler; else PlayerHooks.PlayerChat -= Handler;
                    break;
                case "OnPlayerCommand":
                    if (on) PlayerHooks.PlayerCommand += Handler; else PlayerHooks.PlayerCommand -= Handler;
                    break;
                case "OnPlayerLogout":
                    if (on) PlayerHooks.PlayerLogout += Handler; else PlayerHooks.PlayerLogout -= Handler;
                    break;
                case "OnPlayerPermission":
                    if (on) PlayerHooks.PlayerPermission += Handler; else PlayerHooks.PlayerPermission -= Handler;
                    break;
                case "OnPlayerPostLogin":
                    if (on) PlayerHooks.PlayerPostLogin += Handler; else PlayerHooks.PlayerPostLogin -= Handler;
                    break;
                case "OnPlayerPreLogin":
                    if (on) PlayerHooks.PlayerPreLogin += Handler; else PlayerHooks.PlayerPreLogin -= Handler;
                    break;

                // TShockAPI.Hooks.RegionHooks
                case "OnRegionCreated":
                    if (on) RegionHooks.RegionCreated += Handler; else RegionHooks.RegionCreated -= Handler;
                    break;
                case "OnRegionDeleted":
                    if (on) RegionHooks.RegionDeleted += Handler; else RegionHooks.RegionDeleted -= Handler;
                    break;
                case "OnRegionEntered":
                    if (on) RegionHooks.RegionEntered += Handler; else RegionHooks.RegionEntered -= Handler;
                    break;
                case "OnRegionLeft":
                    if (on) RegionHooks.RegionLeft += Handler; else RegionHooks.RegionLeft -= Handler;
                    break;

                // TShockAPI.TShock
                case "OnTShockInitialize":
                    if (on) TShock.Initialized += Handler; else TShock.Initialized -= Handler;
                    break;

                // TShockAPI.GetDataHandlers
                case "OnPacketChestItemChange":
                    if (on) GetDataHandlers.ChestItemChange += Handler; else GetDataHandlers.ChestItemChange -= Handler;
                    break;
                case "OnPacketChestOpen":
                    if (on) GetDataHandlers.ChestOpen += Handler; else GetDataHandlers.ChestOpen -= Handler;
                    break;
                case "OnPacketGemLockToggle":
                    if (on) GetDataHandlers.GemLockToggle += Handler; else GetDataHandlers.GemLockToggle -= Handler;
                    break;
                case "OnPacketItemDrop":
                    if (on) GetDataHandlers.ItemDrop += Handler; else GetDataHandlers.ItemDrop -= Handler;
                    break;
                case "OnPacketKillMe":
                    if (on) GetDataHandlers.KillMe += Handler; else GetDataHandlers.KillMe -= Handler;
                    break;
                case "OnPacketLiquidSet":
                    if (on) GetDataHandlers.LiquidSet += Handler; else GetDataHandlers.LiquidSet -= Handler;
                    break;
                case "OnPacketNewProjectile":
                    if (on) GetDataHandlers.NewProjectile += Handler; else GetDataHandlers.NewProjectile -= Handler;
                    break;
                case "OnPacketNPCHome":
                    if (on) GetDataHandlers.NPCHome += Handler; else GetDataHandlers.NPCHome -= Handler;
                    break;
                case "OnPacketNPCSpecial":
                    if (on) GetDataHandlers.NPCSpecial += Handler; else GetDataHandlers.NPCSpecial -= Handler;
                    break;
                case "OnPacketNPCStrike":
                    if (on) GetDataHandlers.NPCStrike += Handler; else GetDataHandlers.NPCStrike -= Handler;
                    break;
                case "OnPacketPaintTile":
                    if (on) GetDataHandlers.PaintTile += Handler; else GetDataHandlers.PaintTile -= Handler;
                    break;
                case "OnPacketPaintWall":
                    if (on) GetDataHandlers.PaintWall += Handler; else GetDataHandlers.PaintWall -= Handler;
                    break;
                case "OnPacketPlayerAnimation":
                    if (on) GetDataHandlers.PlayerAnimation += Handler; else GetDataHandlers.PlayerAnimation -= Handler;
                    break;
                case "OnPacketPlayerBuff":
                    if (on) GetDataHandlers.PlayerBuff += Handler; else GetDataHandlers.PlayerBuff -= Handler;
                    break;
                case "OnPacketPlayerBuffUpdate":
                    if (on) GetDataHandlers.PlayerBuffUpdate += Handler; else GetDataHandlers.PlayerBuffUpdate -= Handler;
                    break;
                case "OnPacketPlayerDamage":
                    if (on) GetDataHandlers.PlayerDamage += Handler; else GetDataHandlers.PlayerDamage -= Handler;
                    break;
                case "OnPacketPlayerHP":
                    if (on) GetDataHandlers.PlayerHP += Handler; else GetDataHandlers.PlayerHP -= Handler;
                    break;
                case "OnPacketPlayerInfo":
                    if (on) GetDataHandlers.PlayerInfo += Handler; else GetDataHandlers.PlayerInfo -= Handler;
                    break;
                case "OnPacketPlayerMana":
                    if (on) GetDataHandlers.PlayerMana += Handler; else GetDataHandlers.PlayerMana -= Handler;
                    break;
                case "OnPacketPlayerSlot":
                    if (on) GetDataHandlers.PlayerSlot += Handler; else GetDataHandlers.PlayerSlot -= Handler;
                    break;
                case "OnPacketPlayerSpawn":
                    if (on) GetDataHandlers.PlayerSpawn += Handler; else GetDataHandlers.PlayerSpawn -= Handler;
                    break;
                case "OnPacketPlayerTeam":
                    if (on) GetDataHandlers.PlayerTeam += Handler; else GetDataHandlers.PlayerTeam -= Handler;
                    break;
                case "OnPacketPlayerUpdate":
                    if (on) GetDataHandlers.PlayerUpdate += Handler; else GetDataHandlers.PlayerUpdate -= Handler;
                    break;
                case "OnPacketSendTileSquare":
                    if (on) GetDataHandlers.SendTileSquare += Handler; else GetDataHandlers.SendTileSquare -= Handler;
                    break;
                case "OnPacketSign":
                    if (on) GetDataHandlers.Sign += Handler; else GetDataHandlers.Sign -= Handler;
                    break;
                case "OnPacketTeleport":
                    if (on) GetDataHandlers.Teleport += Handler; else GetDataHandlers.Teleport -= Handler;
                    break;
                case "OnPacketTileEdit":
                    if (on) GetDataHandlers.TileEdit += Handler; else GetDataHandlers.TileEdit -= Handler;
                    break;
                case "OnPacketTileKill":
                    if (on) GetDataHandlers.TileKill += Handler; else GetDataHandlers.TileKill -= Handler;
                    break;
                case "OnPacketTogglePvp":
                    if (on) GetDataHandlers.TogglePvp += Handler; else GetDataHandlers.TogglePvp -= Handler;
                    break;*/
            };

            foreach (var hook in hooks)
                luaEnv.AddHook(hook);
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
