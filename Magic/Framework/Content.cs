using Microsoft.Xna.Framework.Graphics;

namespace Magic.Framework
{
    internal class Content
    {
        /*********
        ** Public methods
        *********/
        public static Texture2D LoadTexture(string path)
        {
            return Mod.Instance.Helper.ModContent.Load<Texture2D>($"assets/{path}");
        }

        public static string LoadTextureKey(string path)
        {
            return Mod.Instance.Helper.ModContent.GetInternalAssetName($"assets/{path}").BaseName;
        }
    }
}
