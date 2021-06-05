using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceCore.UI
{
    public abstract class Container : Element
    {
        private IList<Element> children = new List<Element>();

        public Element RenderLast { get; set; }

        public Element[] Children { get { return children.ToArray(); } }

        public void AddChild(Element element)
        {
            element.Parent?.RemoveChild(element);
            children.Add(element);
            element.Parent = this;
        }

        public void RemoveChild(Element element)
        {
            if (element.Parent != this)
                throw new ArgumentException("Element must be a child of this container.");
            children.Remove(element);
            element.Parent = null;
        }

        public override void Update( bool hidden = false )
        {
            base.Update( hidden );
            foreach ( var element in children )
            {
                element.Update( hidden );
            }
        }

        public override void Draw(SpriteBatch b)
        {
            foreach (var child in children)
            {
                if (child == RenderLast)
                    continue;
                child.Draw(b);
            }
            if (RenderLast != null)
                RenderLast.Draw(b);
        }
    }
}
