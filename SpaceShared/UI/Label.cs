using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace SpaceShared.UI
{
    internal class Label : Element
    {
        public bool Bold { get; set; } = false;
        public float NonBoldScale { get; set; } = 1f; // Only applies when Bold = false
        public bool NonBoldShadow { get; set; } = true; // Only applies when Bold = false
        public Color IdleTextColor { get; set; } = Game1.textColor;
        public Color HoverTextColor { get; set; } = Game1.unselectedOptionColor;
        public string String { get; set; }

        public Action<Element> Callback { get; set; }

        public override int Width => (int)this.Measure().X;
        public override int Height => (int)this.Measure().Y;
        public override string HoveredSound => (this.Callback != null) ? "shiny4" : null;

        public override void Update(bool hidden = false)
        {
            base.Update(hidden);

            if (this.Clicked && this.Callback != null)
                this.Callback.Invoke(this);
        }

        public Vector2 Measure()
        {
            if (this.Bold)
                return new Vector2(SpriteText.getWidthOfString(this.String), SpriteText.getHeightOfString(this.String));
            else
                return Game1.dialogueFont.MeasureString(this.String) * this.NonBoldScale;
        }

        public override void Draw(SpriteBatch b)
        {
            bool altColor = this.Hover && this.Callback != null;
            if (this.Bold)
                SpriteText.drawString(b, this.String, (int)this.Position.X, (int)this.Position.Y, layerDepth: 1, color: altColor ? SpriteText.color_Gray : -1);
            else if (this.NonBoldShadow)
                Utility.drawTextWithShadow(b, this.String, Game1.dialogueFont, this.Position, altColor ? this.HoverTextColor : this.IdleTextColor, this.NonBoldScale);
            else
                b.DrawString(Game1.dialogueFont, this.String, this.Position, altColor ? this.HoverTextColor : this.IdleTextColor, 0f, Vector2.Zero, this.NonBoldScale, SpriteEffects.None, 1);
        }
    }
}
