using HarmonyLib;
using Microsoft.Xna.Framework;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;

namespace SuperHopper
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            Mod.instance = this;
            Log.Monitor = this.Monitor;

            helper.Events.Input.ButtonPressed += this.OnButtonPressed;

            var harmony = new Harmony(this.ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;

            Game1.currentLocation.objects.TryGetValue(e.Cursor.GrabTile, out StardewValley.Object obj);
            if (obj is Chest { SpecialChestType: Chest.SpecialChestTypes.AutoLoader } chest && (e.Button is SButton.MouseLeft or SButton.ControllerA))
            {
                if (chest.heldObject.Value == null)
                {
                    if (Utility.IsNormalObjectAtParentSheetIndex(Game1.player.ActiveObject, StardewValley.Object.iridiumBar))
                    {
                        chest.Tint = Color.DarkViolet;
                        chest.heldObject.Value = (StardewValley.Object)Game1.player.ActiveObject.getOne();
                        if (Game1.player.ActiveObject.Stack > 1)
                            Game1.player.ActiveObject.Stack--;
                        else
                            Game1.player.ActiveObject = null;
                        Game1.playSound("furnace");
                    }
                }
                else if (Game1.player.CurrentItem == null)
                {
                    chest.Tint = Color.White;
                    chest.heldObject.Value = null;
                    Game1.player.addItemToInventory(new StardewValley.Object(StardewValley.Object.iridiumBar, 1));
                    Game1.playSound("shiny4");
                }
            }
        }
    }
}
