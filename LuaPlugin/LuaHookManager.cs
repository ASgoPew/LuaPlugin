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
            luaEnv.AddHook(new LuaHookHandler<Action>(luaEnv, "OnTick", (hook, state) =>
            {
                if (state == true) Main.OnTick += hook.Handler;
                else if (state == false) Main.OnTick -= hook.Handler;
                else hook.Handler = () => hook.Invoke();
            }));

            // ServerApi.Hooks hooks
            AddHookHandlerHook(luaEnv, "OnDropBossBag", ServerApi.Hooks.DropBossBag);
            AddHookHandlerHook(luaEnv, "OnHardmodeTileUpdate", ServerApi.Hooks.GameHardmodeTileUpdate);
            AddHookHandlerHook(luaEnv, "OnGameInitialize", ServerApi.Hooks.GameInitialize);
            AddHookHandlerHook(luaEnv, "OnGamePostInitialize", ServerApi.Hooks.GamePostInitialize);
            AddHookHandlerHook(luaEnv, "OnGamePostUpdate", ServerApi.Hooks.GamePostUpdate);
            AddHookHandlerHook(luaEnv, "OnStatueSpawn", ServerApi.Hooks.GameStatueSpawn);
            AddHookHandlerHook(luaEnv, "OnGameUpdate", ServerApi.Hooks.GameUpdate);
            AddHookHandlerHook(luaEnv, "OnGameWorldConnect", ServerApi.Hooks.GameWorldConnect);
            AddHookHandlerHook(luaEnv, "OnGameWorldDisconnect", ServerApi.Hooks.GameWorldDisconnect);
            AddHookHandlerHook(luaEnv, "OnForceItemIntoChest", ServerApi.Hooks.ItemForceIntoChest);
            AddHookHandlerHook(luaEnv, "OnItemNetDefaults", ServerApi.Hooks.ItemNetDefaults);
            AddHookHandlerHook(luaEnv, "OnItemSetDefualtsInt", ServerApi.Hooks.ItemSetDefaultsInt);
            AddHookHandlerHook(luaEnv, "OnItemSetDefaultsString", ServerApi.Hooks.ItemSetDefaultsString);
            AddHookHandlerHook(luaEnv, "OnGetData", ServerApi.Hooks.NetGetData);
            AddHookHandlerHook(luaEnv, "OnGreetPlayer", ServerApi.Hooks.NetGreetPlayer);
            AddHookHandlerHook(luaEnv, "OnNameCollision", ServerApi.Hooks.NetNameCollision);
            AddHookHandlerHook(luaEnv, "OnSendBytes", ServerApi.Hooks.NetSendBytes);
            AddHookHandlerHook(luaEnv, "OnSendData", ServerApi.Hooks.NetSendData);
            AddHookHandlerHook(luaEnv, "OnNpcAiUpdate", ServerApi.Hooks.NetSendData);
            AddHookHandlerHook(luaEnv, "OnNpcKilled", ServerApi.Hooks.NpcKilled);
            AddHookHandlerHook(luaEnv, "OnNpcLootDrop", ServerApi.Hooks.NpcLootDrop);
            AddHookHandlerHook(luaEnv, "OnNpcNetDefaults", ServerApi.Hooks.NpcNetDefaults);
            AddHookHandlerHook(luaEnv, "OnNpcSetDefaultsInt", ServerApi.Hooks.NpcSetDefaultsInt);
            AddHookHandlerHook(luaEnv, "OnNpcSetDefaultsString", ServerApi.Hooks.NpcSetDefaultsString);
            AddHookHandlerHook(luaEnv, "OnNpcSpawn", ServerApi.Hooks.NpcSpawn);
            AddHookHandlerHook(luaEnv, "OnNpcStrike", ServerApi.Hooks.NpcStrike);
            AddHookHandlerHook(luaEnv, "OnNpcTransform", ServerApi.Hooks.NpcTransform);
            AddHookHandlerHook(luaEnv, "OnNpcTriggerPressurePlate", ServerApi.Hooks.NpcTriggerPressurePlate);
            AddHookHandlerHook(luaEnv, "OnPlayerTriggerPressurePlate", ServerApi.Hooks.PlayerTriggerPressurePlate);
            AddHookHandlerHook(luaEnv, "OnPlayerUpdatePhysics", ServerApi.Hooks.PlayerUpdatePhysics);
            AddHookHandlerHook(luaEnv, "OnProjectileAiUpdate", ServerApi.Hooks.ProjectileAIUpdate);
            AddHookHandlerHook(luaEnv, "OnProjectileSetDefaults", ServerApi.Hooks.ProjectileSetDefaults);
            AddHookHandlerHook(luaEnv, "OnProjectileTriggerPressurePlate", ServerApi.Hooks.ProjectileTriggerPressurePlate);
            AddHookHandlerHook(luaEnv, "OnServerBroadcast", ServerApi.Hooks.ServerBroadcast);
            AddHookHandlerHook(luaEnv, "OnServerChat", ServerApi.Hooks.ServerChat);
            AddHookHandlerHook(luaEnv, "OnServerCommand", ServerApi.Hooks.ServerCommand);
            AddHookHandlerHook(luaEnv, "OnServerConnect", ServerApi.Hooks.ServerConnect);
            AddHookHandlerHook(luaEnv, "OnServerJoin", ServerApi.Hooks.ServerJoin);
            AddHookHandlerHook(luaEnv, "ServerLeave", ServerApi.Hooks.ServerLeave);
            AddHookHandlerHook(luaEnv, "OnServerSocketReset", ServerApi.Hooks.ServerSocketReset);
            AddHookHandlerHook(luaEnv, "OnWireTriggerAnnouncementBox", ServerApi.Hooks.WireTriggerAnnouncementBox);
            AddHookHandlerHook(luaEnv, "OnWorldChristmasCheck", ServerApi.Hooks.WorldChristmasCheck);
            AddHookHandlerHook(luaEnv, "OnWorldHalloweenCheck", ServerApi.Hooks.WorldHalloweenCheck);
            AddHookHandlerHook(luaEnv, "OnWorldMeteorDrop", ServerApi.Hooks.WorldMeteorDrop);
            AddHookHandlerHook(luaEnv, "OnWorldSave", ServerApi.Hooks.WorldSave);
            AddHookHandlerHook(luaEnv, "OnWorldStartHardMode", ServerApi.Hooks.WorldStartHardMode);

            // TShockAPI.Hooks.AccountHooks
            luaEnv.AddEventHook("OnAccountCreate", typeof(AccountHooks), "AccountCreate");
            luaEnv.AddEventHook("OnAccountDelete", typeof(AccountHooks), "AccountDelete");

            // TShockAPI.Hooks.GeneralHooks
            luaEnv.AddEventHook("OnReloadEvent", typeof(GeneralHooks), "ReloadEvent");

            // TShockAPI.Hooks.PlayerHooks
            luaEnv.AddEventHook("OnPlayerChat", typeof(PlayerHooks), "PlayerChat");
            luaEnv.AddEventHook("OnPlayerCommand", typeof(PlayerHooks), "PlayerCommand");
            luaEnv.AddEventHook("OnPlayerLogout", typeof(PlayerHooks), "PlayerLogout");
            luaEnv.AddEventHook("OnPlayerPermission", typeof(PlayerHooks), "PlayerPermission");
            luaEnv.AddEventHook("OnPlayerPostLogin", typeof(PlayerHooks), "PlayerPostLogin");
            luaEnv.AddEventHook("OnPlayerPreLogin", typeof(PlayerHooks), "PlayerPreLogin");

            // TShockAPI.Hooks.RegionHooks
            luaEnv.AddEventHook("OnRegionCreated", typeof(RegionHooks), "RegionCreated");
            luaEnv.AddEventHook("OnRegionDeleted", typeof(RegionHooks), "RegionDeleted");
            luaEnv.AddEventHook("OnRegionEntered", typeof(RegionHooks), "RegionEntered");
            luaEnv.AddEventHook("OnRegionLeft", typeof(RegionHooks), "RegionLeft");

            // TShockAPI.TShock
            luaEnv.AddHook(new LuaHookHandler<Action>(luaEnv, "OnTShockInitialize", (hook, state) =>
            {
                if      (state ==  true) TShock.Initialized += hook.Handler;
                else if (state == false) TShock.Initialized -= hook.Handler;
                else hook.Handler = () => hook.Invoke();
            }));

            // TShockAPI.GetDataHandlers
            AddEventHandlerHook(luaEnv, "OnPacketChestItemChange", ref GetDataHandlers.ChestItemChange);
            AddEventHandlerHook(luaEnv, "OnPacketChestOpen", ref GetDataHandlers.ChestOpen);
            AddEventHandlerHook(luaEnv, "OnPacketGemLockToggle", ref GetDataHandlers.GemLockToggle);
            AddEventHandlerHook(luaEnv, "OnPacketItemDrop", ref GetDataHandlers.ItemDrop);
            AddEventHandlerHook(luaEnv, "OnPacketKillMe", ref GetDataHandlers.KillMe);
            AddEventHandlerHook(luaEnv, "OnPacketLiquidSet", ref GetDataHandlers.LiquidSet);
            AddEventHandlerHook(luaEnv, "OnPacketNewProjectile", ref GetDataHandlers.NewProjectile);
            AddEventHandlerHook(luaEnv, "OnPacketNPCHome", ref GetDataHandlers.NPCHome);
            AddEventHandlerHook(luaEnv, "OnPacketNPCSpecial", ref GetDataHandlers.NPCSpecial);
            AddEventHandlerHook(luaEnv, "OnPacketNPCStrike", ref GetDataHandlers.NPCStrike);
            AddEventHandlerHook(luaEnv, "OnPacketPaintTile", ref GetDataHandlers.PaintTile);
            AddEventHandlerHook(luaEnv, "OnPacketPaintWall", ref GetDataHandlers.PaintWall);
            AddEventHandlerHook(luaEnv, "OnPacketPlayerAnimation", ref GetDataHandlers.PlayerAnimation);
            AddEventHandlerHook(luaEnv, "OnPacketPlayerBuff", ref GetDataHandlers.PlayerBuff);
            AddEventHandlerHook(luaEnv, "OnPacketPlayerBuffUpdate", ref GetDataHandlers.PlayerBuffUpdate);
            AddEventHandlerHook(luaEnv, "OnPacketPlayerDamage", ref GetDataHandlers.PlayerDamage);
            AddEventHandlerHook(luaEnv, "OnPacketPlayerHP", ref GetDataHandlers.PlayerHP);
            AddEventHandlerHook(luaEnv, "OnPacketPlayerInfo", ref GetDataHandlers.PlayerInfo);
            AddEventHandlerHook(luaEnv, "OnPacketPlayerMana", ref GetDataHandlers.PlayerMana);
            AddEventHandlerHook(luaEnv, "OnPacketPlayerSlot", ref GetDataHandlers.PlayerSlot);
            AddEventHandlerHook(luaEnv, "OnPacketPlayerSpawn", ref GetDataHandlers.PlayerSpawn);
            AddEventHandlerHook(luaEnv, "OnPacketPlayerTeam", ref GetDataHandlers.PlayerTeam);
            AddEventHandlerHook(luaEnv, "OnPacketPlayerUpdate", ref GetDataHandlers.PlayerUpdate);
            AddEventHandlerHook(luaEnv, "OnPacketSendTileSquare", ref GetDataHandlers.SendTileSquare);
            AddEventHandlerHook(luaEnv, "OnPacketSign", ref GetDataHandlers.Sign);
            AddEventHandlerHook(luaEnv, "OnPacketTeleport", ref GetDataHandlers.Teleport);
            AddEventHandlerHook(luaEnv, "OnPacketTileEdit", ref GetDataHandlers.TileEdit);
            AddEventHandlerHook(luaEnv, "OnPacketTileKill", ref GetDataHandlers.TileKill);
            AddEventHandlerHook(luaEnv, "OnPacketTogglePvp", ref GetDataHandlers.TogglePvp);
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
