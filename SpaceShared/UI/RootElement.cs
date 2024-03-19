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
         class RootElement : Container
    {
        /*********
        ** Accessors
        *********/
        public bool Obscured { get; set; } = false;

        public override int Width => Game1.viewport.Width;
        public override int Height => Game1.viewport.Height;


        /*********
        ** Public methods
        *********/
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
        }

        /// <inheritdoc />
        internal override RootElement GetRootImpl()
        {
            return this;
        }
    }
}
