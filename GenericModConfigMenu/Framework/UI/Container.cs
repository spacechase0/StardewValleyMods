using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace GenericModConfigMenu.Framework.UI
{
    internal abstract class Container : Element
    {
        private readonly IList<Element> ChildrenImpl = new List<Element>();

        public Element RenderLast { get; set; }

        public Element[] Children => this.ChildrenImpl.ToArray();

        public void AddChild(Element element)
        {
            element.Parent?.RemoveChild(element);
            this.ChildrenImpl.Add(element);
            element.Parent = this;
        }

        public void RemoveChild(Element element)
        {
            if (element.Parent != this)
                throw new ArgumentException("Element must be a child of this container.");
            this.ChildrenImpl.Remove(element);
            element.Parent = null;
        }

        public override void Draw(SpriteBatch b)
        {
            foreach (var child in this.ChildrenImpl)
            {
                if (child == this.RenderLast)
                    continue;
                child.Draw(b);
            }
            this.RenderLast?.Draw(b);
        }
    }
}
