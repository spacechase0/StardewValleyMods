using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceCore.Framework.Interface
{
    internal abstract class TabMenu
    {
        public abstract string Name { get; }
        public readonly TabbedMenu Parent;

        public TabMenu(TabbedMenu parent)
        {
            this.Parent = parent;
        }

        public virtual void Update(GameTime gt)
        {
        }

        public abstract void Draw(SpriteBatch b);

        public virtual void MouseMove(int x, int y)
        {
        }

        public virtual void LeftClick(int x, int y)
        {
        }
    }
}
