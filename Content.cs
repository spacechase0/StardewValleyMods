using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile;
using xTile.Layers;
using xTile.Dimensions;
using xTile.Tiles;
using StardewModdingAPI;
using Tiled;
using System.IO;
using xTile.ObjectModel;
using Microsoft.Xna.Framework.Content;
using System.Xml.Serialization;

namespace SpaceCore
{
    public class Content
    {
        private class TileMapping
        {
            public TileSheet tileSheet = null;
            public int tileId = 0;

            public TileMapping() { }
            public TileMapping( TileSheet ts, int id )
            {
                tileSheet = ts;
                tileId = id;
            }
        }

        public class TileAnimation
        {
            public int[] tileIds = new int[ 0 ];
            public int duration;

            public TileAnimation() { }
            public TileAnimation(int[] ids, int frameLen)
            {
                tileIds = ids;
                duration = frameLen;
            }

            public AnimatedTile makeTile( TileSheet ts, Layer xlayer )
            {
                var tanim = this;

                var xanimTiles = new StaticTile[tanim.tileIds.Length];
                for (int ia = 0; ia < xanimTiles.Length; ++ia)
                    xanimTiles[ia] = new StaticTile(xlayer, ts, BlendMode.Alpha, tileIds[ ia ]);
                return new AnimatedTile(xlayer, xanimTiles, tanim.duration);
            }
        }

        public static Map loadTmx(IModHelper modHelper, string mapName, string path)
        {
            var tmap = new TiledMap(Path.Combine(modHelper.DirectoryPath, path));
            var xmap = new Map(mapName);
            addTiledPropertiesToXTile(tmap.Properties, xmap.Properties);
            
            var tileMapping = new Dictionary<int, TileMapping>();
            var animMapping = new Dictionary<int, TileAnimation>();
            foreach ( var ttileSheet in tmap.Tilesets )
            {
                // xTile wants things like "Mines/mine", not "Mines/mine.png"
                string image = ttileSheet.Image.Source;
                if (image.EndsWith(".png"))
                {
                    string dir = Path.GetDirectoryName(path);
                    string tempKey = Path.Combine(dir, image);
                    string actualKey = modHelper.Content.GetActualAssetKey(tempKey);
                    Log.debug($"{dir} | {image} | {tempKey} | {actualKey} ");
                    image = Path.Combine(Path.GetDirectoryName(path), image);
                    //modHelper.Content.Load<Texture2D>(image);
                    image = modHelper.Content.GetActualAssetKey(image);
                }

                var xtileSheet = new TileSheet(xmap, image, new Size(ttileSheet.Columns, ttileSheet.TileCount / ttileSheet.Columns), new Size(tmap.TileWidth, tmap.TileHeight));
                addTiledPropertiesToXTile(ttileSheet.Properties, xtileSheet.Properties);
                xtileSheet.Id = ttileSheet.Name;
                xtileSheet.Spacing = new Size( ttileSheet.Spacing, ttileSheet.Spacing);
                xtileSheet.Margin = new Size(ttileSheet.Margin, ttileSheet.Margin);
                for (int i = 0; i < ttileSheet.TileCount; ++i)
                {
                    tileMapping.Add(ttileSheet.FirstGlobalId + i, new TileMapping(xtileSheet, i));
                }
                foreach (var ttile in ttileSheet.Tiles)
                {
                    addTiledPropertiesToXTile(ttile.Properties, xtileSheet.TileIndexProperties[ttile.Id]);
                    
                    if (ttile.Animation != null && ttile.Animation.Count > 0)
                    {
                        List<int> tanimFrames = new List<int>();
                        foreach (var ttileAnim in ttile.Animation)
                            tanimFrames.Add(ttileSheet.FirstGlobalId + ttileAnim.TileId);
                        animMapping.Add(ttileSheet.FirstGlobalId + ttile.Id, new TileAnimation(tanimFrames.ToArray<int>(), ttile.Animation[0].Duration));
                    }
                }
                xmap.AddTileSheet(xtileSheet);
            }

            var tobjectGroups = new List<TiledObjectGroup>();
            foreach (var tlayer_ in tmap.Layers)
            {
                if (tlayer_ is TiledTileLayer)
                {
                    var tlayer = tlayer_ as TiledTileLayer;
                    // Note that the tile size needs to be * 4. Otherwise, you will break collisions and many other things.
                    // Yes, even if you don't use the loaded map. Creating the layer is enough.
                    // For some reason vanilla has a tilesize of 16 for tilesheets, but 64 for the layers.
                    // I mean, I knew the game was scaled up, but that's kinda odd.
                    // Anyways, whenever you create a layer with a different tile size, it changes the tile size
                    // of EVERY OTHER LAYER IN EXISTANCE to match. And guess what, that breaks things.
                    // I spent hours figuring this out. I don't care about the underlying cause. I just want to mod.
                    var xlayer = new Layer(tlayer.Name, xmap, new Size(tmap.Width, tmap.Height), new Size(tmap.TileWidth * 4, tmap.TileHeight * 4));
                    addTiledPropertiesToXTile(tlayer.Properties, xlayer.Properties);
                    if (tlayer.Data.Compression != TiledData.CompressionType.NoCompression)
                        throw new InvalidDataException("Compressed tile data is not supported.");
                    if (tlayer.Data.Encoding == TiledData.EncodingType.NoEncoding || tlayer.Data.Encoding == TiledData.EncodingType.Xml)
                    {
                        for (int i = 0; i < tlayer.Data.Tiles.Count; ++i)
                        {
                            var ttile = tlayer.Data.Tiles[i];
                            int ix = i % tmap.Width;
                            int iy = i / tmap.Width;
                            
                            var xtile = new StaticTile(xlayer, tileMapping[ttile.GlobalId].tileSheet, BlendMode.Alpha, tileMapping[ttile.GlobalId].tileId);
                            xlayer.Tiles[ix, iy] = xtile;
                        }
                    }
                    else if (tlayer.Data.Encoding == TiledData.EncodingType.Csv)
                    {
                        string[] ttiles = string.Join("", tlayer.Data.Data).Split(',');
                        for (int i = 0; i < ttiles.Length; ++i)
                        {
                            var ttile = int.Parse(ttiles[i]);
                            if (!tileMapping.ContainsKey(ttile))
                                continue;
                            int ix = i % tmap.Width;
                            int iy = i / tmap.Width;

                            Tile xtile = null;
                            if (animMapping.ContainsKey(ttile))
                            {
                                TileAnimation tanim = animMapping[ttile];
                                var xanimTiles = new StaticTile[tanim.tileIds.Length];
                                for (int ia = 0; ia < xanimTiles.Length; ++ia)
                                    xanimTiles[ia] = new StaticTile(xlayer, tileMapping[tanim.tileIds[ia]].tileSheet, BlendMode.Alpha, tileMapping[tanim.tileIds[ia]].tileId);
                                xtile = new AnimatedTile(xlayer, xanimTiles, tanim.duration);
                            }
                            else
                                xtile = new StaticTile(xlayer, tileMapping[ttile].tileSheet, BlendMode.Alpha, tileMapping[ttile].tileId);
                            xlayer.Tiles[ix, iy] = xtile;
                        }
                    }
                    else throw new InvalidDataException("Tile data encoding type " + tlayer.Data.Encoding + " not supported.");
                    xmap.AddLayer(xlayer);
                }
                else if (tlayer_ is TiledObjectGroup)
                {
                    tobjectGroups.Add(tlayer_ as TiledObjectGroup);
                }
            }
            
            foreach ( var tobjectGroup in tobjectGroups )
            {
                var xlayer = xmap.GetLayer(tobjectGroup.Name);
                if (xlayer == null)
                    continue;

                foreach (var tobj in tobjectGroup.Objects)
                {
                    if (tobj.Name != "TileData" || tobj.Width != tmap.TileWidth || tobj.Height != tmap.TileWidth || tobj.Properties.Count == 0)
                        continue;
                    int x = (int)tobj.X / tmap.TileWidth;
                    int y = (int)tobj.Y / tmap.TileWidth;
                    
                    if (xlayer.Tiles[new Location(x, y)] == null)
                    {
                        Log.warn("Tile property for non-existant tile; skipping");
                        continue;
                    }
                    addTiledPropertiesToXTile(tobj.Properties, xlayer.Tiles[new Location(x, y)].Properties);
                }
            }

            return xmap;
        }

        private static XmlSerializer tilesheetSerializer = new XmlSerializer(typeof(TiledTileset), new XmlRootAttribute("tileset"));
        public static TileSheet loadTsx(IModHelper modHelper, string path, string ts, Map xmap, out Dictionary< int, TileAnimation > animMapping )
        {
            TiledTileset ttileSheet = null;
            Stream stream = null;
            try
            {
                stream = new FileStream(Path.Combine(modHelper.DirectoryPath, path), FileMode.Open);
                ttileSheet = ( TiledTileset ) tilesheetSerializer.Deserialize(stream);
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
            addTiledPropertiesToXTile(ttileSheet.Properties, xtileSheet.Properties);
            xtileSheet.Id = ttileSheet.Name;
            xtileSheet.Spacing = new Size(ttileSheet.Spacing, ttileSheet.Spacing);
            xtileSheet.Margin = new Size(ttileSheet.Margin, ttileSheet.Margin);
            foreach (var ttile in ttileSheet.Tiles)
            {
                addTiledPropertiesToXTile(ttile.Properties, xtileSheet.TileIndexProperties[ttile.Id]);

                if (ttile.Animation != null && ttile.Animation.Count > 0)
                {
                    List<int> tanimFrames = new List<int>();
                    foreach (var ttileAnim in ttile.Animation)
                        tanimFrames.Add(ttileSheet.FirstGlobalId + ttileAnim.TileId);
                    animMapping.Add(ttileSheet.FirstGlobalId + ttile.Id, new TileAnimation(tanimFrames.ToArray<int>(), ttile.Animation[0].Duration));
                }
            }

            return xtileSheet;
        }

        private static void addTiledPropertiesToXTile( List< TiledProperty > tprops, IPropertyCollection xprops )
        {
            foreach (var tprop in tprops)
            {
                if (tprop.Type == TiledProperty.PropertyType.String)
                    xprops.Add(tprop.Name, tprop.Value);
                else if (tprop.Type == TiledProperty.PropertyType.Float)
                    xprops.Add(tprop.Name, float.Parse(tprop.Value));
                else if (tprop.Type == TiledProperty.PropertyType.Int)
                    xprops.Add(tprop.Name, int.Parse(tprop.Value));
                else if (tprop.Type == TiledProperty.PropertyType.Bool)
                    xprops.Add(tprop.Name, bool.Parse(tprop.Value));
                else
                    Log.warn("Bad tilesheet tile property type: " + tprop.Type + " " + tprop.Name + " (not supported by xTile)");
            }
        }
    }
}
