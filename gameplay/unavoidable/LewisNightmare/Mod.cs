using System;
using SpaceCore.Events;
using SpaceShared;

namespace LewisNightmare
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(StardewModdingAPI.IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            Helper.Events.Display.MenuChanged += this.Display_MenuChanged;

            SpaceEvents.ActionActivated += this.SpaceEvents_ActionActivated;
        }

        private void SpaceEvents_ActionActivated(object sender, EventArgsAction e)
        {
        }

        private void Display_MenuChanged(object sender, StardewModdingAPI.Events.MenuChangedEventArgs e)
        {
            // detect dialogue, do things
        }
    }
}
