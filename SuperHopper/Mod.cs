using Microsoft.Xna.Framework;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using SuperHopper.Patches;
using SObject = StardewValley.Object;

namespace SuperHopper
{
    internal class Mod : StardewModdingAPI.Mod
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            Log.Monitor = this.Monitor;

            helper.Events.Input.ButtonPressed += this.OnButtonPressed;

            HarmonyPatcher.Apply(this,
                new ObjectPatcher(this.OnMachineMinutesElapsed)
            );
        }


        /*********
        ** Private methods
        *********/
        /// <inheritdoc cref="IInputEvents.ButtonPressed"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;

            Game1.currentLocation.objects.TryGetValue(e.Cursor.GrabTile, out SObject obj);
            if (obj is Chest { SpecialChestType: Chest.SpecialChestTypes.AutoLoader } chest && (e.Button is SButton.MouseLeft or SButton.ControllerA))
            {
                if (chest.heldObject.Value == null)
                {
                    if (Utility.IsNormalObjectAtParentSheetIndex(Game1.player.ActiveObject, SObject.iridiumBar))
                    {
                        chest.Tint = Color.DarkViolet;
                        chest.heldObject.Value = (SObject)Game1.player.ActiveObject.getOne();
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
                    Game1.player.addItemToInventory(new SObject(SObject.iridiumBar, 1));
                    Game1.playSound("shiny4");
                }
            }
        }

        /// <summary>Called after a machine updates on time change.</summary>
        /// <param name="machine">The machine that updated.</param>
        /// <param name="location">The location containing the machine.</param>
        private void OnMachineMinutesElapsed(SObject machine, GameLocation location)
        {
            if (machine is Chest { SpecialChestType: Chest.SpecialChestTypes.AutoLoader } chest && chest.heldObject.Value != null && Utility.IsNormalObjectAtParentSheetIndex(chest.heldObject.Value, SObject.iridiumBar))
            {
                location.objects.TryGetValue(chest.TileLocation - new Vector2(0, 1), out SObject aboveObj);
                if (aboveObj is Chest aboveChest && chest.items.Count < chest.GetActualCapacity() && aboveChest.items.Count > 0)
                {
                    chest.items.Add(aboveChest.items[0]);
                    aboveChest.items.RemoveAt(0);
                }
                // Not doing for now because I'd need to handle every machine's special rules, like changing ParentSheetIndex
                /*
                else if ( aboveObj != null && aboveObj?.GetType() == typeof( SObject ) && aboveObj.bigCraftable.Value && aboveObj.MinutesUntilReady == 0 && chest.items.Count < chest.GetActualCapacity() )
                {
                    chest.addItem( aboveObj.heldObject.Value );
                    aboveObj.heldObject.Value = null;
                }
                */

                location.objects.TryGetValue(chest.TileLocation + new Vector2(0, 1), out SObject belowObj);
                if (belowObj is Chest belowChest && chest.items.Count > 0 && belowChest.items.Count < belowChest.GetActualCapacity())
                {
                    belowChest.items.Add(chest.items[0]);
                    chest.items.RemoveAt(0);
                }
            }
        }
    }
}
