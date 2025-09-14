using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using Terraforming.Framework;
using xTile;
using xTile.Layers;
using xTile.Tiles;

namespace Terraforming
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;

        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            helper.ConsoleCommands.Add("terraform", "TODO", this.TerraformCommand);
        }

        private void TerraformCommand(string cmd, string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Log.Info("World must be ready");
                return;
            }
            if (Game1.eventUp)
            {
                Log.Info("Probably shouldn't do this during an event");
            }

            Log.Info("Starting up...");
            Mod.SterilizeMap();
            Game1.activeClickableMenu = new TerraformingMenu();
        }

        internal static void SterilizeMap(GameLocation loc = null)
        {
            loc ??= Game1.currentLocation;

            /*
            if (!loc.IsOutdoors)
                throw new NotSupportedException("Location must be outdoors");
            */

            Log.Trace("Creating sterile map...");
            Map map = new Map
            {
                Id = loc.Map.Id + ".Terraform"
            };
            foreach (var prop in loc.Map.Properties)
                map.Properties.Add(prop.Key, prop.Value);
            foreach (var ts in Game1.getFarm().Map.TileSheets)
            {
                var newTs = new TileSheet(ts.Id, map, ts.ImageSource, ts.SheetSize, ts.TileSize);
                foreach (var tsProp in ts.Properties)
                    newTs.Properties.Add(tsProp.Key, tsProp.Value);
                for (int i = 0; i < ts.TileCount; ++i)
                    foreach (var tileProp in ts.TileIndexProperties[i])
                        newTs.TileIndexProperties[i].Add(tileProp.Key, tileProp.Value);
                map.AddTileSheet(newTs);
            }
            foreach (var layer in loc.Map.Layers)
            {
                var newLayer = new Layer(layer.Id, map, layer.LayerSize, layer.TileSize);
                if (newLayer.Id is "Back" or "Buildings" or "Front" or "AlwaysFront")
                    newLayer.AfterDraw += Mod.DrawTerraformLayer;
                if (newLayer.Id == "Back")
                {
                    for (int ix = 0; ix < newLayer.LayerWidth; ix++)
                    {
                        for (int iy = 0; iy < newLayer.LayerHeight; iy++)
                        {
                            var tile = new StaticTile(newLayer, map.TileSheets[1], BlendMode.Alpha, 587);
                            newLayer.Tiles[ix, iy] = tile;
                        }
                    }
                }
                map.AddLayer(newLayer);
            }

            Log.Trace("Replacing location's map with sterile map.");
            loc.Map = map;
            loc.Map.LoadTileSheets(Game1.mapDisplayDevice);
            if (loc.waterTiles != null)
            {
                int w = loc.waterTiles.waterTiles.GetLength(0);
                for (int i = 0; i < loc.waterTiles.waterTiles.Length; ++i)
                {
                    int ix = i % w, iy = i / w;
                    loc.waterTiles[ix, iy] = false;
                }
            }
        }

        private static void DrawTerraformLayer(object sender, LayerEventArgs e)
        {
            foreach (var layer in e.Layer.Map.Layers)
            {
                if (layer.Id.StartsWith(e.Layer.Id + "Terraform"))
                    layer.Draw(Game1.mapDisplayDevice, Game1.viewport, xTile.Dimensions.Location.Origin, false, Game1.pixelZoom);
            }
        }
    }
}
