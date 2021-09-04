using System.Linq;
using CarryChest.Framework;
using CarryChest.Patches;
using Microsoft.Xna.Framework;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace CarryChest
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;

        /// <summary>The previously selected chest on the toolbar.</summary>
        private Chest PreviousHeldChest;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.World.ObjectListChanged += this.OnObjectListChanged;

            HarmonyPatcher.Apply(this,
                new ItemPatcher(),
                new ObjectPatcher()
            );
        }

        /// <summary>Raised before the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;

            // track toolbar info before the game handles any user input
            this.PreviousHeldChest = Game1.player.CurrentItem as Chest;
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;

            // pick up clicked chest
            if (e.Button == SButton.MouseLeft && Game1.player.CurrentItem == null)
            {
                GameLocation location = Game1.currentLocation;
                Vector2 tile = e.Cursor.Tile;
                if (location.objects.TryGetValue(tile, out SObject obj) && ChestHelper.IsSupported(obj) && Game1.player.addItemToInventoryBool(obj, true))
                {
                    location.objects.Remove(tile);
                    this.Helper.Input.Suppress(e.Button);
                }
            }
        }

        /// <summary>Raised after objects are added or removed in a location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;

            // transfer fields for placed chest
            if (this.PreviousHeldChest != null && e.Location == Game1.currentLocation)
            {
                var placed = e.Added.Select(p => p.Value).OfType<Chest>().LastOrDefault();
                if (placed != null)
                {
                    Chest original = this.PreviousHeldChest;

                    placed.Name = original.Name;
                    placed.playerChoiceColor.Value = original.playerChoiceColor.Value;
                    placed.heldObject.Value = original.heldObject.Value;
                    placed.MinutesUntilReady = original.MinutesUntilReady;
                    if (original.items.Any())
                        placed.items.CopyFrom(original.items);
                    foreach (var modData in original.modData)
                        placed.modData.CopyFrom(modData);
                }
            }
        }
    }
}
