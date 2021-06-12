using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GenericModConfigMenu.Framework.UI
{
    internal class Image : Element
    {
        public Texture2D Texture { get; set; }
        public Rectangle? TextureRect { get; set; }
        public float Scale { get; set; } = 1;

        public Action<Element> Callback { get; set; }

        public override int Width => (int)this.GetActualSize().X;
        public override int Height => (int)this.GetActualSize().Y;
        public override string HoveredSound => (this.Callback != null) ? "shiny4" : null;

        public override void Update(bool hidden = false)
        {
            base.Update(hidden);

            if (this.Clicked && this.Callback != null)
                this.Callback.Invoke(this);
        }

        public Vector2 GetActualSize()
        {
            if (this.TextureRect.HasValue)
                return new Vector2(this.TextureRect.Value.Width, this.TextureRect.Value.Height) * this.Scale;
            else
                return new Vector2(this.Texture.Width, this.Texture.Height) * this.Scale;
        }

        public override void Draw(SpriteBatch b)
        {
            b.Draw(this.Texture, this.Position, this.TextureRect, Color.White, 0, Vector2.Zero, this.Scale, SpriteEffects.None, 1);
        }
    }
}
