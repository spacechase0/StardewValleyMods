#if !DEPENDENCY_HAS_SPACESHARED
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
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
        /*********
        ** Accessors
        *********/
        public bool Obscured { get; set; } = false;

        public override int Width => Game1.viewport.Width;
        public override int Height => Game1.viewport.Height;

        public bool GamepadMovementRegionsDirty { get; set; } = false;


        private Func<ClickableComponent> CurrentSnapped;
        private Action<int> ForceSnapInDirection;
        public Element CurrentSnappedElement => (CurrentSnapped?.Invoke() as ElementClickableComponent)?.Parent;

        /*********
        ** Public methods
        *********/

        public RootElement(Func<ClickableComponent> currentSnapped, Action<int> forceSnapInDirection)
        {
            CurrentSnapped = currentSnapped;
            ForceSnapInDirection = forceSnapInDirection;
        }

        /// <inheritdoc />
        public override void Update(bool isOffScreen = false)
        {
            base.Update(isOffScreen || this.Obscured);
            if (Dropdown.ActiveDropdown?.GetRoot() != this)
            {
                Dropdown.ActiveDropdown = null;
            }
            if ( Dropdown.SinceDropdownWasActive > 0 )
            {
                Dropdown.SinceDropdownWasActive--;
            }

            if (Game1.options.gamepadControls && !Game1.lastCursorMotionWasMouse)
            {
                Point mousePos = Game1.getMousePosition();
                var currSnapped = CurrentSnapped();
                if (ourClickables.Contains(currSnapped) && !currSnapped.bounds.Contains(mousePos))
                {
                    Point offset = mousePos;
                    offset.X -= mousePos.X > currSnapped.bounds.Right ? currSnapped.bounds.Right : currSnapped.bounds.Left;
                    offset.Y -= mousePos.Y > currSnapped.bounds.Bottom ? currSnapped.bounds.Bottom : currSnapped.bounds.Top;

                    int dir;
                    if (Math.Abs(offset.X) > Math.Abs(offset.Y))
                        dir = offset.X < 0 ? Game1.left : Game1.right;
                    else
                        dir = offset.Y < 0 ? Game1.up : Game1.down;

                    ForceSnapInDirection(dir);
                }
            }
        }

        /// <inheritdoc />
        internal override RootElement GetRootImpl()
        {
            return this;
        }

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
