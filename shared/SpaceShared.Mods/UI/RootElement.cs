#if !DEPENDENCY_HAS_SPACESHARED
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
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
         class RootElement : Container
    {
        private int _width, _height;

        /*********
        ** Accessors
        *********/
        public bool Obscured { get; set; } = false;
        public override int Width => _width;
        public override int Height => _height;

        public Point LastMousePosition { get; private set; }

        public bool GamepadMovementRegionsDirty { get; set; } = false;
        public Func<ClickableComponent> CurrentSnapped { get; init; }
        public Action<ClickableComponent> SnapTo { get; init; }
        public Element CurrentSnappedElement => (CurrentSnapped?.Invoke() as ElementClickableComponent)?.Parent;

        public Element RenderLast
        {
            get => field;
            set
            {
                field = value;
                MouseHover(LastMousePosition);
            }
        }
        public Element HoveredElement
        {
            get => field;
            set
            {
                if (value != null && value.Root != this)
                    throw new InvalidOperationException("Only elements under this root can be set as the hovered element");

                if (value != null && field != null)
                {
                    int currDepth = 0;
                    for (var elem = HoveredElement; elem != this; elem = elem.Parent)
                        ++currDepth;

                    int valDepth = 0;
                    for (var elem = value; elem != this; elem = elem.Parent)
                        ++valDepth;

                    if (valDepth <= currDepth)
                        return;
                }

                field = value;
            }
        }
        public Element PreviousHoveredElement { get; private set; }

        /*********
        ** Public methods
        *********/

        public RootElement(Func<ClickableComponent> currentSnapped, Action<ClickableComponent> snapTo, int? width = null, int? height = null)
        {
            _width = width ?? Game1.uiViewport.Width;
            _height = height ?? Game1.uiViewport.Height;
            CurrentSnapped = currentSnapped;
            SnapTo = snapTo;
        }

        public override void MouseHover(Point mousePos)
        {
            PreviousHoveredElement = HoveredElement;
            HoveredElement = null;

            if (RenderLast != null)
                RenderLast.MouseHover(mousePos);

            base.MouseHover(mousePos);

            LastMousePosition = mousePos;
        }

        public override bool VerticalScroll(int amount)
        {
            if (RenderLast != null && RenderLast.VerticalScroll(amount))
                return true;

            if (HoveredElement == null)
                return false;

            if (HoveredElement != null)
            {
                for (var elem = HoveredElement; elem != this; elem = elem.Parent)
                {
                    if (elem.VerticalScroll(amount))
                        return true;
                }
            }

            return base.VerticalScroll(amount);
        }

        public override bool LeftClick(Point mousePos, bool pressed)
        {
            if (RenderLast != null && RenderLast.LeftClick(mousePos, pressed))
                return true;

            if (pressed)
            {
                if (HoveredElement == null)
                    return false;

                for (var elem = HoveredElement; elem != this; elem = elem.Parent)
                {
                    if (!elem.Bounds.Contains(mousePos))
                        continue;

                    if (elem.LeftClick(mousePos, pressed))
                        return true;
                }

                return false;
            }
            else
            {
                return base.LeftClick(mousePos, pressed);
            }
        }

        public override bool RightClick(Point mousePos, bool pressed)
        {
            if (RenderLast != null && RenderLast.RightClick(mousePos, pressed))
                return true;

            if (pressed)
            {
                if (HoveredElement == null)
                    return false;

                for (var elem = HoveredElement; elem != this; elem = elem.Parent)
                {
                    if (!elem.Bounds.Contains(mousePos))
                        continue;

                    if (elem.RightClick(mousePos, pressed))
                        return true;
                }
                return false;
            }
            else
            {
                return base.RightClick(mousePos, pressed);
            }
        }

        public override bool KeyPress(Keys key)
        {
            if (RenderLast != null && RenderLast.KeyPress(key))
                return true;

            return base.KeyPress(key);
        }

        /// <inheritdoc />
        public override void Update(bool isOffScreen = false)
        {
            base.Update(isOffScreen || this.Obscured);

            if (Game1.options.gamepadControls && !Game1.lastCursorMotionWasMouse)
            {
                var currSnapped = CurrentSnapped();

                if (ourClickables.Contains(currSnapped) && currSnapped.visible)
                    SnapTo(currSnapped);
            }
        }

        public override void Draw(SpriteBatch b)
        {
            base.Draw(b);
            RenderLast?.Draw(b);
            HoveredElement?.DrawTooltip(b);
        }

        public override RootElement Root => this;

        private HashSet<ClickableComponent> ourClickables = new();
        private ConditionalWeakTable<ClickableComponent, SpaceShared.Holder<bool>> modifiedRegions = new();
        public override IEnumerable<ClickableComponent> GetGamepadMovementRegions()
        {
            ourClickables.Clear();

            if (CurrentSnappedElement != null && CurrentSnappedElement.CurrentlyUsingGamepadMovement(out bool allowSnappy))
            {
                if (allowSnappy)
                {
                    int idCounter = 0;
                    foreach (var clickable in CurrentSnappedElement.GetGamepadMovementRegions().ToArray())
                    {
                        var didMod = modifiedRegions.GetOrCreateValue(clickable);
                        if (!didMod.Value)
                        {
                            didMod.Value = true;
                            if (clickable.myID == ClickableComponent.ID_ignore)
                                clickable.myID = idCounter++; // TODO: This won't work right if a refresh makes new ones appear
                        }
                        yield return clickable;
                    }
                }
                yield break;
            }

            var ret = base.GetGamepadMovementRegions();
            foreach (var entry in ret)
            {
                ourClickables.Add(entry);
                yield return entry;
            }
        }

        public override bool CurrentlyUsingGamepadMovement(out bool allowSnappyMovement)
        {
            var elem = CurrentSnappedElement;
            if (elem != null)
                return elem.CurrentlyUsingGamepadMovement(out allowSnappyMovement);

            return base.CurrentlyUsingGamepadMovement(out allowSnappyMovement);
        }
    }
}
#endif
