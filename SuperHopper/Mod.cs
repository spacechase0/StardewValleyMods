using HarmonyLib;
using Microsoft.Xna.Framework;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;

namespace SuperHopper
{
    public class Mod : StardewModdingAPI.Mod
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
            if (obj is Chest chest && chest.SpecialChestType == Chest.SpecialChestTypes.AutoLoader && (e.Button == SButton.MouseLeft || e.Button == SButton.ControllerA))
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

    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.minutesElapsed))]
    public static class ObjectMinutesElapsedPatch
    {
        public static bool Prefix(StardewValley.Object __instance, int minutes, GameLocation environment)
        {
            if (__instance is Chest chest && chest.SpecialChestType == Chest.SpecialChestTypes.AutoLoader && chest.heldObject.Value != null && Utility.IsNormalObjectAtParentSheetIndex(chest.heldObject.Value, StardewValley.Object.iridiumBar))
            {
                environment.objects.TryGetValue(chest.TileLocation - new Vector2(0, 1), out StardewValley.Object aboveObj);
                if (aboveObj != null && aboveObj is Chest aboveChest && chest.items.Count < chest.GetActualCapacity() && aboveChest.items.Count > 0)
                {
                    chest.items.Add(aboveChest.items[0]);
                    aboveChest.items.RemoveAt(0);
                }
                // Not doing for now because I'd need to handle every machine's special rules, like changing ParentSheetIndex
                /*
                else if ( aboveObj != null && aboveObj?.GetType() == typeof( StardewValley.Object ) && aboveObj.bigCraftable.Value && aboveObj.MinutesUntilReady == 0 && chest.items.Count < chest.GetActualCapacity() )
                {
                    chest.addItem( aboveObj.heldObject.Value );
                    aboveObj.heldObject.Value = null;
                }
                */

                environment.objects.TryGetValue(chest.TileLocation + new Vector2(0, 1), out StardewValley.Object belowObj);
                if (belowObj != null && belowObj is Chest belowChest && chest.items.Count > 0 && belowChest.items.Count < belowChest.GetActualCapacity())
                {
                    belowChest.items.Add(chest.items[0]);
                    chest.items.RemoveAt(0);
                }
                return false;
            }

            return true;
        }
    }
}
