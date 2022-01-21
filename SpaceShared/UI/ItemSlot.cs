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
    class ItemSlot : ItemWithBorder
    {
        public Item Item { get; set; }

        public override void Draw( SpriteBatch b )
        {
            if ( BoxColor.HasValue )
                IClickableMenu.drawTextureBox( b, ( int ) Position.X, ( int ) Position.Y, Width, Height, BoxColor.Value );
            if ( Item != null )
                Item.drawInMenu( b, Position + new Vector2( 16, 16 ), 1, 1, 1 );
            else if ( ItemDisplay != null )
                ItemDisplay.drawInMenu( b, Position + new Vector2( 16, 16 ), 1, TransparentItemDisplay ? 0.5f : 1, 1 );
        }
    }
}
