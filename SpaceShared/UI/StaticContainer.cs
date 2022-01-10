using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

#if IS_SPACECORE
namespace SpaceCore.UI
{
    public
#else
namespace SpaceShared.UI
{
    internal
#endif
         class StaticContainer : Container
    {
        public Vector2 Size { get; set; }

        public Color? OutlineColor { get; set; } = null;

        public override int Width => (int)this.Size.X;

        public override int Height => (int)this.Size.Y;

        public override void Draw(SpriteBatch b)
        {
            if (this.IsHidden())
                return;

            if (this.OutlineColor.HasValue)
            {
                IClickableMenu.drawTextureBox(b, (int)this.Position.X - 12, (int)this.Position.Y - 12, this.Width + 24, this.Height + 24, this.OutlineColor.Value);
            }
            base.Draw(b);
        }
    }
}
