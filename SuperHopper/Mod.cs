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
        ** Fields
        *********/
        /// <summary>The <see cref="Item.modData"/> flag which indicates a hopper is a super hopper.</summary>
        private readonly string ModDataFlag = "spacechase0.SuperHopper";


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
            if (this.TryGetHopper(obj, out Chest chest) && e.Button.IsActionButton())
            {
                if (chest.heldObject.Value == null)
                {
                    if (Utility.IsNormalObjectAtParentSheetIndex(Game1.player.ActiveObject, SObject.iridiumBarID))
                    {
                        chest.Tint = Color.DarkViolet;
                        chest.heldObject.Value = (SObject)Game1.player.ActiveObject.getOne();
                        chest.modData[this.ModDataFlag] = "1";

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
                    chest.modData.Remove(this.ModDataFlag);

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
            // not super hopper
            if (!this.TryGetHopper(machine, out Chest hopper) || hopper.heldObject.Value == null || !Utility.IsNormalObjectAtParentSheetIndex(hopper.heldObject.Value, SObject.iridiumBarID))
                return;

            // fix flag if needed
            if (!hopper.modData.ContainsKey(this.ModDataFlag))
                hopper.modData[this.ModDataFlag] = "1";

            // check for bottom chest
            if (!location.objects.TryGetValue(hopper.TileLocation + new Vector2(0, 1), out SObject objBelow) || objBelow is not Chest chestBelow)
                return;

            // transfer current inventory if any
            hopper.clearNulls();
            var hopperItems = hopper.GetItemsForPlayer(hopper.owner.Value);
            for (int i = hopperItems.Count - 1; i >= 0; i--)
            {
                Item item = hopperItems[i];
                if (chestBelow.addItem(item) == null)
                    hopperItems.RemoveAt(i);
            }

            // check for top chest
            if (!location.objects.TryGetValue(hopper.TileLocation - new Vector2(0, 1), out SObject objAbove) || objAbove is not Chest chestAbove)
                return;

            // transfer items
            chestAbove.clearNulls();
            var chestAboveItems = chestAbove.GetItemsForPlayer(hopper.owner.Value);
            for (int i = chestAboveItems.Count - 1; i >= 0; i--)
            {
                Item item = chestAboveItems[i];
                if (chestBelow.addItem(item) == null)
                    chestAboveItems.RemoveAt(i);
            }
        }

        /// <summary>Get the hopper instance if the object is a hopper.</summary>
        /// <param name="obj">The object to check.</param>
        /// <param name="hopper">The hopper instance.</param>
        /// <returns>Returns whether the object is a hopper.</returns>
        private bool TryGetHopper(SObject obj, out Chest hopper)
        {
            if (obj is Chest { SpecialChestType: Chest.SpecialChestTypes.AutoLoader } chest)
            {
                hopper = chest;
                return true;
            }

            hopper = null;
            return false;
        }
    }
}
