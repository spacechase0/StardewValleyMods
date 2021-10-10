using GenericModConfigMenu.Framework;
using Microsoft.Xna.Framework;

namespace GenericModConfigMenu.ModOption
{
    internal class ImageModOption : ReadOnlyModOption
    {
        /*********
        ** Accessors
        *********/
        public string TexturePath { get; }
        public Rectangle? TextureRect { get; }
        public int Scale { get; }


        /*********
        ** Public methods
        *********/
        public ImageModOption(string texPath, Rectangle? texRect, int scale, ModConfig mod)
            : base(texPath, "", mod)
        {
            this.TexturePath = texPath;
            this.TextureRect = texRect;
            this.Scale = scale;
        }
    }
}
