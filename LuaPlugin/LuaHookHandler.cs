using NLua;
using OTAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace LuaPlugin
{
    public class LuaHookHandler
    {
        //public delegate void HookHandler();
        //public delegate void HookHandler<T>(T args);
        //public delegate void HookHandler<T, U>(T sender, U args);

        public LuaEnvironment luaEnv;
        public string name;
        public bool active = false;

        dynamic handler = null;

        public void OnHook(object arg0 = null, object arg1 = null)
        {
            //luaEnv.Set("source", this, "OnHook"); // Why does this line increases ping linearly?
            //luaEnv.Set("source", name, "OnHook"); // And this one works fine!
            object oldSource = luaEnv.data["source"];
            
            if (luaEnv.CallFunction(name, (lua, state) => {
                if (state == 0)
                    luaEnv.data["source"] = this;
                else if (state > 0)
                    luaEnv.data["source"] = oldSource;
            }, $"OnHook ({name})", arg0, arg1) == null)
                luaEnv.Unhook(name);
        }

        public bool HasPermission(string permissions)
        {
            return true;
        }

        public HookResult OnHookWithResult(object arg0 = null, object arg1 = null)
        {
            throw new NotImplementedException("OTAPI.Hooks not implemented");
            /*try
            {
                if (LuaPlugin.lua.GetFunction(name) != null)
                {
                    object[] result = LuaPlugin.lua.GetFunction(name).Call(arg0, arg1);
                    if (result != null && result.Length == 1)
                        return (bool)result[0] ? HookResult.Cancel : HookResult.Continue;
                    else
                    {
                        //Unhook();
                        //Console.WriteLine("Incorrect HookResult return: " + handlerFunctionName);
                        return HookResult.Continue;
                    }
                }
                else
                {
                    Unhook();
                    Console.WriteLine("Stopped " + name);
                    return HookResult.Continue;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(name + " ERROR: " + e);
                LuaPlugin.lua[name] = null;
                Unhook();
                return HookResult.Continue;
            }*/
        }

        public void Hook()
        {
            //Console.WriteLine("Hooking " + name);
            active = true;
            Control(true);
        }

        public void Unhook()
        {
            //Console.WriteLine("Unhooking " + name);
            Control(false);
            active = false;
        }

        public void Update()
        {
            using (LuaFunction f = luaEnv.Get(name) as LuaFunction)
            {
                if (f != null && !active)
                    Hook();
                else if (f == null && active)
                    Unhook();
            }
        }

        public static List<string> HandlerNames = new List<string>()
        {
            "OnTick",
            "OnDropBossBag","OnHardmodeTileUpdate","OnGameInitialize","OnGamePostInitialize","OnGamePostUpdate","OnStatueSpawn","OnGameUpdate","OnGameWorldConnect","OnGameWorldDisconnect","OnForceItemIntoChest",
            "OnItemNetDefaults","OnItemSetDefualtsInt","OnItemSetDefaultsString","OnGetData","OnGreetPlayer","OnNameCollision","OnSendBytes","OnSendData","OnNpcAiUpdate","OnNpcKilled","OnNpcLootDrop","OnNpcNetDefaults",
            "OnNpcSetDefaultsInt","OnNpcSetDefaultsString","OnNpcSpawn","OnNpcStrike","OnNpcTransform","OnNpcTriggerPressurePlate","OnPlayerTriggerPressurePlate","OnPlayerUpdatePhysics","OnProjectileAiUpdate",
            "OnProjectileSetDefaults","OnProjectileTriggerPressurePlate","OnServerBroadcast","OnServerChat","OnServerCommand","OnConnect","OnJoin","OnLeave","OnServerSocketReset","OnAnnouncementBox","OnChristmasCheck",
            "OnHalloweenCheck","OnMeteorDrop","OnWorldSave","OnStartHardMode",

            "OnAccountCreate","OnAccountDelete","OnReloadEvent","OnPlayerChat","OnPlayerCommand","OnPlayerLogout","OnPlayerPermission","OnPlayerPostLogin","OnPlayerPreLogin","OnRegionCreated","OnRegionDeleted",
            "OnRegionEntered","OnRegionLeft","OnTShockInitialize",

            "OnPacketChestItemChange","OnPacketChestOpen","OnPacketGemLockToggle","OnPacketItemDrop","OnPacketKillMe","OnPacketLiquidSet","OnPacketNewProjectile","OnPacketNPCHome","OnPacketNPCSpecial","OnPacketNPCStrike",
            "OnPacketPaintTile","OnPacketPaintWall","OnPacketPlayerAnimation","OnPacketPlayerBuff","OnPacketPlayerBuffUpdate","OnPacketPlayerDamage","OnPacketPlayerHP","OnPacketPlayerInfo","OnPacketPlayerMana",
            "OnPacketPlayerSlot","OnPacketPlayerSpawn","OnPacketPlayerTeam","OnPacketPlayerUpdate","OnPacketSendTileSquare","OnPacketSign","OnPacketTeleport","OnPacketTileEdit","OnPacketTileKill","OnPacketTogglePvp",

            //"OnProjectilePostKill"
        };

        /*
        case "On":
            handler = new 
            handlerControl = (Action<bool>)((on) => { if (on)  else  });
            break;
        
        case "On":
            handler = new HookHandler<EventArgs>((EventArgs args) => { OnHook(name, args); });
            handlerControl = (Action<bool>)((on) => { if (on) ServerApi.Hooks.DropBossBag.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.DropBossBag.Deregister(LuaPlugin.instance, handler); });
            break;
        */

        public void Control(bool on)
        {
            switch (name)
            {
                // Main hooks
                case "OnTick":
                    if (on) Main.OnTick += handler; else Main.OnTick -= handler;
                    break;
                
                // ServerApi.Hooks hooks
                case "OnDropBossBag":
                    if (on) ServerApi.Hooks.DropBossBag.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.DropBossBag.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnHardmodeTileUpdate":
                    if (on) ServerApi.Hooks.GameHardmodeTileUpdate.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.GameHardmodeTileUpdate.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnGameInitialize":
                    if (on) ServerApi.Hooks.GameInitialize.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.GameInitialize.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnGamePostInitialize":
                    if (on) ServerApi.Hooks.GamePostInitialize.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.GamePostInitialize.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnGamePostUpdate":
                    if (on) ServerApi.Hooks.GamePostUpdate.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.GamePostUpdate.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnStatueSpawn":
                    if (on) ServerApi.Hooks.GameStatueSpawn.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.GameStatueSpawn.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnGameUpdate":
                    if (on) ServerApi.Hooks.GameUpdate.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.GameUpdate.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnGameWorldConnect":
                    if (on) ServerApi.Hooks.GameWorldConnect.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.GameWorldConnect.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnGameWorldDisconnect":
                    if (on) ServerApi.Hooks.GameWorldDisconnect.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.GameWorldDisconnect.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnForceItemIntoChest":
                    if (on) ServerApi.Hooks.ItemForceIntoChest.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.ItemForceIntoChest.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnItemNetDefaults":
                    if (on) ServerApi.Hooks.ItemNetDefaults.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.ItemNetDefaults.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnItemSetDefualtsInt":
                    if (on) ServerApi.Hooks.ItemSetDefaultsInt.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.ItemSetDefaultsInt.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnItemSetDefaultsString":
                    if (on) ServerApi.Hooks.ItemSetDefaultsString.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.ItemSetDefaultsString.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnGetData":
                    if (on) ServerApi.Hooks.NetGetData.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.NetGetData.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnGreetPlayer":
                    if (on) ServerApi.Hooks.NetGreetPlayer.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.NetGreetPlayer.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnNameCollision":
                    if (on) ServerApi.Hooks.NetNameCollision.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.NetNameCollision.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnSendBytes":
                    if (on) ServerApi.Hooks.NetSendBytes.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.NetSendBytes.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnSendData":
                    if (on) ServerApi.Hooks.NetSendData.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.NetSendData.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnNpcAiUpdate":
                    if (on) ServerApi.Hooks.NpcAIUpdate.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.NpcAIUpdate.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnNpcKilled":
                    if (on) ServerApi.Hooks.NpcKilled.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.NpcKilled.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnNpcLootDrop":
                    if (on) ServerApi.Hooks.NpcLootDrop.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.NpcLootDrop.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnNpcNetDefaults":
                    if (on) ServerApi.Hooks.NpcNetDefaults.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.NpcNetDefaults.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnNpcSetDefaultsInt":
                    if (on) ServerApi.Hooks.NpcSetDefaultsInt.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.NpcSetDefaultsInt.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnNpcSetDefaultsString":
                    if (on) ServerApi.Hooks.NpcSetDefaultsString.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.NpcSetDefaultsString.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnNpcSpawn":
                    if (on) ServerApi.Hooks.NpcSpawn.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.NpcSpawn.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnNpcStrike":
                    if (on) ServerApi.Hooks.NpcStrike.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.NpcStrike.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnNpcTransform":
                    if (on) ServerApi.Hooks.NpcTransform.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.NpcTransform.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnNpcTriggerPressurePlate":
                    if (on) ServerApi.Hooks.NpcTriggerPressurePlate.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.NpcTriggerPressurePlate.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnPlayerTriggerPressurePlate":
                    if (on) ServerApi.Hooks.PlayerTriggerPressurePlate.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.PlayerTriggerPressurePlate.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnPlayerUpdatePhysics":
                    if (on) ServerApi.Hooks.PlayerUpdatePhysics.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.PlayerUpdatePhysics.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnProjectileAiUpdate":
                    if (on) ServerApi.Hooks.ProjectileAIUpdate.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.ProjectileAIUpdate.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnProjectileSetDefaults":
                    if (on) ServerApi.Hooks.ProjectileSetDefaults.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.ProjectileSetDefaults.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnProjectileTriggerPressurePlate":
                    if (on) ServerApi.Hooks.ProjectileTriggerPressurePlate.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.ProjectileTriggerPressurePlate.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnServerBroadcast":
                    if (on) ServerApi.Hooks.ServerBroadcast.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.ServerBroadcast.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnServerChat":
                    if (on) ServerApi.Hooks.ServerChat.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.ServerChat.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnServerCommand":
                    if (on) ServerApi.Hooks.ServerCommand.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.ServerCommand.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnConnect":
                    if (on) ServerApi.Hooks.ServerConnect.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.ServerConnect.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnJoin":
                    if (on) ServerApi.Hooks.ServerJoin.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.ServerJoin.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnLeave":
                    if (on) ServerApi.Hooks.ServerLeave.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.ServerLeave.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnServerSocketReset":
                    if (on) ServerApi.Hooks.ServerSocketReset.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.ServerSocketReset.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnAnnouncementBox":
                    if (on) ServerApi.Hooks.WireTriggerAnnouncementBox.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.WireTriggerAnnouncementBox.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnChristmasCheck":
                    if (on) ServerApi.Hooks.WorldChristmasCheck.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.WorldChristmasCheck.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnHalloweenCheck":
                    if (on) ServerApi.Hooks.WorldHalloweenCheck.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.WorldHalloweenCheck.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnMeteorDrop":
                    if (on) ServerApi.Hooks.WorldMeteorDrop.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.WorldMeteorDrop.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnWorldSave":
                    if (on) ServerApi.Hooks.WorldSave.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.WorldSave.Deregister(LuaPlugin.instance, handler);
                    break;
                case "OnStartHardMode":
                    if (on) ServerApi.Hooks.WorldStartHardMode.Register(LuaPlugin.instance, handler); else ServerApi.Hooks.WorldStartHardMode.Deregister(LuaPlugin.instance, handler);
                    break;

                // TShockAPI.Hooks.AccountHooks
                case "OnAccountCreate":
                    if (on) AccountHooks.AccountCreate += handler; else AccountHooks.AccountCreate -= handler;
                    break;
                case "OnAccountDelete":
                    if (on) AccountHooks.AccountDelete += handler; else AccountHooks.AccountDelete -= handler;
                    break;

                // TShockAPI.Hooks.GeneralHooks
                case "OnReloadEvent":
                    if (on) GeneralHooks.ReloadEvent += handler; else GeneralHooks.ReloadEvent -= handler;
                    break;

                // TShockAPI.Hooks.PlayerHooks
                case "OnPlayerChat":
                    if (on) PlayerHooks.PlayerChat += handler; else PlayerHooks.PlayerChat -= handler;
                    break;
                case "OnPlayerCommand":
                    if (on) PlayerHooks.PlayerCommand += handler; else PlayerHooks.PlayerCommand -= handler;
                    break;
                case "OnPlayerLogout":
                    if (on) PlayerHooks.PlayerLogout += handler; else PlayerHooks.PlayerLogout -= handler;
                    break;
                case "OnPlayerPermission":
                    if (on) PlayerHooks.PlayerPermission += handler; else PlayerHooks.PlayerPermission -= handler;
                    break;
                case "OnPlayerPostLogin":
                    if (on) PlayerHooks.PlayerPostLogin += handler; else PlayerHooks.PlayerPostLogin -= handler;
                    break;
                case "OnPlayerPreLogin":
                    if (on) PlayerHooks.PlayerPreLogin += handler; else PlayerHooks.PlayerPreLogin -= handler;
                    break;

                // TShockAPI.Hooks.RegionHooks
                case "OnRegionCreated":
                    if (on) RegionHooks.RegionCreated += handler; else RegionHooks.RegionCreated -= handler;
                    break;
                case "OnRegionDeleted":
                    if (on) RegionHooks.RegionDeleted += handler; else RegionHooks.RegionDeleted -= handler;
                    break;
                case "OnRegionEntered":
                    if (on) RegionHooks.RegionEntered += handler; else RegionHooks.RegionEntered -= handler;
                    break;
                case "OnRegionLeft":
                    if (on) RegionHooks.RegionLeft += handler; else RegionHooks.RegionLeft -= handler;
                    break;

                // TShockAPI.TShock
                case "OnTShockInitialize":
                    if (on) TShock.Initialized += handler; else TShock.Initialized -= handler;
                    break;

                // TShockAPI.GetDataHandlers
                case "OnPacketChestItemChange":
                    if (on) GetDataHandlers.ChestItemChange += handler; else GetDataHandlers.ChestItemChange -= handler;
                    break;
                case "OnPacketChestOpen":
                    if (on) GetDataHandlers.ChestOpen += handler; else GetDataHandlers.ChestOpen -= handler;
                    break;
                case "OnPacketGemLockToggle":
                    if (on) GetDataHandlers.GemLockToggle += handler; else GetDataHandlers.GemLockToggle -= handler;
                    break;
                case "OnPacketItemDrop":
                    if (on) GetDataHandlers.ItemDrop += handler; else GetDataHandlers.ItemDrop -= handler;
                    break;
                case "OnPacketKillMe":
                    if (on) GetDataHandlers.KillMe += handler; else GetDataHandlers.KillMe -= handler;
                    break;
                case "OnPacketLiquidSet":
                    if (on) GetDataHandlers.LiquidSet += handler; else GetDataHandlers.LiquidSet -= handler;
                    break;
                case "OnPacketNewProjectile":
                    if (on) GetDataHandlers.NewProjectile += handler; else GetDataHandlers.NewProjectile -= handler;
                    break;
                case "OnPacketNPCHome":
                    if (on) GetDataHandlers.NPCHome += handler; else GetDataHandlers.NPCHome -= handler;
                    break;
                case "OnPacketNPCSpecial":
                    if (on) GetDataHandlers.NPCSpecial += handler; else GetDataHandlers.NPCSpecial -= handler;
                    break;
                case "OnPacketNPCStrike":
                    if (on) GetDataHandlers.NPCStrike += handler; else GetDataHandlers.NPCStrike -= handler;
                    break;
                case "OnPacketPaintTile":
                    if (on) GetDataHandlers.PaintTile += handler; else GetDataHandlers.PaintTile -= handler;
                    break;
                case "OnPacketPaintWall":
                    if (on) GetDataHandlers.PaintWall += handler; else GetDataHandlers.PaintWall -= handler;
                    break;
                case "OnPacketPlayerAnimation":
                    if (on) GetDataHandlers.PlayerAnimation += handler; else GetDataHandlers.PlayerAnimation -= handler;
                    break;
                case "OnPacketPlayerBuff":
                    if (on) GetDataHandlers.PlayerBuff += handler; else GetDataHandlers.PlayerBuff -= handler;
                    break;
                case "OnPacketPlayerBuffUpdate":
                    if (on) GetDataHandlers.PlayerBuffUpdate += handler; else GetDataHandlers.PlayerBuffUpdate -= handler;
                    break;
                case "OnPacketPlayerDamage":
                    if (on) GetDataHandlers.PlayerDamage += handler; else GetDataHandlers.PlayerDamage -= handler;
                    break;
                case "OnPacketPlayerHP":
                    if (on) GetDataHandlers.PlayerHP += handler; else GetDataHandlers.PlayerHP -= handler;
                    break;
                case "OnPacketPlayerInfo":
                    if (on) GetDataHandlers.PlayerInfo += handler; else GetDataHandlers.PlayerInfo -= handler;
                    break;
                case "OnPacketPlayerMana":
                    if (on) GetDataHandlers.PlayerMana += handler; else GetDataHandlers.PlayerMana -= handler;
                    break;
                case "OnPacketPlayerSlot":
                    if (on) GetDataHandlers.PlayerSlot += handler; else GetDataHandlers.PlayerSlot -= handler;
                    break;
                case "OnPacketPlayerSpawn":
                    if (on) GetDataHandlers.PlayerSpawn += handler; else GetDataHandlers.PlayerSpawn -= handler;
                    break;
                case "OnPacketPlayerTeam":
                    if (on) GetDataHandlers.PlayerTeam += handler; else GetDataHandlers.PlayerTeam -= handler;
                    break;
                case "OnPacketPlayerUpdate":
                    if (on) GetDataHandlers.PlayerUpdate += handler; else GetDataHandlers.PlayerUpdate -= handler;
                    break;
                case "OnPacketSendTileSquare":
                    if (on) GetDataHandlers.SendTileSquare += handler; else GetDataHandlers.SendTileSquare -= handler;
                    break;
                case "OnPacketSign":
                    if (on) GetDataHandlers.Sign += handler; else GetDataHandlers.Sign -= handler;
                    break;
                case "OnPacketTeleport":
                    if (on) GetDataHandlers.Teleport += handler; else GetDataHandlers.Teleport -= handler;
                    break;
                case "OnPacketTileEdit":
                    if (on) GetDataHandlers.TileEdit += handler; else GetDataHandlers.TileEdit -= handler;
                    break;
                case "OnPacketTileKill":
                    if (on) GetDataHandlers.TileKill += handler; else GetDataHandlers.TileKill -= handler;
                    break;
                case "OnPacketTogglePvp":
                    if (on) GetDataHandlers.TogglePvp += handler; else GetDataHandlers.TogglePvp -= handler;
                    break;

                // TODO???: OTAPI.Hooks
                /*case "OnProjectilePreKill":
                    handler = new OTAPI.Modifications.NetworkText.AfterChatMessageHandler((text, color, ignore) => { return OnHookWithResult(); });
                    handlerControl = (Action<bool>)((on) => { if (on) OTAPI.Hooks.BroadcastChatMessage.AfterBroadcastChatMessage += handler; else OTAPI.Hooks.BroadcastChatMessage.AfterBroadcastChatMessage -= handler; });
                    OTAPI.Hooks.BroadcastChatMessage.AfterBroadcastChatMessage += new Action(() => { });
                    break;
                case "OnProjectilePreKill":
                    handler = new OTAPI.Hooks.Projectile.PreKillHandler((Projectile p) => { return OnHookWithResult(p); });
                    handlerControl = (Action<bool>)((on) => { if (on) OTAPI.Hooks.Projectile.PreKill += handler; else OTAPI.Hooks.Projectile.PreKill -= handler; });
                    break;*/

                /*case "OnProjectilePostKill":
                    if (on) OTAPI.Hooks.Projectile.PostKilled += handler; else OTAPI.Hooks.Projectile.PostKilled -= handler;
                    break;*/

                default:
                    luaEnv.PrintError("Trying to create unknown hook!");
                    break;
            }
        }

        public LuaHookHandler(LuaEnvironment newLuaEnv, string newName)
        {
            luaEnv = newLuaEnv;
            name = newName;
            switch (name)
            {
                // Main hooks
                case "OnTick":
                    handler = new Action(() => { OnHook(Main.netPlayCounter); });
                    break;
                
                // ServerApi.Hooks hooks
                case "OnDropBossBag":
                    handler = new HookHandler<DropBossBagEventArgs>((DropBossBagEventArgs args) => { OnHook(args); });
                    break;
                case "OnHardmodeTileUpdate":
                    handler = new HookHandler<HardmodeTileUpdateEventArgs>((HardmodeTileUpdateEventArgs args) => { OnHook(args); });
                    break;
                case "OnGameInitialize":
                    handler = new HookHandler<EventArgs>((EventArgs args) => { OnHook(args); });
                    break;
                case "OnGamePostInitialize":
                    handler = new HookHandler<EventArgs>((EventArgs args) => { OnHook(args); });
                    break;
                case "OnGamePostUpdate":
                    handler = new HookHandler<EventArgs>((EventArgs args) => { OnHook(args); });
                    break;
                case "OnStatueSpawn":
                    handler = new HookHandler<StatueSpawnEventArgs>((StatueSpawnEventArgs args) => { OnHook(args); });
                    break;
                case "OnGameUpdate":
                    handler = new HookHandler<EventArgs>((EventArgs args) => { OnHook(args); });
                    break;
                case "OnGameWorldConnect":
                    handler = new HookHandler<EventArgs>((EventArgs args) => { OnHook(args); });
                    break;
                case "OnGameWorldDisconnect":
                    handler = new HookHandler<EventArgs>((EventArgs args) => { OnHook(args); });
                    break;
                case "OnForceItemIntoChest":
                    handler = new HookHandler<ForceItemIntoChestEventArgs>((ForceItemIntoChestEventArgs args) => { OnHook(args); });
                    break;
                case "OnItemNetDefaults":
                    handler = new HookHandler<SetDefaultsEventArgs<Item, int>>((SetDefaultsEventArgs<Item, int> args) => { OnHook(args); });
                    break;
                case "OnItemSetDefualtsInt":
                    handler = new HookHandler<SetDefaultsEventArgs<Item, int>>((SetDefaultsEventArgs<Item, int> args) => { OnHook(args); });
                    break;
                case "OnItemSetDefaultsString":
                    handler = new HookHandler<SetDefaultsEventArgs<Item, string>>((SetDefaultsEventArgs<Item, string> args) => { OnHook(args); });
                    break;
                case "OnGetData":
                    handler = new HookHandler<GetDataEventArgs>((GetDataEventArgs args) => { OnHook(args); });
                    break;
                case "OnGreetPlayer":
                    handler = new HookHandler<GreetPlayerEventArgs>((GreetPlayerEventArgs args) => { OnHook(args); });
                    break;
                case "OnNameCollision":
                    handler = new HookHandler<NameCollisionEventArgs>((NameCollisionEventArgs args) => { OnHook(args); });
                    break;
                case "OnSendBytes":
                    handler = new HookHandler<SendBytesEventArgs>((SendBytesEventArgs args) => { OnHook(args); });
                    break;
                case "OnSendData":
                    handler = new HookHandler<SendDataEventArgs>((SendDataEventArgs args) => { OnHook(args); });
                    break;
                case "OnNpcAiUpdate":
                    handler = new HookHandler<NpcAiUpdateEventArgs>((NpcAiUpdateEventArgs args) => { OnHook(args); });
                    break;
                case "OnNpcKilled":
                    handler = new HookHandler<NpcKilledEventArgs>((NpcKilledEventArgs args) => { OnHook(args); });
                    break;
                case "OnNpcLootDrop":
                    handler = new HookHandler<NpcLootDropEventArgs>((NpcLootDropEventArgs args) => { OnHook(args); });
                    break;
                case "OnNpcNetDefaults":
                    handler = new HookHandler<SetDefaultsEventArgs<NPC, int>>((SetDefaultsEventArgs<NPC, int> args) => { OnHook(args); });
                    break;
                case "OnNpcSetDefaultsInt":
                    handler = new HookHandler<SetDefaultsEventArgs<NPC, int>>((SetDefaultsEventArgs<NPC, int> args) => { OnHook(args); });
                    break;
                case "OnNpcSetDefaultsString":
                    handler = new HookHandler<SetDefaultsEventArgs<NPC, string>>((SetDefaultsEventArgs<NPC, string> args) => { OnHook(args); });
                    break;
                case "OnNpcSpawn":
                    handler = new HookHandler<NpcSpawnEventArgs>((NpcSpawnEventArgs args) => { OnHook(args); });
                    break;
                case "OnNpcStrike":
                    handler = new HookHandler<NpcStrikeEventArgs>((NpcStrikeEventArgs args) => { OnHook(args); });
                    break;
                case "OnNpcTransform":
                    handler = new HookHandler<NpcTransformationEventArgs>((NpcTransformationEventArgs args) => { OnHook(args); });
                    break;
                case "OnNpcTriggerPressurePlate":
                    handler = new HookHandler<TriggerPressurePlateEventArgs<NPC>>((TriggerPressurePlateEventArgs<NPC> args) => { OnHook(args); });
                    break;
                case "OnPlayerTriggerPressurePlate":
                    handler = new HookHandler<TriggerPressurePlateEventArgs<Player>>((TriggerPressurePlateEventArgs<Player> args) => { OnHook(args); });
                    break;
                case "OnPlayerUpdatePhysics":
                    handler = new HookHandler<UpdatePhysicsEventArgs>((UpdatePhysicsEventArgs args) => { OnHook(args); });
                    break;
                case "OnProjectileAiUpdate":
                    handler = new HookHandler<ProjectileAiUpdateEventArgs>((ProjectileAiUpdateEventArgs args) => { OnHook(args); });
                    break;
                case "OnProjectileSetDefaults":
                    handler = new HookHandler<SetDefaultsEventArgs<Projectile, int>>((SetDefaultsEventArgs<Projectile, int> args) => { OnHook(args); });
                    break;
                case "OnProjectileTriggerPressurePlate":
                    handler = new HookHandler<TriggerPressurePlateEventArgs<Projectile>>((TriggerPressurePlateEventArgs<Projectile> args) => { OnHook(args); });
                    break;
                case "OnServerBroadcast":
                    handler = new HookHandler<ServerBroadcastEventArgs>((ServerBroadcastEventArgs args) => { OnHook(args); });
                    break;
                case "OnServerChat":
                    handler = new HookHandler<ServerChatEventArgs>((ServerChatEventArgs args) => { OnHook(args); });
                    break;
                case "OnServerCommand":
                    handler = new HookHandler<CommandEventArgs>((CommandEventArgs args) => { OnHook(args); });
                    break;
                case "OnConnect":
                    handler = new HookHandler<ConnectEventArgs>((ConnectEventArgs args) => { OnHook(args); });
                    break;
                case "OnJoin":
                    handler = new HookHandler<JoinEventArgs>((JoinEventArgs args) => { OnHook(args); });
                    break;
                case "OnLeave":
                    handler = new HookHandler<LeaveEventArgs>((LeaveEventArgs args) => { OnHook(args); });
                    break;
                case "OnServerSocketReset":
                    handler = new HookHandler<SocketResetEventArgs>((SocketResetEventArgs args) => { OnHook(args); });
                    break;
                case "OnAnnouncementBox":
                    handler = new HookHandler<TriggerAnnouncementBoxEventArgs>((TriggerAnnouncementBoxEventArgs args) => { OnHook(args); });
                    break;
                case "OnChristmasCheck":
                    handler = new HookHandler<ChristmasCheckEventArgs>((ChristmasCheckEventArgs args) => { OnHook(args); });
                    break;
                case "OnHalloweenCheck":
                    handler = new HookHandler<HalloweenCheckEventArgs>((HalloweenCheckEventArgs args) => { OnHook(args); });
                    break;
                case "OnMeteorDrop":
                    handler = new HookHandler<MeteorDropEventArgs>((MeteorDropEventArgs args) => { OnHook(args); });
                    break;
                case "OnWorldSave":
                    handler = new HookHandler<WorldSaveEventArgs>((WorldSaveEventArgs args) => { OnHook(args); });
                    break;
                case "OnStartHardMode":
                    handler = new HookHandler<HandledEventArgs>((HandledEventArgs args) => { OnHook(args); });
                    break;

                // TShockAPI.Hooks.AccountHooks
                case "OnAccountCreate":
                    handler = new AccountHooks.AccountCreateD((AccountCreateEventArgs args) => { OnHook(args); });
                    break;
                case "OnAccountDelete":
                    handler = new AccountHooks.AccountDeleteD((AccountDeleteEventArgs args) => { OnHook(args); });
                    break;

                // TShockAPI.Hooks.GeneralHooks
                case "OnReloadEvent":
                    handler = new GeneralHooks.ReloadEventD((ReloadEventArgs args) => { OnHook(args); });
                    break;

                // TShockAPI.Hooks.PlayerHooks
                case "OnPlayerChat":
                    handler = new PlayerHooks.PlayerChatD((PlayerChatEventArgs args) => { OnHook(args); });
                    break;
                case "OnPlayerCommand":
                    handler = new PlayerHooks.PlayerCommandD((PlayerCommandEventArgs args) => { OnHook(args); });
                    break;
                case "OnPlayerLogout":
                    handler = new PlayerHooks.PlayerLogoutD((PlayerLogoutEventArgs args) => { OnHook(args); });
                    break;
                case "OnPlayerPermission":
                    handler = new PlayerHooks.PlayerPermissionD((PlayerPermissionEventArgs args) => { OnHook(args); });
                    break;
                case "OnPlayerPostLogin":
                    handler = new PlayerHooks.PlayerPostLoginD((PlayerPostLoginEventArgs args) => { OnHook(args); });
                    break;
                case "OnPlayerPreLogin":
                    handler = new PlayerHooks.PlayerPreLoginD((PlayerPreLoginEventArgs args) => { OnHook(args); });
                    break;

                // TShockAPI.Hooks.RegionHooks
                case "OnRegionCreated":
                    handler = new RegionHooks.RegionCreatedD((RegionHooks.RegionCreatedEventArgs args) => { OnHook(args); });
                    break;
                case "OnRegionDeleted":
                    handler = new RegionHooks.RegionDeletedD((RegionHooks.RegionDeletedEventArgs args) => { OnHook(args); });
                    break;
                case "OnRegionEntered":
                    handler = new RegionHooks.RegionEnteredD((RegionHooks.RegionEnteredEventArgs args) => { OnHook(args); });
                    break;
                case "OnRegionLeft":
                    handler = new RegionHooks.RegionLeftD((RegionHooks.RegionLeftEventArgs args) => { OnHook(args); });
                    break;

                // TShockAPI.TShock
                case "OnTShockInitialize":
                    handler = new Action(() => { OnHook(); });
                    break;

                // TShockAPI.GetDataHandlers
                case "OnPacketChestItemChange":
                    handler = new EventHandler<GetDataHandlers.ChestItemEventArgs>((object sender, GetDataHandlers.ChestItemEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketChestOpen":
                    handler = new EventHandler<GetDataHandlers.ChestOpenEventArgs>((object sender, GetDataHandlers.ChestOpenEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketGemLockToggle":
                    handler = new EventHandler<GetDataHandlers.GemLockToggleEventArgs>((object sender, GetDataHandlers.GemLockToggleEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketItemDrop":
                    handler = new EventHandler<GetDataHandlers.ItemDropEventArgs>((object sender, GetDataHandlers.ItemDropEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketKillMe":
                    handler = new EventHandler<GetDataHandlers.KillMeEventArgs>((object sender, GetDataHandlers.KillMeEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketLiquidSet":
                    handler = new EventHandler<GetDataHandlers.LiquidSetEventArgs>((object sender, GetDataHandlers.LiquidSetEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketNewProjectile":
                    handler = new EventHandler<GetDataHandlers.NewProjectileEventArgs>((object sender, GetDataHandlers.NewProjectileEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketNPCHome":
                    handler = new EventHandler<GetDataHandlers.NPCHomeChangeEventArgs>((object sender, GetDataHandlers.NPCHomeChangeEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketNPCSpecial":
                    handler = new EventHandler<GetDataHandlers.NPCSpecialEventArgs>((object sender, GetDataHandlers.NPCSpecialEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketNPCStrike":
                    handler = new EventHandler<GetDataHandlers.NPCStrikeEventArgs>((object sender, GetDataHandlers.NPCStrikeEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketPaintTile":
                    handler = new EventHandler<GetDataHandlers.PaintTileEventArgs>((object sender, GetDataHandlers.PaintTileEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketPaintWall":
                    handler = new EventHandler<GetDataHandlers.PaintWallEventArgs>((object sender, GetDataHandlers.PaintWallEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketPlayerAnimation":
                    handler = new EventHandler<GetDataHandlers.PlayerAnimationEventArgs>((object sender, GetDataHandlers.PlayerAnimationEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketPlayerBuff":
                    handler = new EventHandler<GetDataHandlers.PlayerBuffEventArgs>((object sender, GetDataHandlers.PlayerBuffEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketPlayerBuffUpdate":
                    handler = new EventHandler<GetDataHandlers.PlayerBuffUpdateEventArgs>((object sender, GetDataHandlers.PlayerBuffUpdateEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketPlayerDamage":
                    handler = new EventHandler<GetDataHandlers.PlayerDamageEventArgs>((object sender, GetDataHandlers.PlayerDamageEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketPlayerHP":
                    handler = new EventHandler<GetDataHandlers.PlayerHPEventArgs>((object sender, GetDataHandlers.PlayerHPEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketPlayerInfo":
                    handler = new EventHandler<GetDataHandlers.PlayerInfoEventArgs>((object sender, GetDataHandlers.PlayerInfoEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketPlayerMana":
                    handler = new EventHandler<GetDataHandlers.PlayerManaEventArgs>((object sender, GetDataHandlers.PlayerManaEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketPlayerSlot":
                    handler = new EventHandler<GetDataHandlers.PlayerSlotEventArgs>((object sender, GetDataHandlers.PlayerSlotEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketPlayerSpawn":
                    handler = new EventHandler<GetDataHandlers.SpawnEventArgs>((object sender, GetDataHandlers.SpawnEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketPlayerTeam":
                    handler = new EventHandler<GetDataHandlers.PlayerTeamEventArgs>((object sender, GetDataHandlers.PlayerTeamEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketPlayerUpdate":
                    handler = new EventHandler<GetDataHandlers.PlayerUpdateEventArgs>((object sender, GetDataHandlers.PlayerUpdateEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketSendTileSquare":
                    handler = new EventHandler<GetDataHandlers.SendTileSquareEventArgs>((object sender, GetDataHandlers.SendTileSquareEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketSign":
                    handler = new EventHandler<GetDataHandlers.SignEventArgs>((object sender, GetDataHandlers.SignEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketTeleport":
                    handler = new EventHandler<GetDataHandlers.TeleportEventArgs>((object sender, GetDataHandlers.TeleportEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketTileEdit":
                    handler = new EventHandler<GetDataHandlers.TileEditEventArgs>((object sender, GetDataHandlers.TileEditEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketTileKill":
                    handler = new EventHandler<GetDataHandlers.TileKillEventArgs>((object sender, GetDataHandlers.TileKillEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketTogglePvp":
                    handler = new EventHandler<GetDataHandlers.TogglePvpEventArgs>((object sender, GetDataHandlers.TogglePvpEventArgs args) => { OnHook(sender, args); });
                    break;

                // TODO???: OTAPI.Hooks
                /*case "OnProjectilePreKill":
                    handler = new OTAPI.Modifications.NetworkText.AfterChatMessageHandler((text, color, ignore) => { return OnHookWithResult(); });
                    handlerControl = (Action<bool>)((on) => { if (on) OTAPI.Hooks.BroadcastChatMessage.AfterBroadcastChatMessage += handler; else OTAPI.Hooks.BroadcastChatMessage.AfterBroadcastChatMessage -= handler; });
                    OTAPI.Hooks.BroadcastChatMessage.AfterBroadcastChatMessage += new Action(() => { });
                    break;
                case "OnProjectilePreKill":
                    handler = new OTAPI.Hooks.Projectile.PreKillHandler((Projectile p) => { return OnHookWithResult(p); });
                    handlerControl = (Action<bool>)((on) => { if (on) OTAPI.Hooks.Projectile.PreKill += handler; else OTAPI.Hooks.Projectile.PreKill -= handler; });
                    break;*/
                
                /*case "OnProjectilePostKill":
                    handler = new OTAPI.Hooks.Projectile.PostKilledHandler((Projectile p) => { OnHook(p); });
                    break;*/

                default:
                    handler = null;
                    luaEnv.PrintError("Trying to create unknown hook!");
                    break;
            }
        }
    }
}
