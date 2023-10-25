using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#if IS_SPACECORE
namespace SpaceCore.UI
{
    public
#else
namespace SpaceShared.UI
{
    internal
#endif
         class Image : Element, ISingleTexture
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The image texture to display.</summary>
        public Texture2D Texture { get; set; }

        /// <summary>The pixel area within the texture to display, or <c>null</c> to show the entire image.</summary>
        public Rectangle? TexturePixelArea { get; set; }

        /// <summary>The zoom factor to apply to the image.</summary>
        public int Scale { get; set; }

        public Action<Element> Callback { get; set; }

        /// <inheritdoc />
        public override int Width => (int)this.GetActualSize().X;

        /// <inheritdoc />
        public override int Height => (int)this.GetActualSize().Y;

        /// <inheritdoc />
        public override string HoveredSound => (this.Callback != null) ? "shiny4" : null;

        public Color DrawColor { get; set; } = Color.White;

        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Update(bool isOffScreen = false)
        {
            base.Update(isOffScreen);

            if (this.Clicked)
                this.Callback?.Invoke(this);
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch b)
        {
            if (this.IsHidden())
                return;

            b.Draw(this.Texture, this.Position, this.TexturePixelArea, DrawColor, 0, Vector2.Zero, this.Scale, SpriteEffects.None, 1);
        }


        /*********
        ** Private methods
        *********/
        private Vector2 GetActualSize()
        {
            if (this.TexturePixelArea.HasValue)
                return new Vector2(this.TexturePixelArea.Value.Width, this.TexturePixelArea.Value.Height) * this.Scale;
            else
                return new Vector2(this.Texture.Width, this.Texture.Height) * this.Scale;
        }
    }
}
