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

        private static readonly XmlSerializer TilesheetSerializer = new(typeof(TiledTileset), new XmlRootAttribute("tileset"));
        public static TileSheet LoadTsx(IModHelper modHelper, string path, string ts, Map xmap, out Dictionary<int, TileAnimation> animMapping)
        {
            TiledTileset ttileSheet;
            Stream stream = null;
            try
            {
                stream = new FileStream(Path.Combine(modHelper.DirectoryPath, path), FileMode.Open);
                ttileSheet = (TiledTileset)Content.TilesheetSerializer.Deserialize(stream);
            }
            finally
            {
                stream?.Close();
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
