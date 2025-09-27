using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using SpaceShared;
using StardewModdingAPI.Utilities;

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

        /// <summary>
        /// Whether to create a dummy clickable component.
        /// Setting this true will automatically create the dummy component at the element's position in <see cref="Table"/>.
        /// </summary>
        public bool CreateDummyClickableComponent = false;
        /// <summary>
        /// Used for gamepad/snappy navigation in menus.
        /// Needs to be added to <see cref="IClickableMenu.allClickableComponents">allClickableComponents</see> list of the menu.
        /// </summary>
        public ClickableComponent DummyClickableComponent { get; set; } = null;

        /// <inheritdoc />
        public string ScreenReaderText { get; set; }
        /// <inheritdoc />
        public string ScreenReaderDescription { get; set; }
        /// <inheritdoc />
        public bool ScreenReaderIgnore { get; set; } = false;

        /// <summary>Whether to disable the element so it's invisible and can't be interacted with.</summary>
        public Func<bool> ForceHide;

        /// <summary> Triggered when the mouse cursor enters the element i.e., on first hover. </summary>
        public static event EventHandler<EventArgs> MouseEntered;
        /// <summary> Triggered while the mouse cursor is hovering the element. </summary>
        public static event EventHandler<EventArgs> MouseHovered;

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
            if (newHover)
            {
                if (!this.Hover)
                {
                    if (this.HoveredSound != null) Game1.playSound(this.HoveredSound);
                    Element.MouseEntered?.Invoke(this, EventArgs.Empty);
                }
                Element.MouseHovered?.Invoke((this), EventArgs.Empty);
            }

            this.Hover = newHover;

            this.ClickGestured = (Game1.input.GetMouseState().LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton == ButtonState.Released);
            this.ClickGestured = this.ClickGestured || (Game1.options.gamepadControls && (Game1.input.GetGamePadState().IsButtonDown(Buttons.A) && !Game1.oldPadState.IsButtonDown(Buttons.A)));
            KeybindList leftClickMainKey = KeybindList.Parse("LeftControl + Enter");
            KeybindList leftClickAlternateKey = KeybindList.Parse("OemOpenBrackets");
            this.ClickGestured = this.ClickGestured || leftClickMainKey.JustPressed() || leftClickAlternateKey.JustPressed();
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
