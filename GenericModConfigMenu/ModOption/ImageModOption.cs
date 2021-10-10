using GenericModConfigMenu.Framework;
using Microsoft.Xna.Framework;

namespace GenericModConfigMenu.ModOption
{
    /// <summary>A mod option which renders an image.</summary>
    internal class ImageModOption : ReadOnlyModOption
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The image texture path to display.</summary>
        public string TexturePath { get; }

        /// <summary>The pixel area within the texture to display, or <c>null</c> to show the entire image.</summary>
        public Rectangle? TexturePixelArea { get; }

        /// <summary>The zoom factor to apply to the image.</summary>
        public int Scale { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="texturePath">The image texture path to display.</param>
        /// <param name="texturePixelArea">The pixel area within the texture to display, or <c>null</c> to show the entire image.</param>
        /// <param name="scale">The zoom factor to apply to the image.</param>
        /// <param name="mod">The mod config UI that contains this option.</param>
        public ImageModOption(string texturePath, Rectangle? texturePixelArea, int scale, ModConfig mod)
            : base(texturePath, "", mod)
        {
            this.TexturePath = texturePath;
            this.TexturePixelArea = texturePixelArea;
            this.Scale = scale;
        }
    }
}
