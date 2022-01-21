using System;
using Microsoft.Xna.Framework;
using SpaceCore.Interface;
using SpaceShared;
using StardewValley;
using StardewValley.Events;
using StardewValley.Network;
using SObject = StardewValley.Object;

namespace SpaceCore.Events
{
    public class SpaceEvents
    {
        /// <summary>This occurs before loading starts.</summary>
        /// Locations should be added here so that SaveData.loadDataToLocations picks them up
        public static event EventHandler OnBlankSave;

        /// <summary>When the shipping menu pops up, level up menus, ...</summary>
        public static event EventHandler<EventArgsShowNightEndMenus> ShowNightEndMenus;

        /// <summary>Lets you hook into Utillity.pickFarmEvent</summary>
        public static event EventHandler<EventArgsChooseNightlyFarmEvent> ChooseNightlyFarmEvent;

        /// <summary>When the player is done eating an item.</summary>
        /// Check what item using player.itemToEat
        public static event EventHandler OnItemEaten;

        /// <summary>When a tile "Action" is activated</summary>
        public static event EventHandler<EventArgsAction> ActionActivated;

        /// <summary>When a tile "TouchAction" is activated</summary>
        public static event EventHandler<EventArgsAction> TouchActionActivated;

        /// <summary>Server side, when a client joins</summary>
        public static event EventHandler<EventArgsServerGotClient> ServerGotClient;

        /// <summary>Right before a gift is given to someone. Sender is farmer.</summary>
        public static event EventHandler<EventArgsBeforeReceiveObject> BeforeGiftGiven;

        /// <summary>When a gift is given to someone. Sender is farmer.</summary>
        public static event EventHandler<EventArgsGiftGiven> AfterGiftGiven;

        /// B<summary>efore the player is about to warp. Can cancel warping or change the target location.</summary>
        public static event EventHandler<EventArgsBeforeWarp> BeforeWarp;

        /// <summary>When a bomb explodes</summary>
        public static event EventHandler<EventArgsBombExploded> BombExploded;

        /// <summary>When an event finishes. Use Game1.CurrentEvent to check which one.</summary>
        public static event EventHandler OnEventFinished;

        /// <summary>Event for adding wallet items to NewSkillsPage, before the controller-pickable logic needs to run</summary>
        public static event EventHandler AddWalletItems;

        internal static void InvokeOnBlankSave()
        {
            Log.Trace("Event: OnBlankSave");
            if (SpaceEvents.OnBlankSave == null)
                return;
            Util.InvokeEvent("SpaceEvents.OnBlankSave", SpaceEvents.OnBlankSave.GetInvocationList(), null);
        }

        internal static void InvokeShowNightEndMenus(EventArgsShowNightEndMenus args)
        {
            Log.Trace("Event: ShowNightEndMenus");
            if (SpaceEvents.ShowNightEndMenus == null)
                return;
            Util.InvokeEvent("SpaceEvents.ShowNightEndMenus", SpaceEvents.ShowNightEndMenus.GetInvocationList(), null, args);
        }

        internal static FarmEvent InvokeChooseNightlyFarmEvent(FarmEvent vanilla)
        {
            var args = new EventArgsChooseNightlyFarmEvent
            {
                NightEvent = vanilla
            };

            Log.Trace("Event: ChooseNightlyFarmEvent");
            if (SpaceEvents.ChooseNightlyFarmEvent == null)
                return args.NightEvent;
            Util.InvokeEvent("SpaceEvents.ChooseNightlyFarmEvent", SpaceEvents.ChooseNightlyFarmEvent.GetInvocationList(), null, args);
            return args.NightEvent;
        }

        internal static void InvokeOnItemEaten(Farmer farmer)
        {
            Log.Trace("Event: OnItemEaten");
            if (SpaceEvents.OnItemEaten == null || !farmer.IsLocalPlayer)
                return;
            Util.InvokeEvent("SpaceEvents.OnItemEaten", SpaceEvents.OnItemEaten.GetInvocationList(), farmer);
        }

        internal static bool InvokeActionActivated(Farmer who, string action, xTile.Dimensions.Location pos)
        {
            Log.Trace("Event: ActionActivated");
            if (SpaceEvents.ActionActivated == null || !who.IsLocalPlayer)
                return false;
            var arg = new EventArgsAction(false, action, pos);
            return Util.InvokeEventCancelable("SpaceEvents.ActionActivated", SpaceEvents.ActionActivated.GetInvocationList(), who, arg);
        }

        internal static bool InvokeTouchActionActivated(Farmer who, string action, xTile.Dimensions.Location pos)
        {
            Log.Trace("Event: TouchActionActivated");
            if (SpaceEvents.TouchActionActivated == null || !who.IsLocalPlayer)
                return false;
            var arg = new EventArgsAction(true, action, pos);
            return Util.InvokeEventCancelable("SpaceEvents.TouchActionActivated", SpaceEvents.TouchActionActivated.GetInvocationList(), who, arg);
        }

        internal static void InvokeServerGotClient(GameServer server, long peer)
        {
            var args = new EventArgsServerGotClient
            {
                FarmerID = peer
            };

            Log.Trace("Event: ServerGotClient");
            if (SpaceEvents.ServerGotClient == null)
                return;
            Util.InvokeEvent("SpaceEvents.ServerGotClient", SpaceEvents.ServerGotClient.GetInvocationList(), server, args);
        }

        internal static bool InvokeBeforeReceiveObject(NPC npc, SObject obj, Farmer farmer)
        {
            Log.Trace("Event: BeforeReceiveObject");
            if (SpaceEvents.BeforeGiftGiven == null)
                return false;
            var arg = new EventArgsBeforeReceiveObject(npc, obj);
            return Util.InvokeEventCancelable("SpaceEvents.BeforeReceiveObject", SpaceEvents.BeforeGiftGiven.GetInvocationList(), farmer, arg);
        }

        // Public for use in DGA
        public static void InvokeAfterGiftGiven(NPC npc, SObject obj, Farmer farmer)
        {
            Log.Trace("Event: AfterGiftGiven");
            if (SpaceEvents.AfterGiftGiven == null)
                return;
            var arg = new EventArgsGiftGiven(npc, obj);
            Util.InvokeEvent("SpaceEvents.AfterGiftGiven", SpaceEvents.AfterGiftGiven.GetInvocationList(), farmer, arg);
        }

        internal static bool InvokeBeforeWarp(ref LocationRequest req, ref int targetX, ref int targetY, ref int facing)
        {
            Log.Trace("Event: BeforeWarp");
            if (SpaceEvents.BeforeWarp == null)
                return false;
            var arg = new EventArgsBeforeWarp(req, targetX, targetY, facing);
            bool ret = Util.InvokeEventCancelable("SpaceEvents.BeforeWarp", SpaceEvents.BeforeWarp.GetInvocationList(), Game1.player, arg);
            req = arg.WarpTargetLocation;
            targetX = arg.WarpTargetX;
            targetY = arg.WarpTargetY;
            facing = arg.WarpTargetFacing;
            return ret;
        }

        internal static void InvokeBombExploded(Farmer who, Vector2 tileLocation, int radius)
        {
            Log.Trace("Event: BombExploded");
            if (SpaceEvents.BombExploded == null)
                return;
            var arg = new EventArgsBombExploded(tileLocation, radius);
            Util.InvokeEvent("SpaceEvents.BombExploded", SpaceEvents.BombExploded.GetInvocationList(), who, arg);
        }

        internal static void InvokeOnEventFinished()
        {
            Log.Trace("Event: OnEventFinished");
            if (SpaceEvents.OnEventFinished == null)
                return;
            Util.InvokeEvent("SpaceEvents.OnEventFinished", SpaceEvents.OnEventFinished.GetInvocationList(), null);
        }

        internal static void InvokeAddWalletItems( NewSkillsPage page )
        {
            Log.Trace( "Event: AddWalletItems" );
            if ( SpaceEvents.AddWalletItems == null )
                return;
            Util.InvokeEvent( "SpaceEvents.AddWalletItems", SpaceEvents.AddWalletItems.GetInvocationList(), page );
        }

        internal static bool HasAddWalletItemEventHandlers()
        {
            return AddWalletItems != null;
        }
    }
}
