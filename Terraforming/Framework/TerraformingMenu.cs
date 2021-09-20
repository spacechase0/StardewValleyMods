using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace Terraforming.Framework
{
    internal class TerraformingMenu : IClickableMenu
    {
        private readonly int TerrainWidth;
        private readonly int TerrainHeight;
        private readonly TileType[,] TerrainData;
        private readonly Dictionary<TileType, Color> TypeColors = TerraformingMenu.GetCorrespondingColors();

        private TileType Sel = TileType.Dirt;

        public TerraformingMenu()
        : base(0, 0, Game1.viewport.Width, Game1.viewport.Height)
        {
            this.TerrainWidth = Game1.currentLocation.Map.Layers[0].LayerWidth;
            this.TerrainHeight = Game1.currentLocation.Map.Layers[0].LayerHeight;
            this.TerrainData = new TileType[this.TerrainWidth + 1, this.TerrainHeight + 1];
        }

        private bool JustClicked;
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            this.JustClicked = true;
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        public override void releaseLeftClick(int x, int y)
        {
            this.JustClicked = false;
        }

        public override void receiveKeyPress(Keys key)
        {
            if (key == Keys.Escape)
            {
                this.CommitTerrain();
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

            for (int ix = Math.Max(0, minX); ix <= Math.Min(maxX, this.TerrainWidth); ++ix)
            {
                for (int iy = Math.Max(0, minY); iy <= Math.Min(maxY, this.TerrainHeight); ++iy)
                {
                    var type = this.TerrainData[ix, iy];
                    var col = this.TypeColors[type];
                    col.A = 192;

                    Vector2 pos = Game1.GlobalToLocal(new Vector2((ix - 0.5f) * Game1.tileSize, (iy - 0.5f) * Game1.tileSize));
                    Rectangle rect = new Rectangle((int)pos.X + 1, (int)pos.Y + 1, Game1.tileSize - 2, Game1.tileSize - 2);
                    if (rect.Contains(Game1.getMouseX(), Game1.getMouseY()))
                    {
                        col.A = 255;
                        if (this.JustClicked)
                        {
                            this.TerrainData[ix, iy] = type = this.Sel;
                        }
                    }
                    b.Draw(Game1.staminaRect, rect, col);
                }
            }

            // Draw UI
            IClickableMenu.drawTextureBox(b, 64, 64, 64 + IClickableMenu.borderWidth * 2 + 32, IClickableMenu.borderWidth * 2 + (64 + 32) * (int)TileType.Count, Color.White);
            for (int i = 0; i < (int)TileType.Count; ++i)
            {
                TileType type = (TileType)i;
                Color col = this.TypeColors[type];
                int x = 64 + IClickableMenu.borderWidth + 16;
                int y = 64 + IClickableMenu.borderWidth + 16 + (64 + 32) * i;

                Rectangle rect = new Rectangle(x, y, 64, 64);
                if (this.JustClicked && rect.Contains(Game1.getMouseX(), Game1.getMouseY()))
                    this.Sel = type;

                if (type == this.Sel)
                    IClickableMenu.drawTextureBox(b, x - 16, y - 16, 64 + 32, 64 + 32, Color.Green);

                b.Draw(Game1.staminaRect, rect, col);
            }

            this.drawMouse(b);
        }

        public static Dictionary<TileType, Color> GetCorrespondingColors()
        {
            return new()
            {
                [TileType.Dirt] = Color.Goldenrod,
                [TileType.DarkDirt] = Color.DarkGoldenrod,
                [TileType.LightGrass] = Color.LawnGreen,
                [TileType.MediumGrass] = Color.Green,
                [TileType.DarkGrass] = Color.DarkGreen,
                [TileType.Water] = Color.DodgerBlue,
                [TileType.DeepWater] = Color.Blue
            };
        }

        private void CommitTerrain()
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
            var types = new List<TileType>
            {
                TileType.DarkDirt,
                TileType.LightGrass,
                TileType.MediumGrass,
                TileType.DarkGrass
            };

            // Add tile sheets
            var typesTs = new Dictionary<TileType, xTile.Tiles.TileSheet>();
            char tsCounter = '0';
            foreach (var type in types)
            {
                var ts = new xTile.Tiles.TileSheet(Game1.currentLocation.Map,
                                                   Mod.Instance.Helper.Content.GetActualAssetKey($"assets/vanilla/{type}.png"),
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
                for (int ix = 0; ix < this.TerrainWidth; ++ix)
                {
                    for (int iy = 0; iy < this.TerrainHeight; ++iy)
                    {
                        bool GetTile(int x, int y)
                        {
                            if (x < 0 || y < 0 || x > this.TerrainWidth || y > this.TerrainHeight)
                                return false;
                            return this.TerrainData[x, y] == type;
                        }

                        int cornerFlags = 0;
                        if (GetTile(ix + 1, iy + 0))
                            cornerFlags |= 1;
                        if (GetTile(ix + 1, iy + 1))
                            cornerFlags |= 2;
                        if (GetTile(ix + 0, iy + 1))
                            cornerFlags |= 4;
                        if (GetTile(ix + 0, iy + 0))
                            cornerFlags |= 8;

                        if (cornerFlags == 0)
                            continue;

                        layer.Tiles[ix, iy] = new xTile.Tiles.StaticTile(layer, ts, xTile.Tiles.BlendMode.Alpha, tileIndexLookup[cornerFlags]);
                    }
                }
                Game1.currentLocation.Map.AddLayer(layer);
            }

            // Water tile effects
            Game1.currentLocation.waterTiles ??= new bool[this.TerrainWidth, this.TerrainHeight];
            for (int i = 0; i < Game1.currentLocation.waterTiles.waterTiles.Length; ++i)
            {
                int ix = i % this.TerrainWidth, iy = i / this.TerrainWidth;
                var tile = this.TerrainData[ix, iy];
                Game1.currentLocation.waterTiles[ix, iy] = (tile is TileType.Water or TileType.DeepWater);
            }
        }
    }
}
