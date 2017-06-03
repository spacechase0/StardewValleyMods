using SpaceCore.Utilities;
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

        internal static void InvokeOnBlankSave()
        {
            Log.trace("Event: OnBlankSave");
            Util.invokeEvent("SpaceEvents.OnBlankSave", OnBlankSave.GetInvocationList(), null);
        }

        internal static void InvokeShowNightEndMenus(EventArgsShowNightEndMenus args)
        {
            Log.trace("Event: ShowNightEndMenus");
            Util.invokeEvent( "SpaceEvents.ShowNightEndMenus", ShowNightEndMenus.GetInvocationList(), null, args);
        }
    }
}
