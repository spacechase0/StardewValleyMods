#if !DEPENDENCY_HAS_SPACESHARED
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
        abstract class Container : Element
    {
        /*********
        ** Fields
        *********/
        private readonly IList<Element> ChildrenImpl = new List<Element>();


        /*********
        ** Accessors
        *********/
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

        public virtual void OnChildrenChanged(bool transitive = false)
        {
            Parent?.OnChildrenChanged(true);
        }

        public override void MouseHover(Point mousePos)
        {
            foreach (var child in ChildrenImpl)
                child.MouseHover(mousePos);
        }

        public override bool VerticalScroll(int amount)
        {
            foreach (var elem in Children)
            {
                if (elem.VerticalScroll(amount))
                    return true;
            }

            return false;
        }

        public override bool KeyPress(Keys key)
        {
            foreach (var child in ChildrenImpl)
            {
                if (child.KeyPress(key))
                    return true;
            }

            return false;
        }

        /// <inheritdoc />
        public override void Update(bool isOffScreen = false)
        {
            foreach (var element in this.ChildrenImpl)
            {
                element.Update(isOffScreen || !element.Bounds.Intersects(Bounds));

                if (element is Container)
                    continue;

                foreach (var region in element.GetGamepadMovementRegions())
                    region.visible = !isOffScreen;
            }
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch b)
        {
            foreach (var child in this.ChildrenImpl)
            {
                if (child.IsHidden() || child == Root?.RenderLast)
                    continue;
                child.Draw(b);
            }
        }

        private ConditionalWeakTable<ClickableComponent, SpaceShared.Holder<bool>> modifiedRegions = new();
        public override IEnumerable<ClickableComponent> GetGamepadMovementRegions()
        {
            int[] idSkip = [ClickableComponent.SNAP_AUTOMATIC, ClickableComponent.CUSTOM_SNAP_BEHAVIOR, ClickableComponent.SNAP_TO_DEFAULT, -1];
            int childCounter = 0;
            foreach (var child in this.ChildrenImpl)
            {
                int idCounter = 0;
                foreach (var region in child.GetGamepadMovementRegions().ToArray())
                {
                    ++idCounter; // TODO: This won't work right if a refresh makes new ones appear

                    var didMod = modifiedRegions.GetOrCreateValue(region);
                    if (!didMod.Value)
                    {
                        didMod.Value = true;

                        if (region.myID == ClickableComponent.ID_ignore)
                            region.myID = idCounter;

                        region.myID += childCounter * 1000;
                        if (!idSkip.Contains(region.leftNeighborID)) region.leftNeighborID += childCounter * 1000;
                        if (!idSkip.Contains(region.rightNeighborID)) region.rightNeighborID += childCounter * 1000;
                        if (!idSkip.Contains(region.upNeighborID)) region.upNeighborID += childCounter * 1000;
                        if (!idSkip.Contains(region.downNeighborID)) region.downNeighborID += childCounter * 1000;
                    }
                    yield return region;
                }
                ++childCounter;
            }
        }
    }
}
#endif
