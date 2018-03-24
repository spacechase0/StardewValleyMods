using SpaceCore.Utilities;
using StardewValley.Events;
using System;

namespace SpaceCore.Events
{
    public class SpaceEvents
    {
        // This occurs before loading starts.
        // Locations should be added here so that SaveData.loadDataToLocations picks them up
        public static event EventHandler OnBlankSave;

        // When the shipping menu pops up, level up menus, ...
        public static event EventHandler<EventArgsShowNightEndMenus> ShowNightEndMenus;

        // Lets you hook into Utillity.pickFarmEvent
        public static event EventHandler<EventArgsChooseNightlyFarmEvent> ChooseNightlyFarmEvent;

        // When the player is done eating an item.
        // Check what item using player.itemToEat
        public static event EventHandler OnItemEaten;

        internal static void InvokeOnBlankSave()
        {
            Log.trace("Event: OnBlankSave");
            if (OnBlankSave == null)
                return;
            Util.invokeEvent("SpaceEvents.OnBlankSave", OnBlankSave.GetInvocationList(), null);
        }

        internal static void InvokeShowNightEndMenus(EventArgsShowNightEndMenus args)
        {
            Log.trace("Event: ShowNightEndMenus");
            if (ShowNightEndMenus == null)
                return;
            Util.invokeEvent( "SpaceEvents.ShowNightEndMenus", ShowNightEndMenus.GetInvocationList(), null, args);
        }

        internal static FarmEvent InvokeChooseNightlyFarmEvent(FarmEvent vanilla)
        {
            var args = new EventArgsChooseNightlyFarmEvent();
            args.NightEvent = vanilla;

            Log.trace("Event: ChooseNightlyFarmEvent");
            if (ChooseNightlyFarmEvent == null)
                return args.NightEvent;
            Util.invokeEvent("SpaceEvents.ChooseNightlyFarmEvent", ChooseNightlyFarmEvent.GetInvocationList(), null, args);
            return args.NightEvent;
        }

        internal static void InvokeOnItemEaten()
        {
            Log.trace("Event: OnItemEaten");
            if (OnItemEaten == null)
                return;
            Util.invokeEvent("SpaceEvents.OnItemEaten", OnItemEaten.GetInvocationList(), null);
        }
    }
}
