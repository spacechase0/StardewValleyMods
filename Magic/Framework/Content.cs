using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using xTile;
using xTile.Tiles;

namespace Magic.Framework
{
    internal class Content
    {
        /*********
        ** Public methods
        *********/
        public static Texture2D LoadTexture(string path)
        {
            return Mod.Instance.Helper.Content.Load<Texture2D>($"assets/{path}");
        }

        public static string LoadTextureKey(string path)
        {
            return Mod.Instance.Helper.Content.GetActualAssetKey($"assets/{path}");
        }

        public static TileSheet LoadTilesheet(string ts, Map xmap, out Dictionary<int, SpaceCore.Content.TileAnimation> animMapping)
        {
            string path = $"assets/{ts}.tsx";
            return SpaceCore.Content.LoadTsx(Mod.Instance.Helper, path, ts, xmap, out animMapping);
        }
    }
}
