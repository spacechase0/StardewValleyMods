#if !DEPENDENCY_HAS_SPACESHARED
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
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
         abstract class Element : IScreenReadable
    {
        /*********
        ** Accessors
        *********/
        public object? UserData { get; set; }

        public Container? Parent { get; internal set; }
        public Vector2 LocalPosition { get; set; }
        public Vector2 Position => (Parent?.Position ?? Vector2.Zero) + LocalPosition;

        public abstract int Width { get; }
        public abstract int Height { get; }
        public Rectangle Bounds => new((int)this.Position.X, (int)this.Position.Y, this.Width, this.Height);

        public bool Hover => Root?.HoveredElement == this;
        public virtual string HoveredSound => null;

        public virtual string ClickedSound => null;

        /// <summary>Whether to disable the element so it's invisible and can't be interacted with.</summary>
        public Func<bool>? ForceHide;

        /*********
        ** Public methods
        *********/
        public virtual void MouseHover(Point mousePos)
        {
            if (Bounds.Contains(mousePos))
                Root?.HoveredElement = this;
        }
        public virtual bool VerticalScroll(int amount) => false;
        public virtual bool LeftClick(Point mousePos, bool pressed)
        {
            if (ClickedSound != null)
                Game1.playSound(ClickedSound);

            return false;
        }
        public virtual bool RightClick(Point mousePos, bool pressed) => false;
        public virtual bool KeyPress(Keys key) => false;

        // TODO: Is isOffScreen still needed?
        /// <summary>Update the element for the current game tick.</summary>
        /// <param name="isOffScreen">Whether the element is currently off-screen.</param>
        public virtual void Update(bool isOffScreen = false)
        {
            defaultClickable?.bounds = Bounds;

            if (Root.HoveredElement == this && Root.PreviousHoveredElement != this)
            {
                if (HoveredSound != null)
                    Game1.playSound(HoveredSound);
            }
        }

        public abstract void Draw(SpriteBatch b);

        public Container Tooltip { get; set; }
        public virtual void DrawTooltip(SpriteBatch b)
        {
            if (Tooltip == null)
                return;

            Tooltip.LocalPosition = Root.LastMousePosition.ToVector2();
            Tooltip.Draw(b);
        }

        public virtual bool RecursivelyContains(Element elem)
        {
            return elem == this;
        }

        public virtual RootElement Root => Parent?.Root;

        /// <summary>Get whether the element is hidden based on <see cref="ForceHide"/> or its position relative to the screen.</summary>
        /// <param name="isOffScreen">Whether the element is currently off-screen.</param>
        public bool IsHidden(bool isOffScreen = false)
        {
            return isOffScreen || this.ForceHide?.Invoke() == true;
        }

        public string? ScreenReaderText { get; set; }
        public string? ScreenReaderDescription { get; set; }
        public bool ScreenReaderIgnore { get; set; } = false;

        private ClickableComponent? defaultClickable;
        public virtual IEnumerable<ClickableComponent> GetGamepadMovementRegions()
        {
            if (ScreenReaderIgnore)
                yield break;

            defaultClickable ??= new ElementClickableComponent(this, Bounds)
            {
                leftNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                rightNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                upNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                downNeighborID = ClickableComponent.SNAP_AUTOMATIC,
            };

            yield return defaultClickable;
        }

        public virtual bool CurrentlyUsingGamepadMovement(out bool allowSnappyMovement)
        {
            allowSnappyMovement = true;
            return false;
        }
    }
}
#endif
