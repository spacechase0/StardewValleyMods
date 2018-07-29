using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using Microsoft.Xna.Framework;

namespace FlowerColorPicker
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            instance = this;

            ControlEvents.MouseChanged += onMouseChanged;
        }

        private void onMouseChanged( object sender, EventArgsMouseStateChanged args )
        {
            if (!Context.IsWorldReady)
                return;
            if (!(args.NewState.LeftButton == ButtonState.Pressed && args.PriorState.LeftButton == ButtonState.Released))
                return;

            float mouseTileX = (args.NewPosition.X + Game1.viewport.X) / Game1.tileSize;
            float mouseTileY = (args.NewPosition.Y + Game1.viewport.Y) / Game1.tileSize;
            Vector2 mouseTilePos = new Vector2(mouseTileX, mouseTileY);

            if (!Game1.player.currentLocation.terrainFeatures.ContainsKey(mouseTilePos))
                return;

            var holding = Game1.player.ActiveObject as ColoredObject;
            var hoeDirt = Game1.player.currentLocation.terrainFeatures[mouseTilePos] as HoeDirt;
            if (hoeDirt != null && hoeDirt.crop != null)
            {
                //hoeDirt.crop.growCompletely();
            }
            if (holding == null || hoeDirt == null || hoeDirt.crop == null || holding.ParentSheetIndex != hoeDirt.crop.indexOfHarvest.Value)
                return;
            
            hoeDirt.crop.tintColor.Value = holding.color.Value;
        }
    }
}
