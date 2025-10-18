using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;

#if IS_SPACECORE
namespace SpaceCore.UI
{
    public
#else
namespace SpaceShared.UI
{
    internal
#endif
         class Label : Element
    {
        /*********
        ** Accessors
        *********/
        public bool Bold { get; set; } = false;
        public float NonBoldScale
        {
            get => this.Scale;
            set => this.Scale = value;
        } // Maintained for compatibility

        public bool NonBoldShadow { get; set; } = true; // Only applies when Bold = false
        public Color IdleTextColor { get; set; } = Game1.textColor;
        public Color HoverTextColor { get; set; } = Game1.unselectedOptionColor;

        public SpriteFont Font { get; set; } = Game1.dialogueFont; // Only applies when Bold = false

        public float Scale { get; set; } = 1.0f;

        public string String { get; set; }

        public Action<Element> Callback { get; set; }

        /// <inheritdoc />
        public override int Width => (int)this.Measure().X;

        /// <inheritdoc />
        public override int Height => (int)this.Measure().Y;

        /// <inheritdoc />
        public override string HoveredSound => (this.Callback != null) ? "shiny4" : null;


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

        /// <summary>Measure the label's rendered dialogue text size.</summary>
        public Vector2 Measure()
        {
            return Label.MeasureString(this.String, this.Bold, scale: this.Scale, font: this.Font);
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch b)
        {
            if (this.IsHidden())
                return;

            bool altColor = this.Hover && this.Callback != null;
            if (this.Bold)
            {
                float originalTextScale = SpriteText.fontPixelZoom;
                SpriteText.fontPixelZoom *= this.Scale;
                SpriteText.drawString(b, this.String, (int)this.Position.X, (int)this.Position.Y, layerDepth: 1, color: altColor ? SpriteText.color_Gray : null);
                SpriteText.fontPixelZoom = originalTextScale;
            }
            else
            {
                Color col = altColor ? this.HoverTextColor : this.IdleTextColor;
                if (col.A <= 0)
                    return;

                if (this.NonBoldShadow)
                    Utility.drawTextWithShadow(b, this.String, this.Font, this.Position, col, this.Scale);
                else
                    b.DrawString(this.Font, this.String, this.Position, col, 0f, Vector2.Zero, this.Scale, SpriteEffects.None, 1);
            }
        }

        /// <summary>Measure the rendered dialogue text size for the given text.</summary>
        /// <param name="text">The text to measure.</param>
        /// <param name="bold">Whether the font is bold.</param>
        /// <param name="scale">The scale to apply to the size.</param>
        /// <param name="font">The font to measure. Defaults to <see cref="Game1.dialogueFont"/> if <c>null</c>.</param>
        public static Vector2 MeasureString(string text, bool bold = false, float scale = 1f, SpriteFont font = null)
        {
            if (bold)
                return new Vector2(SpriteText.getWidthOfString(text) * scale, SpriteText.getHeightOfString(text) * scale);
            else
                return (font ?? Game1.dialogueFont).MeasureString(text) * scale;
        }
    }
}
