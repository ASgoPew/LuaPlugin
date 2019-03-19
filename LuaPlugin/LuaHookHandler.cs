using NLua;
using OTAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace MyLua
{
    public class LuaHookHandler<T> : ILuaHookHandler
    {
        LuaEnvironment LuaEnv { get; set; }
        public string Name { get; set; }
        Action<LuaHookHandler<T>, bool?> Control { get; set; }
        public T Handler { get; set; }
        public bool Active { get; private set; }

        public LuaHookHandler(LuaEnvironment luaEnv, string name, Action<LuaHookHandler<T>, bool?> control)
        {
            LuaEnv = luaEnv;
            Name = name;
            Control = control;
        }

        public void Invoke(params object[] args)
        {
            try
            {
                LuaEnv.CallFunctionByName(Name, args);
            }
            catch (Exception e)
            {
                Control.Invoke(this, false);
                LuaEnv.Set(Name, null);
                //LuaEnv.LuaException.Invoke(e);
            }
        }

        public void Update()
        {
            LuaFunction f = LuaEnv.Get(Name) as LuaFunction;
            if (f != null && !Active)
                Enable();
            else if (f == null && Active)
                Disable();
        }

        public void Enable()
        {
            Active = true;
            Control.Invoke(this, true);
        }

        public void Disable()
        {
            Control.Invoke(this, false);
            Active = false;
        }
    }

    public class LuaHookHandler2
    {
        LuaHookHandler<Action> h = new LuaHookHandler<Action>(null, "OnTick", (hook, state) =>
        {
            if      (state ==  true) Main.OnTick += hook.Handler;
            else if (state == false) Main.OnTick -= hook.Handler;
            else hook.Handler = () => hook.Invoke();
        });
        LuaHookHandler<HookHandler<EventArgs>> h2 = new LuaHookHandler<HookHandler<EventArgs>>(null, "OnTick", (hook, state) =>
        {
            if      (state ==  true) ServerApi.Hooks.GameInitialize.Register(null, hook.Handler);
            else if (state == false) ServerApi.Hooks.GameInitialize.Deregister(null, hook.Handler);
            else hook.Handler = (args) => hook.Invoke(args);
        });
        LuaHookHandler<AccountHooks.AccountCreateD> h3 = new LuaHookHandler<AccountHooks.AccountCreateD>(null, "OnTick", (hook, state) =>
        {
            if      (state ==  true) AccountHooks.AccountCreate += hook.Handler;
            else if (state == false) AccountHooks.AccountCreate -= hook.Handler;
            else hook.Handler = (args) => hook.Invoke(args);
        });
        LuaHookHandler<EventHandler<GetDataHandlers.NewProjectileEventArgs>> h4 = new LuaHookHandler<EventHandler<GetDataHandlers.NewProjectileEventArgs>>(null, "asd", (hook, state) =>
        {
            if      (state ==  true) GetDataHandlers.NewProjectile += hook.Handler;
            else if (state == false) GetDataHandlers.NewProjectile -= hook.Handler;
            else hook.Handler = (sender, args) => hook.Invoke(args);
        });

        public void a()
        {
            typeof(AccountHooks.AccountCreateD).GetConstructors()[0].Invoke(new object[] {  });
        }

        //public delegate void HookHandler();
        //public delegate void HookHandler<T>(T args);
        //public delegate void HookHandler<T, U>(T sender, U args);

        public LuaEnvironment2 LuaEnv;
        public string Name;
        public bool Active = false;

        dynamic Handler = null;

        public void OnHook(object arg0 = null, object arg1 = null)
        {
            //luaEnv.Set("source", this, "OnHook"); // Why does this line increases ping linearly?
            //luaEnv.Set("source", name, "OnHook"); // And this one works fine!
            object oldSource = LuaEnv.Data["source"];
            
            if (LuaEnv.CallFunction(Name, (lua, state) => {
                if (state == 0)
                    LuaEnv.Data["source"] = this;
                else if (state > 0)
                    LuaEnv.Data["source"] = oldSource;
            }, $"OnHook ({Name})", arg0, arg1) == null)
                LuaEnv.Unhook(Name);
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
            Active = true;
            Control(true);
        }

        public void Unhook()
        {
            //Console.WriteLine("Unhooking " + name);
            Control(false);
            Active = false;
        }

        public void Update()
        {
            using (LuaFunction f = LuaEnv.Get(Name) as LuaFunction)
            {
                if (f != null && !Active)
                    Hook();
                else if (f == null && Active)
                    Unhook();
            }
        }

        //public static Dictionary<string, Type> HandlerNames = new Dictionary<string, Type>()
        //  { "OnTick", typeof(EventArgs)
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
            switch (Name)
            {
                // Main hooks
                case "OnTick":
                    if (on) Main.OnTick += Handler; else Main.OnTick -= Handler;
                    break;
                
                // ServerApi.Hooks hooks
                case "OnDropBossBag":
                    if (on) ServerApi.Hooks.DropBossBag.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.DropBossBag.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnHardmodeTileUpdate":
                    if (on) ServerApi.Hooks.GameHardmodeTileUpdate.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.GameHardmodeTileUpdate.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnGameInitialize":
                    if (on) ServerApi.Hooks.GameInitialize.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.GameInitialize.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnGamePostInitialize":
                    if (on) ServerApi.Hooks.GamePostInitialize.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.GamePostInitialize.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnGamePostUpdate":
                    if (on) ServerApi.Hooks.GamePostUpdate.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.GamePostUpdate.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnStatueSpawn":
                    if (on) ServerApi.Hooks.GameStatueSpawn.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.GameStatueSpawn.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnGameUpdate":
                    if (on) ServerApi.Hooks.GameUpdate.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.GameUpdate.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnGameWorldConnect":
                    if (on) ServerApi.Hooks.GameWorldConnect.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.GameWorldConnect.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnGameWorldDisconnect":
                    if (on) ServerApi.Hooks.GameWorldDisconnect.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.GameWorldDisconnect.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnForceItemIntoChest":
                    if (on) ServerApi.Hooks.ItemForceIntoChest.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.ItemForceIntoChest.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnItemNetDefaults":
                    if (on) ServerApi.Hooks.ItemNetDefaults.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.ItemNetDefaults.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnItemSetDefualtsInt":
                    if (on) ServerApi.Hooks.ItemSetDefaultsInt.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.ItemSetDefaultsInt.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnItemSetDefaultsString":
                    if (on) ServerApi.Hooks.ItemSetDefaultsString.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.ItemSetDefaultsString.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnGetData":
                    if (on) ServerApi.Hooks.NetGetData.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.NetGetData.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnGreetPlayer":
                    if (on) ServerApi.Hooks.NetGreetPlayer.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.NetGreetPlayer.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnNameCollision":
                    if (on) ServerApi.Hooks.NetNameCollision.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.NetNameCollision.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnSendBytes":
                    if (on) ServerApi.Hooks.NetSendBytes.Register(LuaPlugin.Instance, Handler); else ServerApi.Hooks.NetSendBytes.Deregister(LuaPlugin.Instance, Handler);
                    break;
                case "OnSendData":
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
                    LuaEnv.PrintError("Trying to create unknown hook!");
                    break;
            }
        }

        public Action CreateAction()
        {
            return () => OnHook(Main.netPlayCounter);
        }

        public Action<T> CreateAction<T>()
        {
            return (T args) => OnHook(args);
        }

        public HookHandler<T> CreateHookHandler<T>() where T : EventArgs
        {
            return new HookHandler<T>((T args) => { OnHook(args); });
        }

        public EventHandler<T> CreateEventHandler<T>() where T : EventArgs
        {
            return new EventHandler<T>((object sender, T args) => { OnHook(args); });
        }

        public LuaHookHandler2(LuaEnvironment2 newLuaEnv, string newName)
        {
            //handler = GetType().GetMethod("CreateHookHandler").MakeGenericMethod(typeof(DropBossBagEventArgs)).Invoke(this, null);

            LuaEnv = newLuaEnv;
            Name = newName;
            switch (Name)
            {
                // Main hooks
                case "OnTick":
                    Handler = new Action(() => { OnHook(Main.netPlayCounter); });
                    break;
                
                // ServerApi.Hooks hooks
                case "OnDropBossBag":
                    Handler = new HookHandler<DropBossBagEventArgs>((DropBossBagEventArgs args) => { OnHook(args); });
                    break;
                case "OnHardmodeTileUpdate":
                    Handler = new HookHandler<HardmodeTileUpdateEventArgs>((HardmodeTileUpdateEventArgs args) => { OnHook(args); });
                    break;
                case "OnGameInitialize":
                    Handler = new HookHandler<EventArgs>((EventArgs args) => { OnHook(args); });
                    break;
                case "OnGamePostInitialize":
                    Handler = new HookHandler<EventArgs>((EventArgs args) => { OnHook(args); });
                    break;
                case "OnGamePostUpdate":
                    Handler = new HookHandler<EventArgs>((EventArgs args) => { OnHook(args); });
                    break;
                case "OnStatueSpawn":
                    Handler = new HookHandler<StatueSpawnEventArgs>((StatueSpawnEventArgs args) => { OnHook(args); });
                    break;
                case "OnGameUpdate":
                    Handler = new HookHandler<EventArgs>((EventArgs args) => { OnHook(args); });
                    break;
                case "OnGameWorldConnect":
                    Handler = new HookHandler<EventArgs>((EventArgs args) => { OnHook(args); });
                    break;
                case "OnGameWorldDisconnect":
                    Handler = new HookHandler<EventArgs>((EventArgs args) => { OnHook(args); });
                    break;
                case "OnForceItemIntoChest":
                    Handler = new HookHandler<ForceItemIntoChestEventArgs>((ForceItemIntoChestEventArgs args) => { OnHook(args); });
                    break;
                case "OnItemNetDefaults":
                    Handler = new HookHandler<SetDefaultsEventArgs<Item, int>>((SetDefaultsEventArgs<Item, int> args) => { OnHook(args); });
                    break;
                case "OnItemSetDefualtsInt":
                    Handler = new HookHandler<SetDefaultsEventArgs<Item, int>>((SetDefaultsEventArgs<Item, int> args) => { OnHook(args); });
                    break;
                case "OnItemSetDefaultsString":
                    Handler = new HookHandler<SetDefaultsEventArgs<Item, string>>((SetDefaultsEventArgs<Item, string> args) => { OnHook(args); });
                    break;
                case "OnGetData":
                    Handler = new HookHandler<GetDataEventArgs>((GetDataEventArgs args) => { OnHook(args); });
                    break;
                case "OnGreetPlayer":
                    Handler = new HookHandler<GreetPlayerEventArgs>((GreetPlayerEventArgs args) => { OnHook(args); });
                    break;
                case "OnNameCollision":
                    Handler = new HookHandler<NameCollisionEventArgs>((NameCollisionEventArgs args) => { OnHook(args); });
                    break;
                case "OnSendBytes":
                    Handler = new HookHandler<SendBytesEventArgs>((SendBytesEventArgs args) => { OnHook(args); });
                    break;
                case "OnSendData":
                    Handler = new HookHandler<SendDataEventArgs>((SendDataEventArgs args) => { OnHook(args); });
                    break;
                case "OnNpcAiUpdate":
                    Handler = new HookHandler<NpcAiUpdateEventArgs>((NpcAiUpdateEventArgs args) => { OnHook(args); });
                    break;
                case "OnNpcKilled":
                    Handler = new HookHandler<NpcKilledEventArgs>((NpcKilledEventArgs args) => { OnHook(args); });
                    break;
                case "OnNpcLootDrop":
                    Handler = new HookHandler<NpcLootDropEventArgs>((NpcLootDropEventArgs args) => { OnHook(args); });
                    break;
                case "OnNpcNetDefaults":
                    Handler = new HookHandler<SetDefaultsEventArgs<NPC, int>>((SetDefaultsEventArgs<NPC, int> args) => { OnHook(args); });
                    break;
                case "OnNpcSetDefaultsInt":
                    Handler = new HookHandler<SetDefaultsEventArgs<NPC, int>>((SetDefaultsEventArgs<NPC, int> args) => { OnHook(args); });
                    break;
                case "OnNpcSetDefaultsString":
                    Handler = new HookHandler<SetDefaultsEventArgs<NPC, string>>((SetDefaultsEventArgs<NPC, string> args) => { OnHook(args); });
                    break;
                case "OnNpcSpawn":
                    Handler = new HookHandler<NpcSpawnEventArgs>((NpcSpawnEventArgs args) => { OnHook(args); });
                    break;
                case "OnNpcStrike":
                    Handler = new HookHandler<NpcStrikeEventArgs>((NpcStrikeEventArgs args) => { OnHook(args); });
                    break;
                case "OnNpcTransform":
                    Handler = new HookHandler<NpcTransformationEventArgs>((NpcTransformationEventArgs args) => { OnHook(args); });
                    break;
                case "OnNpcTriggerPressurePlate":
                    Handler = new HookHandler<TriggerPressurePlateEventArgs<NPC>>((TriggerPressurePlateEventArgs<NPC> args) => { OnHook(args); });
                    break;
                case "OnPlayerTriggerPressurePlate":
                    Handler = new HookHandler<TriggerPressurePlateEventArgs<Player>>((TriggerPressurePlateEventArgs<Player> args) => { OnHook(args); });
                    break;
                case "OnPlayerUpdatePhysics":
                    Handler = new HookHandler<UpdatePhysicsEventArgs>((UpdatePhysicsEventArgs args) => { OnHook(args); });
                    break;
                case "OnProjectileAiUpdate":
                    Handler = new HookHandler<ProjectileAiUpdateEventArgs>((ProjectileAiUpdateEventArgs args) => { OnHook(args); });
                    break;
                case "OnProjectileSetDefaults":
                    Handler = new HookHandler<SetDefaultsEventArgs<Projectile, int>>((SetDefaultsEventArgs<Projectile, int> args) => { OnHook(args); });
                    break;
                case "OnProjectileTriggerPressurePlate":
                    Handler = new HookHandler<TriggerPressurePlateEventArgs<Projectile>>((TriggerPressurePlateEventArgs<Projectile> args) => { OnHook(args); });
                    break;
                case "OnServerBroadcast":
                    Handler = new HookHandler<ServerBroadcastEventArgs>((ServerBroadcastEventArgs args) => { OnHook(args); });
                    break;
                case "OnServerChat":
                    Handler = new HookHandler<ServerChatEventArgs>((ServerChatEventArgs args) => { OnHook(args); });
                    break;
                case "OnServerCommand":
                    Handler = new HookHandler<CommandEventArgs>((CommandEventArgs args) => { OnHook(args); });
                    break;
                case "OnConnect":
                    Handler = new HookHandler<ConnectEventArgs>((ConnectEventArgs args) => { OnHook(args); });
                    break;
                case "OnJoin":
                    Handler = new HookHandler<JoinEventArgs>((JoinEventArgs args) => { OnHook(args); });
                    break;
                case "OnLeave":
                    Handler = new HookHandler<LeaveEventArgs>((LeaveEventArgs args) => { OnHook(args); });
                    break;
                case "OnServerSocketReset":
                    Handler = new HookHandler<SocketResetEventArgs>((SocketResetEventArgs args) => { OnHook(args); });
                    break;
                case "OnAnnouncementBox":
                    Handler = new HookHandler<TriggerAnnouncementBoxEventArgs>((TriggerAnnouncementBoxEventArgs args) => { OnHook(args); });
                    break;
                case "OnChristmasCheck":
                    Handler = new HookHandler<ChristmasCheckEventArgs>((ChristmasCheckEventArgs args) => { OnHook(args); });
                    break;
                case "OnHalloweenCheck":
                    Handler = new HookHandler<HalloweenCheckEventArgs>((HalloweenCheckEventArgs args) => { OnHook(args); });
                    break;
                case "OnMeteorDrop":
                    Handler = new HookHandler<MeteorDropEventArgs>((MeteorDropEventArgs args) => { OnHook(args); });
                    break;
                case "OnWorldSave":
                    Handler = new HookHandler<WorldSaveEventArgs>((WorldSaveEventArgs args) => { OnHook(args); });
                    break;
                case "OnStartHardMode":
                    Handler = new HookHandler<HandledEventArgs>((HandledEventArgs args) => { OnHook(args); });
                    break;

                // TShockAPI.Hooks.AccountHooks
                case "OnAccountCreate":
                    Handler = new AccountHooks.AccountCreateD((AccountCreateEventArgs args) => { OnHook(args); });
                    break;
                case "OnAccountDelete":
                    Handler = new AccountHooks.AccountDeleteD((AccountDeleteEventArgs args) => { OnHook(args); });
                    break;

                // TShockAPI.Hooks.GeneralHooks
                case "OnReloadEvent":
                    Handler = new GeneralHooks.ReloadEventD((ReloadEventArgs args) => { OnHook(args); });
                    break;

                // TShockAPI.Hooks.PlayerHooks
                case "OnPlayerChat":
                    Handler = new PlayerHooks.PlayerChatD((PlayerChatEventArgs args) => { OnHook(args); });
                    break;
                case "OnPlayerCommand":
                    Handler = new PlayerHooks.PlayerCommandD((PlayerCommandEventArgs args) => { OnHook(args); });
                    break;
                case "OnPlayerLogout":
                    Handler = new PlayerHooks.PlayerLogoutD((PlayerLogoutEventArgs args) => { OnHook(args); });
                    break;
                case "OnPlayerPermission":
                    Handler = new PlayerHooks.PlayerPermissionD((PlayerPermissionEventArgs args) => { OnHook(args); });
                    break;
                case "OnPlayerPostLogin":
                    Handler = new PlayerHooks.PlayerPostLoginD((PlayerPostLoginEventArgs args) => { OnHook(args); });
                    break;
                case "OnPlayerPreLogin":
                    Handler = new PlayerHooks.PlayerPreLoginD((PlayerPreLoginEventArgs args) => { OnHook(args); });
                    break;

                // TShockAPI.Hooks.RegionHooks
                case "OnRegionCreated":
                    Handler = new RegionHooks.RegionCreatedD((RegionHooks.RegionCreatedEventArgs args) => { OnHook(args); });
                    break;
                case "OnRegionDeleted":
                    Handler = new RegionHooks.RegionDeletedD((RegionHooks.RegionDeletedEventArgs args) => { OnHook(args); });
                    break;
                case "OnRegionEntered":
                    Handler = new RegionHooks.RegionEnteredD((RegionHooks.RegionEnteredEventArgs args) => { OnHook(args); });
                    break;
                case "OnRegionLeft":
                    Handler = new RegionHooks.RegionLeftD((RegionHooks.RegionLeftEventArgs args) => { OnHook(args); });
                    Handler = CreateAction<RegionHooks.RegionLeftEventArgs>();
                    break;

                // TShockAPI.TShock
                case "OnTShockInitialize":
                    Handler = new Action(() => { OnHook(); });
                    break;

                // TShockAPI.GetDataHandlers
                case "OnPacketChestItemChange":
                    Handler = new EventHandler<GetDataHandlers.ChestItemEventArgs>((object sender, GetDataHandlers.ChestItemEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketChestOpen":
                    Handler = new EventHandler<GetDataHandlers.ChestOpenEventArgs>((object sender, GetDataHandlers.ChestOpenEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketGemLockToggle":
                    Handler = new EventHandler<GetDataHandlers.GemLockToggleEventArgs>((object sender, GetDataHandlers.GemLockToggleEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketItemDrop":
                    Handler = new EventHandler<GetDataHandlers.ItemDropEventArgs>((object sender, GetDataHandlers.ItemDropEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketKillMe":
                    Handler = new EventHandler<GetDataHandlers.KillMeEventArgs>((object sender, GetDataHandlers.KillMeEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketLiquidSet":
                    Handler = new EventHandler<GetDataHandlers.LiquidSetEventArgs>((object sender, GetDataHandlers.LiquidSetEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketNewProjectile":
                    Handler = new EventHandler<GetDataHandlers.NewProjectileEventArgs>((object sender, GetDataHandlers.NewProjectileEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketNPCHome":
                    Handler = new EventHandler<GetDataHandlers.NPCHomeChangeEventArgs>((object sender, GetDataHandlers.NPCHomeChangeEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketNPCSpecial":
                    Handler = new EventHandler<GetDataHandlers.NPCSpecialEventArgs>((object sender, GetDataHandlers.NPCSpecialEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketNPCStrike":
                    Handler = new EventHandler<GetDataHandlers.NPCStrikeEventArgs>((object sender, GetDataHandlers.NPCStrikeEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketPaintTile":
                    Handler = new EventHandler<GetDataHandlers.PaintTileEventArgs>((object sender, GetDataHandlers.PaintTileEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketPaintWall":
                    Handler = new EventHandler<GetDataHandlers.PaintWallEventArgs>((object sender, GetDataHandlers.PaintWallEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketPlayerAnimation":
                    Handler = new EventHandler<GetDataHandlers.PlayerAnimationEventArgs>((object sender, GetDataHandlers.PlayerAnimationEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketPlayerBuff":
                    Handler = new EventHandler<GetDataHandlers.PlayerBuffEventArgs>((object sender, GetDataHandlers.PlayerBuffEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketPlayerBuffUpdate":
                    Handler = new EventHandler<GetDataHandlers.PlayerBuffUpdateEventArgs>((object sender, GetDataHandlers.PlayerBuffUpdateEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketPlayerDamage":
                    Handler = new EventHandler<GetDataHandlers.PlayerDamageEventArgs>((object sender, GetDataHandlers.PlayerDamageEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketPlayerHP":
                    Handler = new EventHandler<GetDataHandlers.PlayerHPEventArgs>((object sender, GetDataHandlers.PlayerHPEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketPlayerInfo":
                    Handler = new EventHandler<GetDataHandlers.PlayerInfoEventArgs>((object sender, GetDataHandlers.PlayerInfoEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketPlayerMana":
                    Handler = new EventHandler<GetDataHandlers.PlayerManaEventArgs>((object sender, GetDataHandlers.PlayerManaEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketPlayerSlot":
                    Handler = new EventHandler<GetDataHandlers.PlayerSlotEventArgs>((object sender, GetDataHandlers.PlayerSlotEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketPlayerSpawn":
                    Handler = new EventHandler<GetDataHandlers.SpawnEventArgs>((object sender, GetDataHandlers.SpawnEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketPlayerTeam":
                    Handler = new EventHandler<GetDataHandlers.PlayerTeamEventArgs>((object sender, GetDataHandlers.PlayerTeamEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketPlayerUpdate":
                    Handler = new EventHandler<GetDataHandlers.PlayerUpdateEventArgs>((object sender, GetDataHandlers.PlayerUpdateEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketSendTileSquare":
                    Handler = new EventHandler<GetDataHandlers.SendTileSquareEventArgs>((object sender, GetDataHandlers.SendTileSquareEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketSign":
                    Handler = new EventHandler<GetDataHandlers.SignEventArgs>((object sender, GetDataHandlers.SignEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketTeleport":
                    Handler = new EventHandler<GetDataHandlers.TeleportEventArgs>((object sender, GetDataHandlers.TeleportEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketTileEdit":
                    Handler = new EventHandler<GetDataHandlers.TileEditEventArgs>((object sender, GetDataHandlers.TileEditEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketTileKill":
                    Handler = new EventHandler<GetDataHandlers.TileKillEventArgs>((object sender, GetDataHandlers.TileKillEventArgs args) => { OnHook(sender, args); });
                    break;
                case "OnPacketTogglePvp":
                    Handler = new EventHandler<GetDataHandlers.TogglePvpEventArgs>((object sender, GetDataHandlers.TogglePvpEventArgs args) => { OnHook(sender, args); });
                    break;

                // TODO???: OTAPI.Hooks
                case "OnProjectilePreKill":
                    Handler = new OTAPI.Modifications.NetworkText.AfterChatMessageHandler(this.f);
                    //Control = (Action<bool>)((on) => { if (on) OTAPI.Hooks.BroadcastChatMessage.AfterBroadcastChatMessage += handler; else OTAPI.Hooks.BroadcastChatMessage.AfterBroadcastChatMessage -= handler; });
                    //OTAPI.Hooks.BroadcastChatMessage.AfterBroadcastChatMessage += new Action(() => { });
                    break;
                //case "OnProjectilePreKill":
                    //handler = new OTAPI.Hooks.Projectile.PreKillHandler((Projectile p) => { return OnHookWithResult(p); });
                    //handlerControl = (Action<bool>)((on) => { if (on) OTAPI.Hooks.Projectile.PreKill += handler; else OTAPI.Hooks.Projectile.PreKill -= handler; });
                    //break;
                
                /*case "OnProjectilePostKill":
                    handler = new OTAPI.Hooks.Projectile.PostKilledHandler((Projectile p) => { OnHook(p); });
                    break;*/

                default:
                    Handler = null;
                    LuaEnv.PrintError("Trying to create unknown hook!");
                    break;
            }
        }

        /*public void f(Terraria.Localization.NetworkText text, ref Microsoft.Xna.Framework.Color color, ref int igonrePlayer)
        {
            
        }*/
    }

    public static class cl
    {
        public static void f(this LuaHookHandler2 hook, Terraria.Localization.NetworkText text, ref Microsoft.Xna.Framework.Color color, ref int igonrePlayer)
        {

        }
    }
}
