using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GenericModConfigMenu.Framework.ModOption
{
    /// <summary>A mod option which renders an image.</summary>
    internal class ImageModOption : ReadOnlyModOption
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The image texture to display.</summary>
        public Func<Texture2D> Texture { get; }

        /// <summary>The pixel area within the texture to display, or <c>null</c> to show the entire image.</summary>
        public Rectangle? TexturePixelArea { get; }

        /// <summary>The zoom factor to apply to the image.</summary>
        public int Scale { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="texture">The image texture to display.</param>
        /// <param name="texturePixelArea">The pixel area within the texture to display, or <c>null</c> to show the entire image.</param>
        /// <param name="scale">The zoom factor to apply to the image.</param>
        /// <param name="mod">The mod config UI that contains this option.</param>
        public ImageModOption(Func<Texture2D> texture, Rectangle? texturePixelArea, int scale, ModConfig mod)
            : base(() => "", null, mod)
        {
            this.Texture = texture;
            this.TexturePixelArea = texturePixelArea;
            this.Scale = scale;
        }
    }
}
