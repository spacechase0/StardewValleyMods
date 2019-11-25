using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericModConfigMenu.UI
{
    public class Button : Element
    {
        public Texture2D Texture { get; set; }
        public Rectangle IdleTextureRect { get; set; }
        public Rectangle HoverTextureRect { get; set; }

        public Action<Element> Callback { get; set; }

        public bool Hover { get; private set; } = false;

        public Button(Texture2D tex)
        {
            Texture = tex;
            IdleTextureRect = new Rectangle(0, 0, tex.Width / 2, tex.Height);
            HoverTextureRect = new Rectangle(tex.Width / 2, 0, tex.Width / 2, tex.Height);
        }

        public override void Update()
        {
            var bounds = new Rectangle((int)Position.X, (int)Position.Y, IdleTextureRect.Width, IdleTextureRect.Height);
            Hover = bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY());

            if (Hover && Game1.oldMouseState.LeftButton == ButtonState.Released && Mouse.GetState().LeftButton == ButtonState.Pressed && Callback != null)
                Callback.Invoke(this);
        }

        public override void Draw(SpriteBatch b)
        {
            b.Draw(Texture, Position, Hover ? HoverTextureRect : IdleTextureRect, Color.White);
            Game1.activeClickableMenu?.drawMouse(b);
        }
    }
}
