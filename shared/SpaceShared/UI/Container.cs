using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

#if IS_SPACECORE
namespace SpaceCore.UI
{
    public
#else
namespace SpaceShared.UI
{
    internal
#endif
        abstract class Container : Element
    {
        /*********
        ** Fields
        *********/
        private readonly IList<Element> ChildrenImpl = new List<Element>();

        /// <summary>Whether to update the <see cref="Children"/> when <see cref="Update"/> is called.</summary>
        protected bool UpdateChildren { get; set; } = true;


        /*********
        ** Accessors
        *********/
        private Element renderLast = null;
        public Element RenderLast
        {
            get => renderLast;
            set {
                renderLast = value;
                if (this.Parent is not null) {
                    if (value is null) {
                        if (this.Parent.RenderLast == this) {
                            this.Parent.RenderLast = null;
                        }
                    } else {
                        this.Parent.RenderLast = this;
                    }
                }
            }
        }

        public Element[] Children => this.ChildrenImpl.ToArray();


        /*********
        ** Public methods
        *********/
        public void AddChild(Element element)
        {
            element.Parent?.RemoveChild(element);
            this.ChildrenImpl.Add(element);
            element.Parent = this;

            OnChildrenChanged();
        }

        public void RemoveChild(Element element)
        {
            if (element.Parent != this)
                throw new ArgumentException("Element must be a child of this container.");
            this.ChildrenImpl.Remove(element);
            element.Parent = null;

            OnChildrenChanged();
        }

        public virtual void OnChildrenChanged()
        {
        }

        /// <inheritdoc />
        public override void Update(bool isOffScreen = false)
        {
            base.Update(isOffScreen);
            if (this.UpdateChildren)
            {
                foreach (var element in this.ChildrenImpl)
                    element.Update(isOffScreen);
            }
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch b)
        {
            if (this.IsHidden())
                return;

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
