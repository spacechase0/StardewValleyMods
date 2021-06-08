using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace GenericModConfigMenu.ModOption
{
    internal class ImageModOption : BaseModOption
    {
        public string TexturePath { get; }
        public Rectangle? TextureRect { get; }
        public int Scale { get; }

        public override void SyncToMod()
        {
        }

        public override void Save()
        {
        }

        public ImageModOption(string texPath, Rectangle? texRect, int scale, IManifest mod)
            : base(texPath, "", texPath, mod)
        {
            TexturePath = texPath;
            TextureRect = texRect;
            Scale = scale;
        }
    }
}
