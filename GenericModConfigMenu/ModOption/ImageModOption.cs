using GenericModConfigMenu.Framework;
using Microsoft.Xna.Framework;

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

        public ImageModOption(string texPath, Rectangle? texRect, int scale, ModConfig mod)
            : base(texPath, "", texPath, mod)
        {
            this.TexturePath = texPath;
            this.TextureRect = texRect;
            this.Scale = scale;
        }
    }
}
