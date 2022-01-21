using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        public Action<Element> Callback { get; set; }

        public override int Width { get; } = Game1.tileSize + 16 * 2;
        public override int Height { get; } = Game1.tileSize + 16 * 2;

        public override void Update( bool hidden = false )
        {
            base.Update( hidden );

            if ( Hover )
                HoveredElement = this;
            else if ( HoveredElement == this )
                HoveredElement = null;

            if ( Clicked && Callback != null )
                Callback.Invoke( this );
        }

        public override void Draw( SpriteBatch b )
        {
            if ( BoxColor.HasValue )
                IClickableMenu.drawTextureBox( b, ( int ) Position.X, ( int ) Position.Y, Width, Height, BoxColor.Value );
            if ( ItemDisplay != null )
                ItemDisplay.drawInMenu( b, Position + new Vector2( 16, 16 ), 1, TransparentItemDisplay ? 0.5f : 1, 1 );
        }
    }
}
