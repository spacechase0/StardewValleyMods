using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI.Events;
using SObject = StardewValley.Object;

namespace CarryChest
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            instance = this;

            helper.Events.GameLoop.UpdateTicked += onUpdateTicked;
            helper.Events.World.ObjectListChanged += onObjectListChanged;
        }

        private Vector2 mostRecentPos;
        private StardewValley.Object mostRecent;

        /// <summary>Raised after objects are added or removed in a location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onObjectListChanged( object sender, ObjectListChangedEventArgs e )
        {
            if (e.Location == Game1.currentLocation && e.Added.Any())
            {
                KeyValuePair<Vector2, SObject> lastObj = e.Added.Last();
                mostRecentPos = lastObj.Key;
                mostRecent = lastObj.Value;
            }
        }

        private int prevSel = -1;
        private StardewValley.Object prevHolding;
        private bool prevMousePressed = false;

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onUpdateTicked( object sender, UpdateTickedEventArgs e )
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree || Game1.activeClickableMenu != null)
                return;

            if (prevHolding is Chest chest && mostRecent is Chest mostRecentChest && Game1.player.ActiveObject == null && Game1.player.CurrentToolIndex == prevSel)
            {
                mostRecentChest.playerChoiceColor.Value = chest.playerChoiceColor.Value;
                if (chest.items.Count > 0)
                {
                    mostRecentChest.items.CopyFrom(chest.items);
                }

                prevSel = Game1.player.CurrentToolIndex;
                prevHolding = Game1.player.ActiveObject;
                prevMousePressed = true;
                return;
            }
            else if ( prevHolding != null && mostRecent != null && Game1.player.ActiveObject == null && Game1.player.CurrentToolIndex == prevSel &&
                      prevHolding.heldObject.Value != null && prevHolding.MinutesUntilReady > 0)
            {
                mostRecent.heldObject.Value = prevHolding.heldObject.Value;
                mostRecent.MinutesUntilReady = prevHolding.MinutesUntilReady;

                prevSel = Game1.player.CurrentToolIndex;
                prevHolding = Game1.player.ActiveObject;
                prevMousePressed = true;
                return;
            }
            prevSel = Game1.player.CurrentToolIndex;
            prevHolding = Game1.player.ActiveObject;

            var mouse = Mouse.GetState();
            if ( mouse.LeftButton == ButtonState.Pressed && !prevMousePressed && 
                 Game1.player.ActiveObject == null && Game1.player.CurrentTool == null)
            {
                Point pos = new Point(Game1.getMouseX() + Game1.viewport.X, Game1.getMouseY() + Game1.viewport.Y);
                Vector2 tile = new Vector2(pos.X / Game1.tileSize, pos.Y / Game1.tileSize);
                
                if (Game1.currentLocation.objects.ContainsKey(tile))
                {
                    var obj = Game1.currentLocation.objects[tile];
                    if (obj is Chest || (obj.ParentSheetIndex != 156 && obj.heldObject.Value != null && obj.MinutesUntilReady > 0))
                    {
                        if (Game1.player.addItemToInventoryBool(obj, true))
                        {
                            Game1.currentLocation.objects.Remove(tile);
                        }
                    }
                }
            }
            prevMousePressed = mouse.LeftButton == ButtonState.Pressed;
        }
    }
}
