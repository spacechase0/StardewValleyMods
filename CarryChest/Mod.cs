using System;
using System.Linq;
using System.Reflection;
using CarryChest.Overrides;
using Harmony;
using Microsoft.Xna.Framework;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace CarryChest
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        private HarmonyInstance harmony;

        /// <summary>The previously selected chest on the toolbar.</summary>
        private Chest previousHeldChest;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            helper.Events.GameLoop.UpdateTicking += onUpdateTicking;
            helper.Events.Input.ButtonPressed += onButtonPressed;
            helper.Events.World.ObjectListChanged += onObjectListChanged;

            harmony = HarmonyInstance.Create(ModManifest.UniqueID);
            doPrefix(typeof(Item), nameof(Item.canStackWith), typeof(ItemCanStackHook));
            doPostfix(typeof(StardewValley.Object), nameof(StardewValley.Object.getDescription), typeof(ObjectDescriptionHook));
        }
        private void doPrefix(Type origType, string origMethod, Type newType)
        {
            doPrefix(origType.GetMethod(origMethod, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static), newType.GetMethod("Prefix"));
        }
        private void doPrefix(MethodInfo orig, MethodInfo prefix)
        {
            try
            {
                Log.trace($"Doing prefix patch {orig}:{prefix}...");
                harmony.Patch(orig, new HarmonyMethod(prefix));
            }
            catch (Exception e)
            {
                Log.error($"Exception doing prefix patch {orig}:{prefix}: {e}");
            }
        }
        private void doPostfix(Type origType, string origMethod, Type newType)
        {
            doPostfix(origType.GetMethod(origMethod, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static), newType.GetMethod("Postfix"));
        }
        private void doPostfix(MethodInfo orig, MethodInfo postfix)
        {
            try
            {
                Log.trace($"Doing postfix patch {orig}:{postfix}...");
                harmony.Patch(orig, null, new HarmonyMethod(postfix));
            }
            catch (Exception e)
            {
                Log.error($"Exception doing postfix patch {orig}:{postfix}: {e}");
            }
        }

        /// <summary>Raised before the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onUpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;

            // track toolbar info before the game handles any user input
            this.previousHeldChest = Game1.player.CurrentItem as Chest;
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;

            // pick up clicked chest
            if (e.Button == SButton.MouseLeft && Game1.player.CurrentItem == null)
            {
                GameLocation location = Game1.currentLocation;
                Vector2 tile = e.Cursor.Tile;
                if (location.objects.TryGetValue(tile, out SObject obj) && obj is Chest chest && chest.playerChest.Value && Game1.player.addItemToInventoryBool(obj, true))
                {
                    location.objects.Remove(tile);
                    this.Helper.Input.Suppress(e.Button);
                }
            }
        }

        /// <summary>Raised after objects are added or removed in a location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;

            // transfer fields for placed chest
            if (this.previousHeldChest != null && e.Location == Game1.currentLocation)
            {
                var placed = e.Added.Select(p => p.Value).OfType<Chest>().LastOrDefault();
                if (placed != null)
                {
                    Chest original = this.previousHeldChest;
                    
                    placed.Name = original.Name;
                    placed.playerChoiceColor.Value = original.playerChoiceColor.Value;
                    placed.heldObject.Value = original.heldObject.Value;
                    placed.MinutesUntilReady = original.MinutesUntilReady;
                    if (original.items.Any())
                        placed.items.CopyFrom(original.items);
                }
            }
        }
    }
}
