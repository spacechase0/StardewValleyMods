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

        private Chest mostRecentChest;
        private Vector2 mostRecent;
        private void locObjectsChanged( object sender, NotifyCollectionChangedEventArgs args )
        {
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                mostRecent = (Vector2)args.NewItems[0];
                var obj = Game1.currentLocation.objects[(Vector2)args.NewItems[0]];
                if (obj is Chest chest)
                {
                    mostRecentChest = chest;
                }
            }
        }
        
        private int prevSel = -1;
        private StardewValley.Object prevHolding;
        private bool prevMousePressed = false;
        private void update( object sender, EventArgs args )
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree || Game1.activeClickableMenu != null)
                return;

            if (prevHolding is Chest && Game1.player.ActiveObject == null && Game1.player.CurrentToolIndex == prevSel)
            {
                Chest chest = prevHolding as Chest;
                if ( chest.items.Count > 0 && mostRecentChest != null )
                {
                    mostRecentChest.items = chest.items;
                }
                prevSel = Game1.player.CurrentToolIndex;
                prevHolding = Game1.player.ActiveObject;
                prevMousePressed = true;
                return;
            }
            else if ( prevHolding != null && Game1.player.ActiveObject == null && Game1.player.CurrentToolIndex == prevSel )
            {
                Game1.currentLocation.objects[mostRecent] = prevHolding;
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
                    if (obj is Chest || (obj.heldObject != null && obj.minutesUntilReady > 0))
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
