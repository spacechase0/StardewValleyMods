using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace SpaceCore.UI
{
    public class Button : Element
    {
        public Texture2D Texture { get; set; }
        public Rectangle IdleTextureRect { get; set; }
        public Rectangle HoverTextureRect { get; set; }

        public Action<Element> Callback { get; set; }

        private float scale = 1f;

        public Button(Texture2D tex)
        {
            this.Texture = tex;
            this.IdleTextureRect = new Rectangle(0, 0, tex.Width / 2, tex.Height);
            this.HoverTextureRect = new Rectangle(tex.Width / 2, 0, tex.Width / 2, tex.Height);
        }

        public override int Width => this.IdleTextureRect.Width;
        public override int Height => this.IdleTextureRect.Height;
        public override string HoveredSound => "Cowboy_Footstep";

        public override void Update(bool hidden = false)
        {
            base.Update(hidden);

            this.scale = this.Hover ? Math.Min(this.scale + 0.013f, 1.083f) : Math.Max(this.scale - 0.013f, 1f);

            if (this.Clicked && this.Callback != null)
                this.Callback.Invoke(this);
        }

        public override void Draw(SpriteBatch b)
        {
            Vector2 origin = new Vector2(this.Texture.Width / 4f, this.Texture.Height / 2f);
            b.Draw(this.Texture, this.Position + origin, this.Hover ? this.HoverTextureRect : this.IdleTextureRect, Color.White, 0f, origin, this.scale, SpriteEffects.None, 0f);
            Game1.activeClickableMenu?.drawMouse(b);
        }
    }
}
