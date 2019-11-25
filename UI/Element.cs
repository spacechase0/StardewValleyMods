using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericModConfigMenu.UI
{
    public abstract class Element
    {
        public object UserData { get; set; }

        public Container Parent { get; internal set; }
        public Vector2 LocalPosition { get; set; }
        public Vector2 Position
        {
            get
            {
                if (Parent != null)
                    return Parent.Position + LocalPosition;
                return LocalPosition;
            }
        }

        public abstract void Update();
        public abstract void Draw(SpriteBatch b);

        
        public RootElement GetRoot()
        {
            return GetRootImpl();
        }
        
        internal virtual RootElement GetRootImpl()
        {
            if (Parent == null)
                throw new Exception("Element must have a parent.");
            return Parent.GetRoot();
        }
    }
}
