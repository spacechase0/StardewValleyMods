using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Specialized;
using SObject = StardewValley.Object;

namespace CarryChest
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public override void Entry(IModHelper helper)
        {
            instance = this;

            GameEvents.UpdateTick += update;
            PlayerEvents.Warped += locChanged;
            SaveEvents.AfterLoad += afterLoad;
        }

        private void afterLoad(object sender, EventArgs args)
        {
            Game1.currentLocation.netObjects.OnValueAdded += locObjectsChanged;
        }
        
        private void locChanged( object sender, EventArgsPlayerWarped args )
        {
            if (args.PriorLocation != null)
                args.PriorLocation.netObjects.OnValueAdded -= locObjectsChanged;
            args.NewLocation.netObjects.OnValueAdded += locObjectsChanged;
        }

        private Vector2 mostRecentPos;
        private StardewValley.Object mostRecent;
        private void locObjectsChanged( Vector2 key, SObject value )
        {
            mostRecentPos = (Vector2)key;
            mostRecent = value;
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
