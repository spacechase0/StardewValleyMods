using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace Terraforming
{
    public class TerraformingMenu : IClickableMenu
    {
        private int terrainWidth, terrainHeight;
        private TileType[,] terrainData;
        private Dictionary<TileType, Color> typeColors = GetCorrespondingColors();

        private TileType sel = TileType.Dirt;

        public TerraformingMenu()
        : base(0, 0, Game1.viewport.Width, Game1.viewport.Height, false)
        {
            terrainWidth = Game1.currentLocation.Map.Layers[0].LayerWidth;
            terrainHeight = Game1.currentLocation.Map.Layers[0].LayerHeight;
            terrainData = new TileType[terrainWidth + 1, terrainHeight + 1];
        }

        private bool justClicked = false;
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            justClicked = true;
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        public override void releaseLeftClick(int x, int y)
        {
            justClicked = false;
        }

        public override void receiveKeyPress(Keys key)
        {
            if (key == Keys.Escape)
            {
                commitTerrain();
                Game1.exitActiveMenu();
            }
        }

        public override void update(GameTime time)
        {
            if (Game1.isOneOfTheseKeysDown(Game1.GetKeyboardState(), Game1.options.moveDownButton))
                Game1.panScreen(0, 4);
            if (Game1.isOneOfTheseKeysDown(Game1.GetKeyboardState(), Game1.options.moveUpButton))
                Game1.panScreen(0, -4);
            if (Game1.isOneOfTheseKeysDown(Game1.GetKeyboardState(), Game1.options.moveRightButton))
                Game1.panScreen(4, 0);
            if (Game1.isOneOfTheseKeysDown(Game1.GetKeyboardState(), Game1.options.moveLeftButton))
                Game1.panScreen(-4, 0);
        }

        public override void draw(SpriteBatch b)
        {
            // Draw tiles
            int minX = Game1.viewport.X / 64;
            int minY = Game1.viewport.Y / 64;
            int maxX = minX + Game1.viewport.Width / 64;
            int maxY = minY + Game1.viewport.Height / 64;

            for (int ix = Math.Max(0, minX); ix <= Math.Min(maxX, terrainWidth); ++ix)
            {
                for (int iy = Math.Max(0, minY); iy <= Math.Min(maxY, terrainHeight); ++iy)
                {
                    var type = terrainData[ix, iy];
                    var col = typeColors[type];
                    col.A = 192;

                    Vector2 pos = Game1.GlobalToLocal(new Vector2((ix - 0.5f) * Game1.tileSize, (iy - 0.5f) * Game1.tileSize));
                    Rectangle rect = new Rectangle((int)pos.X + 1, (int)pos.Y + 1, Game1.tileSize - 2, Game1.tileSize - 2);
                    if (rect.Contains(Game1.getMouseX(), Game1.getMouseY()))
                    {
                        col.A = 255;
                        if (justClicked)
                        {
                            terrainData[ix, iy] = type = sel;
                        }
                    }
                    b.Draw(Game1.staminaRect, rect, col);
                }
            }

            // Draw UI
            drawTextureBox(b, 64, 64, 64 + IClickableMenu.borderWidth * 2 + 32, IClickableMenu.borderWidth * 2 + (64 + 32) * (int)TileType.Count, Color.White);
            for (int i = 0; i < (int)TileType.Count; ++i)
            {
                TileType type = (TileType)i;
                Color col = typeColors[type];
                int x = 64 + IClickableMenu.borderWidth + 16;
                int y = 64 + IClickableMenu.borderWidth + 16 + (64 + 32) * i;

                Rectangle rect = new Rectangle(x, y, 64, 64);
                if (justClicked && rect.Contains(Game1.getMouseX(), Game1.getMouseY()))
                    sel = type;

                if (type == sel)
                    drawTextureBox(b, x - 16, y - 16, 64 + 32, 64 + 32, Color.Green);

                b.Draw(Game1.staminaRect, rect, col);
            }

            drawMouse(b);
        }

        public static Dictionary<TileType, Color> GetCorrespondingColors()
        {
            var cols = new Dictionary<TileType, Color>();
            cols.Add(TileType.Dirt, Color.Goldenrod);
            cols.Add(TileType.DarkDirt, Color.DarkGoldenrod);
            cols.Add(TileType.LightGrass, Color.LawnGreen);
            cols.Add(TileType.MediumGrass, Color.Green);
            cols.Add(TileType.DarkGrass, Color.DarkGreen);
            cols.Add(TileType.Water, Color.DodgerBlue);
            cols.Add(TileType.DeepWater, Color.Blue);
            return cols;
        }

        private void commitTerrain()
        {
            // Remove old layers
            var layersToRemove = new List<xTile.Layers.Layer>();
            foreach (var check in Game1.currentLocation.Map.Layers)
            {
                if (check.Id.Contains("Terraform"))
                    layersToRemove.Add(check);
            }
            foreach (var remove in layersToRemove)
                Game1.currentLocation.Map.RemoveLayer(remove);

            // Remove old tilesheets
            var sheetsToRemove = new List<xTile.Tiles.TileSheet>();
            foreach (var check in Game1.currentLocation.Map.TileSheets)
            {
                if (check.Id.Contains("Terraform"))
                    sheetsToRemove.Add(check);
            }
            foreach (var remove in sheetsToRemove)
                Game1.currentLocation.Map.RemoveTileSheet(remove);

            // Collect tile types
            var types = new List<TileType>();
            types.Add(TileType.DarkDirt);
            types.Add(TileType.LightGrass);
            types.Add(TileType.MediumGrass);
            types.Add(TileType.DarkGrass);

            // Add tile sheets
            var typesTs = new Dictionary<TileType, xTile.Tiles.TileSheet>();
            char tsCounter = '0';
            foreach (var type in types)
            {
                var ts = new xTile.Tiles.TileSheet(Game1.currentLocation.Map,
                                                   Mod.instance.Helper.Content.GetActualAssetKey($"assets/vanilla/{type}.png"),
                                                   new xTile.Dimensions.Size(4, 4), new xTile.Dimensions.Size(16, 16));
                ts.Id = "\u03a9" + ts.Id + "Terraform" + (tsCounter++) + type.ToString();
                Game1.currentLocation.Map.AddTileSheet(ts);
                typesTs.Add(type, ts);
            }
            Game1.currentLocation.Map.LoadTileSheets(Game1.mapDisplayDevice);

            int[] tileIndexLookup = new[] { 12, 8, 13, 1, 0, 14, 3, 5, 15, 9, 4, 10, 11, 7, 2, 6 };

            // Add our tiles, by layer
            foreach (var type in types)
            {
                var layer = new xTile.Layers.Layer("BackTerraform_" + type, Game1.currentLocation.Map, Game1.currentLocation.Map.Layers[0].LayerSize, new xTile.Dimensions.Size(Game1.tileSize, Game1.tileSize));
                var ts = typesTs[type];
                for (int ix = 0; ix < terrainWidth; ++ix)
                {
                    for (int iy = 0; iy < terrainHeight; ++iy)
                    {
                        Func<int, int, bool> getTile =
                        (x, y) =>
                        {
                            if (x < 0 || y < 0 || x > terrainWidth || y > terrainHeight)
                                return false;
                            return terrainData[x, y] == type;
                        };

                        int cornerFlags = 0;
                        if (getTile(ix + 1, iy + 0))
                            cornerFlags |= 1;
                        if (getTile(ix + 1, iy + 1))
                            cornerFlags |= 2;
                        if (getTile(ix + 0, iy + 1))
                            cornerFlags |= 4;
                        if (getTile(ix + 0, iy + 0))
                            cornerFlags |= 8;

                        if (cornerFlags == 0)
                            continue;

                        layer.Tiles[ix, iy] = new xTile.Tiles.StaticTile(layer, ts, xTile.Tiles.BlendMode.Alpha, tileIndexLookup[cornerFlags]);
                    }
                }
                Game1.currentLocation.Map.AddLayer(layer);
            }

            // Water tile effects
            if (Game1.currentLocation.waterTiles == null)
            {
                Game1.currentLocation.waterTiles = new bool[terrainWidth, terrainHeight];
            }
            for (int i = 0; i < Game1.currentLocation.waterTiles.Length; ++i)
            {
                int ix = i % terrainWidth, iy = i / terrainWidth;
                var tile = terrainData[ix, iy];
                Game1.currentLocation.waterTiles[ix, iy] = (tile == TileType.Water || tile == TileType.DeepWater);
            }
        }
    }
}
