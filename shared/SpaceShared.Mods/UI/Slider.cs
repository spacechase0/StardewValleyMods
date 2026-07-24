#if !DEPENDENCY_HAS_SPACESHARED
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceShared;
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
         class Slider : Element
    {
        /*********
        ** Fields
        *********/
        protected bool Dragging;


        /*********
        ** Accessors
        *********/
        public int RequestWidth { get; set; }

        public Action<Element> Callback { get; set; }

        /// <inheritdoc />
        public override int Width => this.RequestWidth;

        /// <inheritdoc />
        public override int Height => 24;


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Draw(SpriteBatch b) { }
    }

    internal class Slider<T> : Slider
    {
        /*********
        ** Accessors
        *********/
        public T Minimum { get; set; }
        public T Maximum { get; set; }
        public T Value { get; set; }

        public T Interval { get; set; }


        /*********
        ** Public methods
        *********/
        public override void MouseHover(Point mousePos)
        {
            base.MouseHover(mousePos);

            if (Dragging)
            {
                float perc = mousePos.X / (float) Width;
                Value = Util.Adjust(Value, Interval);
                Value = Value switch
                {
                    int => Util.Clamp<T>(Minimum, (T)(object)(int)(perc * ((int)(object)Maximum - (int)(object)Minimum) + (int)(object)Minimum), Maximum),
                    float => Util.Clamp<T>(Minimum, (T)(object)(perc * ((float)(object)Maximum - (float)(object)Minimum) + (float)(object)Minimum), Maximum),
                    _ => Value
                };

                Callback?.Invoke(this);
                Root.GamepadMovementRegionsDirty = true;
            }
        }

        public override bool LeftClick(Point mousePos, bool pressed)
        {
            base.LeftClick(mousePos, pressed);
            if (Dragging != pressed)
            {
                Dragging = pressed;
                Root.GamepadMovementRegionsDirty = true;
            }
            return true;
        }

        /// <inheritdoc />
        public override void Update(bool isOffScreen = false)
        {
            base.Update(isOffScreen);

            if (valueMarkerRegion != null)
                valueMarkerRegion.bounds = Bounds;

        }

        private ElementClickableComponent? valueMarkerRegion;
        /// <inheritdoc />
        public override void Draw(SpriteBatch b)
        {
            float perc = this.Value switch
            {
                int => ((int)(object)this.Value - (int)(object)this.Minimum!) / (float)((int)(object)this.Maximum! - (int)(object)this.Minimum),
                float => ((float)(object)this.Value - (float)(object)this.Minimum!) / ((float)(object)this.Maximum! - (float)(object)this.Minimum),
                _ => 0
            };

            Rectangle back = new Rectangle((int)this.Position.X, (int)this.Position.Y, this.Width, this.Height);
            Rectangle front = new Rectangle((int)(this.Position.X + perc * (this.Width - 40)), (int)this.Position.Y, 40, this.Height);
            if (valueMarkerRegion != null)
                valueMarkerRegion.bounds = front;

            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), back.X, back.Y, back.Width, back.Height, Color.White, Game1.pixelZoom, false);
            b.Draw(Game1.mouseCursors, new Vector2(front.X, front.Y), new Rectangle(420, 441, 10, 6), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
        }

        public override IEnumerable<ClickableComponent> GetGamepadMovementRegions()
        {
            valueMarkerRegion ??= new ElementClickableComponent(this, Rectangle.Empty)
            {
                leftNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                rightNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                upNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                downNeighborID = ClickableComponent.SNAP_AUTOMATIC,
                ScreenReaderText = Value?.ToString() ?? "",
            };
            yield return valueMarkerRegion;
        }

        public override bool CurrentlyUsingGamepadMovement(out bool allowSnappyMovement)
        {
            if (!Dragging)
                return base.CurrentlyUsingGamepadMovement(out allowSnappyMovement);

            allowSnappyMovement = false;
            return true;
        }
    }
}
#endif
