using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;

#if IS_SPACECORE
namespace SpaceCore.UI
{
    public
#else
namespace SpaceShared.UI
{
    internal
#endif
         abstract class Element
    {
        /*********
        ** Accessors
        *********/
        public object UserData { get; set; }

        public Container Parent { get; internal set; }
        public Vector2 LocalPosition { get; set; }
        public Vector2 Position
        {
            get
            {
                if (this.Parent != null)
                    return this.Parent.Position + this.LocalPosition;
                return this.LocalPosition;
            }
        }

        public abstract int Width { get; }
        public abstract int Height { get; }
        public Rectangle Bounds => new((int)this.Position.X, (int)this.Position.Y, this.Width, this.Height);

        public bool Hover { get; private set; }
        public virtual string HoveredSound => null;

        public bool ClickGestured { get; private set; }
        public bool Clicked => this.Hover && this.ClickGestured;
        public virtual string ClickedSound => null;

        /// <summary>Whether to disable the element so it's invisible and can't be interacted with.</summary>
        public Func<bool> ForceHide;


        /*********
        ** Public methods
        *********/
        /// <summary>Update the element for the current game tick.</summary>
        /// <param name="isOffScreen">Whether the element is currently off-screen.</param>
        public virtual void Update(bool isOffScreen = false)
        {
            bool hidden = this.IsHidden(isOffScreen);

            if (hidden)
            {
                this.Hover = false;
                this.ClickGestured = false;
                return;
            }

            int mouseX;
            int mouseY;
            if (Constants.TargetPlatform == GamePlatform.Android)
            {
                mouseX = Game1.getMouseX();
                mouseY = Game1.getMouseY();
            }
            else
            {
                mouseX = Game1.getOldMouseX();
                mouseY = Game1.getOldMouseY();
            }

            bool newHover = !hidden && !this.GetRoot().Obscured && this.Bounds.Contains(mouseX, mouseY);
            if (newHover && !this.Hover && this.HoveredSound != null)
                Game1.playSound(this.HoveredSound);
            this.Hover = newHover;

            this.ClickGestured = (Game1.input.GetMouseState().LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton == ButtonState.Released);
            this.ClickGestured = this.ClickGestured || (Game1.options.gamepadControls && (Game1.input.GetGamePadState().IsButtonDown(Buttons.A) && !Game1.oldPadState.IsButtonDown(Buttons.A)));
            if (this.ClickGestured && (Dropdown.SinceDropdownWasActive > 0 || Dropdown.ActiveDropdown != null))
            {
                this.ClickGestured = false;
            }
            if (this.Clicked && this.ClickedSound != null)
                Game1.playSound(this.ClickedSound);
        }

        public abstract void Draw(SpriteBatch b);

        public RootElement GetRoot()
        {
            return this.GetRootImpl();
        }

        internal virtual RootElement GetRootImpl()
        {
            if (this.Parent == null)
                throw new Exception("Element must have a parent.");
            return this.Parent.GetRoot();
        }

        /// <summary>Get whether the element is hidden based on <see cref="ForceHide"/> or its position relative to the screen.</summary>
        /// <param name="isOffScreen">Whether the element is currently off-screen.</param>
        public bool IsHidden(bool isOffScreen = false)
        {
            return isOffScreen || this.ForceHide?.Invoke() == true;
        }
    }
}
