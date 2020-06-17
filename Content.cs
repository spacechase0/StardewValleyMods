using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System.Collections.Generic;
using xTile;
using xTile.Tiles;
using static SpaceCore.Content;

namespace Magic
{
    public class Content
    {
        public static Texture2D loadTexture( string path )
        {
            return Mod.instance.Helper.Content.Load<Texture2D>($"assets/{path}", ContentSource.ModFolder );
        }
        public static string loadTextureKey(string path)
        {
            return Mod.instance.Helper.Content.GetActualAssetKey($"assets/{path}", ContentSource.ModFolder);
        }

        public static Map loadMap(string mapName, string variant = "map")
        {
            string path = $"assets/{mapName}/{variant}.tmx";
            return SpaceCore.Content.loadTmx(Mod.instance.Helper, mapName, path );
        }

        public static TileSheet loadTilesheet(string ts, Map xmap, out Dictionary< int, TileAnimation > animMapping )
        {
            string path = $"assets/{ts}.tsx";
            return SpaceCore.Content.loadTsx(Mod.instance.Helper, path, ts, xmap, out animMapping );
        }
    }
}
