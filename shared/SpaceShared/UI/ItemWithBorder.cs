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

#if IS_SPACECORE
namespace SpaceCore.UI
{
    public
#else
namespace SpaceShared.UI
{
    internal
#endif
    class ItemWithBorder : Element
    {
        public static ItemWithBorder HoveredElement { get; private set; }

        public Item ItemDisplay { get; set; }

        public bool TransparentItemDisplay { get; set; } = false;

        public Color? BoxColor { get; set; } = Color.White;
        public bool BoxIsThin { get; set; } = false;

        public Action<Element> Callback { get; set; }
        public Action<Element> SecondaryCallback { get; set; }

        public override int Width => Game1.tileSize + (BoxIsThin ? 0 : 16) * 2;
        public override int Height => Game1.tileSize + (BoxIsThin ? 0 : 16) * 2;

        public override void Update( bool hidden = false )
        {
            base.Update( hidden );

            if ( Hover )
                HoveredElement = this;
            else if ( HoveredElement == this )
                HoveredElement = null;

            if ( Clicked && Callback != null )
                Callback.Invoke( this );


            bool SecondaryClickGestured = (Game1.input.GetMouseState().RightButton == ButtonState.Pressed && Game1.oldMouseState.RightButton == ButtonState.Released);
            SecondaryClickGestured = SecondaryClickGestured || (Game1.options.gamepadControls && (Game1.input.GetGamePadState().IsButtonDown(Buttons.B) && !Game1.oldPadState.IsButtonDown(Buttons.B)));
            if (Hover && SecondaryClickGestured && SecondaryCallback != null)
                SecondaryCallback.Invoke(this);
        }

        public override void Draw( SpriteBatch b )
        {
            if (BoxColor.HasValue)
            {
                if (BoxIsThin)
                    b.Draw(Game1.menuTexture, Position, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), BoxColor.Value, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.5f);
                else
                    IClickableMenu.drawTextureBox(b, (int)Position.X, (int)Position.Y, Width, Height, BoxColor.Value);
            }
            if ( ItemDisplay != null )
                ItemDisplay.drawInMenu( b, Position + (BoxIsThin ? Vector2.Zero : new Vector2( 16, 16 )), 1, TransparentItemDisplay ? 0.5f : 1, 1 );
        }
    }
}
