using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Specialized;

namespace CarryChest
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public override void Entry(IModHelper helper)
        {
            base.Entry(helper);
            instance = this;

            GameEvents.UpdateTick += update;
            LocationEvents.CurrentLocationChanged += locChanged;
        }
        
        private void locChanged( object sender, EventArgsCurrentLocationChanged args )
        {
            if (args.PriorLocation != null)
                args.PriorLocation.objects.CollectionChanged -= locObjectsChanged;
            args.NewLocation.objects.CollectionChanged += locObjectsChanged;
        }

        private Vector2 mostRecentPos;
        private StardewValley.Object mostRecent;
        private void locObjectsChanged( object sender, NotifyCollectionChangedEventArgs args )
        {
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                mostRecentPos = (Vector2)args.NewItems[0];
                mostRecent = Game1.currentLocation.objects[mostRecentPos];
            }
        }
        
        private int prevSel = -1;
        private StardewValley.Object prevHolding;
        private bool prevMousePressed = false;
        private void update( object sender, EventArgs args )
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree || Game1.activeClickableMenu != null)
                return;

            if (prevHolding is Chest chest && mostRecent is Chest mostRecentChest && Game1.player.ActiveObject == null && Game1.player.CurrentToolIndex == prevSel)
            {
                mostRecentChest.playerChoiceColor = chest.playerChoiceColor;
                if (chest.items.Count > 0)
                    mostRecentChest.items = chest.items;

                prevSel = Game1.player.CurrentToolIndex;
                prevHolding = Game1.player.ActiveObject;
                prevMousePressed = true;
                return;
            }
            else if ( prevHolding != null && mostRecent != null && Game1.player.ActiveObject == null && Game1.player.CurrentToolIndex == prevSel &&
                      prevHolding.heldObject != null && prevHolding.minutesUntilReady > 0)
            {
                mostRecent.heldObject = prevHolding.heldObject;
                mostRecent.minutesUntilReady = prevHolding.minutesUntilReady;

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
                    if (obj is Chest || (obj.parentSheetIndex != 156 && obj.heldObject != null && obj.minutesUntilReady > 0))
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
