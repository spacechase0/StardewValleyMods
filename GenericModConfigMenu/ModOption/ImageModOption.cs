using GenericModConfigMenu.Framework;
using Microsoft.Xna.Framework;

namespace GenericModConfigMenu.ModOption
{
    internal class ImageModOption : BaseModOption
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
            : base(texPath, "", texPath, mod)
        {
            this.TexturePath = texPath;
            this.TextureRect = texRect;
            this.Scale = scale;
        }

        /// <inheritdoc />
        public override void SyncToMod() { }

        /// <inheritdoc />
        public override void Save() { }
    }
}
