using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewModdingAPI;
using Tiled;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace SpaceCore
{
    public class Content
    {
        private class TileMapping
        {
            public TileSheet TileSheet;
            public int TileId;

            public TileMapping() { }
            public TileMapping(TileSheet ts, int id)
            {
                this.TileSheet = ts;
                this.TileId = id;
            }
        }

        public class TileAnimation
        {
            public int[] TileIds = new int[0];
            public int Duration;

            public TileAnimation() { }
            public TileAnimation(int[] ids, int frameLen)
            {
                this.TileIds = ids;
                this.Duration = frameLen;
            }

            public AnimatedTile MakeTile(TileSheet ts, Layer xLayer)
            {
                var tileAnimation = this;

                var animTiles = new StaticTile[tileAnimation.TileIds.Length];
                for (int ia = 0; ia < animTiles.Length; ++ia)
                    animTiles[ia] = new StaticTile(xLayer, ts, BlendMode.Alpha, this.TileIds[ia]);
                return new AnimatedTile(xLayer, animTiles, tileAnimation.Duration);
            }
        }

        public static Map LoadTmx(IModHelper modHelper, string mapName, string path)
        {
            var tMap = new TiledMap(Path.Combine(modHelper.DirectoryPath, path));
            var xMap = new Map(mapName);
            Content.AddTiledPropertiesToXTile(tMap.Properties, xMap.Properties);

            var tileMapping = new Dictionary<int, TileMapping>();
            var animMapping = new Dictionary<int, TileAnimation>();
            foreach (var tTileSheet in tMap.Tilesets)
            {
                // xTile wants things like "Mines/mine", not "Mines/mine.png"
                string image = tTileSheet.Image.Source;
                if (image.EndsWith(".png"))
                {
                    string dir = Path.GetDirectoryName(path);
                    string tempKey = Path.Combine(dir, image);
                    string actualKey = modHelper.Content.GetActualAssetKey(tempKey);
                    Log.Debug($"{dir} | {image} | {tempKey} | {actualKey} ");
                    image = Path.Combine(Path.GetDirectoryName(path), image);
                    //modHelper.Content.Load<Texture2D>(image);
                    image = modHelper.Content.GetActualAssetKey(image);
                }

                var xTileSheet = new TileSheet(xMap, image, new Size(tTileSheet.Columns, tTileSheet.TileCount / tTileSheet.Columns), new Size(tMap.TileWidth, tMap.TileHeight));
                Content.AddTiledPropertiesToXTile(tTileSheet.Properties, xTileSheet.Properties);
                xTileSheet.Id = tTileSheet.Name;
                xTileSheet.Spacing = new Size(tTileSheet.Spacing, tTileSheet.Spacing);
                xTileSheet.Margin = new Size(tTileSheet.Margin, tTileSheet.Margin);
                for (int i = 0; i < tTileSheet.TileCount; ++i)
                {
                    tileMapping.Add(tTileSheet.FirstGlobalId + i, new TileMapping(xTileSheet, i));
                }
                foreach (var tTile in tTileSheet.Tiles)
                {
                    Content.AddTiledPropertiesToXTile(tTile.Properties, xTileSheet.TileIndexProperties[tTile.Id]);

                    if (tTile.Animation != null && tTile.Animation.Count > 0)
                    {
                        List<int> tAnimFrames = new List<int>();
                        foreach (var tTileAnim in tTile.Animation)
                            tAnimFrames.Add(tTileSheet.FirstGlobalId + tTileAnim.TileId);
                        animMapping.Add(tTileSheet.FirstGlobalId + tTile.Id, new TileAnimation(tAnimFrames.ToArray<int>(), tTile.Animation[0].Duration));
                    }
                }
                xMap.AddTileSheet(xTileSheet);
            }

            var tObjectGroups = new List<TiledObjectGroup>();
            foreach (var rawLayer in tMap.Layers)
            {
                if (rawLayer is TiledTileLayer tLayer)
                {
                    // Note that the tile size needs to be * 4. Otherwise, you will break collisions and many other things.
                    // Yes, even if you don't use the loaded map. Creating the layer is enough.
                    // For some reason vanilla has a tilesize of 16 for tilesheets, but 64 for the layers.
                    // I mean, I knew the game was scaled up, but that's kinda odd.
                    // Anyways, whenever you create a layer with a different tile size, it changes the tile size
                    // of EVERY OTHER LAYER IN EXISTANCE to match. And guess what, that breaks things.
                    // I spent hours figuring this out. I don't care about the underlying cause. I just want to mod.
                    var xLayer = new Layer(tLayer.Name, xMap, new Size(tMap.Width, tMap.Height), new Size(tMap.TileWidth * 4, tMap.TileHeight * 4));
                    Content.AddTiledPropertiesToXTile(tLayer.Properties, xLayer.Properties);
                    if (tLayer.Data.Compression != TiledData.CompressionType.NoCompression)
                        throw new InvalidDataException("Compressed tile data is not supported.");
                    if (tLayer.Data.Encoding == TiledData.EncodingType.NoEncoding || tLayer.Data.Encoding == TiledData.EncodingType.Xml)
                    {
                        for (int i = 0; i < tLayer.Data.Tiles.Count; ++i)
                        {
                            var tTile = tLayer.Data.Tiles[i];
                            int ix = i % tMap.Width;
                            int iy = i / tMap.Width;

                            var xTile = new StaticTile(xLayer, tileMapping[tTile.GlobalId].TileSheet, BlendMode.Alpha, tileMapping[tTile.GlobalId].TileId);
                            xLayer.Tiles[ix, iy] = xTile;
                        }
                    }
                    else if (tLayer.Data.Encoding == TiledData.EncodingType.Csv)
                    {
                        string[] tTiles = string.Join("", tLayer.Data.Data).Split(',');
                        for (int i = 0; i < tTiles.Length; ++i)
                        {
                            int tTile = int.Parse(tTiles[i]);
                            if (!tileMapping.ContainsKey(tTile))
                                continue;

                            int ix = i % tMap.Width;
                            int iy = i / tMap.Width;

                            Tile xTile = null;
                            if (animMapping.ContainsKey(tTile))
                            {
                                TileAnimation tAnim = animMapping[tTile];
                                var xAnimTiles = new StaticTile[tAnim.TileIds.Length];
                                for (int ia = 0; ia < xAnimTiles.Length; ++ia)
                                    xAnimTiles[ia] = new StaticTile(xLayer, tileMapping[tAnim.TileIds[ia]].TileSheet, BlendMode.Alpha, tileMapping[tAnim.TileIds[ia]].TileId);
                                xTile = new AnimatedTile(xLayer, xAnimTiles, tAnim.Duration);
                            }
                            else
                                xTile = new StaticTile(xLayer, tileMapping[tTile].TileSheet, BlendMode.Alpha, tileMapping[tTile].TileId);
                            xLayer.Tiles[ix, iy] = xTile;
                        }
                    }
                    else throw new InvalidDataException("Tile data encoding type " + tLayer.Data.Encoding + " not supported.");
                    xMap.AddLayer(xLayer);
                }
                else if (rawLayer is TiledObjectGroup tiledObjectGroup)
                {
                    tObjectGroups.Add(tiledObjectGroup);
                }
            }

            foreach (var tObjectGroup in tObjectGroups)
            {
                var xLayer = xMap.GetLayer(tObjectGroup.Name);
                if (xLayer == null)
                    continue;

                foreach (var tObj in tObjectGroup.Objects)
                {
                    if (tObj.Name != "TileData" || tObj.Width != tMap.TileWidth || tObj.Height != tMap.TileWidth || tObj.Properties.Count == 0)
                        continue;
                    int x = (int)tObj.X / tMap.TileWidth;
                    int y = (int)tObj.Y / tMap.TileWidth;

                    if (xLayer.Tiles[new Location(x, y)] == null)
                    {
                        Log.Warn("Tile property for non-existant tile; skipping");
                        continue;
                    }
                    Content.AddTiledPropertiesToXTile(tObj.Properties, xLayer.Tiles[new Location(x, y)].Properties);
                }
            }

            return xMap;
        }

        private static readonly XmlSerializer TilesheetSerializer = new(typeof(TiledTileset), new XmlRootAttribute("tileset"));
        public static TileSheet LoadTsx(IModHelper modHelper, string path, string ts, Map xmap, out Dictionary<int, TileAnimation> animMapping)
        {
            TiledTileset ttileSheet = null;
            Stream stream = null;
            try
            {
                stream = new FileStream(Path.Combine(modHelper.DirectoryPath, path), FileMode.Open);
                ttileSheet = (TiledTileset)Content.TilesheetSerializer.Deserialize(stream);
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            animMapping = new Dictionary<int, TileAnimation>();

            // xTile wants things like "Mines/mine", not "Mines/mine.png"
            string image = ttileSheet.Image.Source;
            if (image.EndsWith(".png"))
            {
                image = Path.Combine(Path.GetDirectoryName(path), image);
                modHelper.Content.Load<Texture2D>(image);
                image = modHelper.Content.GetActualAssetKey(image);
            }

            var xtileSheet = new TileSheet(xmap, image, new Size(ttileSheet.Columns, ttileSheet.TileCount / ttileSheet.Columns), new Size(ttileSheet.TileWidth, ttileSheet.TileHeight));
            Content.AddTiledPropertiesToXTile(ttileSheet.Properties, xtileSheet.Properties);
            xtileSheet.Id = ttileSheet.Name;
            xtileSheet.Spacing = new Size(ttileSheet.Spacing, ttileSheet.Spacing);
            xtileSheet.Margin = new Size(ttileSheet.Margin, ttileSheet.Margin);
            foreach (var tTile in ttileSheet.Tiles)
            {
                Content.AddTiledPropertiesToXTile(tTile.Properties, xtileSheet.TileIndexProperties[tTile.Id]);

                if (tTile.Animation != null && tTile.Animation.Count > 0)
                {
                    List<int> tAnimFrames = new List<int>();
                    foreach (var tTileAnim in tTile.Animation)
                        tAnimFrames.Add(ttileSheet.FirstGlobalId + tTileAnim.TileId);
                    animMapping.Add(ttileSheet.FirstGlobalId + tTile.Id, new TileAnimation(tAnimFrames.ToArray<int>(), tTile.Animation[0].Duration));
                }
            }

            return xtileSheet;
        }

        private static void AddTiledPropertiesToXTile(List<TiledProperty> tProps, IPropertyCollection xProps)
        {
            foreach (var tProp in tProps)
            {
                if (tProp.Type == TiledProperty.PropertyType.String)
                    xProps.Add(tProp.Name, tProp.Value);
                else if (tProp.Type == TiledProperty.PropertyType.Float)
                    xProps.Add(tProp.Name, float.Parse(tProp.Value));
                else if (tProp.Type == TiledProperty.PropertyType.Int)
                    xProps.Add(tProp.Name, int.Parse(tProp.Value));
                else if (tProp.Type == TiledProperty.PropertyType.Bool)
                    xProps.Add(tProp.Name, bool.Parse(tProp.Value));
                else
                    Log.Warn("Bad tilesheet tile property type: " + tProp.Type + " " + tProp.Name + " (not supported by xTile)");
            }
        }
    }
}
