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

        // So you can prevent something from happening when a hotbar button is pressed
        // NOTE: Only works for the number bar (1234567890-=)
        public static event EventHandler<EventArgsSelectHotbarSlot> SelectHotbarSlot;

        // Lets you hook into Utillity.pickFarmEvent
        public static event EventHandler<EventArgsChooseNightlyFarmEvent> ChooseNightlyFarmEvent;

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

        internal static bool InvokeSelectHotbarSlot(int slot)
        {
            EventArgsSelectHotbarSlot args = new EventArgsSelectHotbarSlot();
            args.Slot = slot;

            Log.trace("Event: SelectHotbarSlot");
            if (SelectHotbarSlot == null)
                return !args.Canceled;
            Util.invokeEvent("SpaceEvents.SelectHotbarSlot", SelectHotbarSlot.GetInvocationList(), null, args);
            return !args.Canceled;
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
    }
}
