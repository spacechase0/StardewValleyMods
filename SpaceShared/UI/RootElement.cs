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

        public override int Width => 0;
        public override int Height => 0;


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Update(bool hidden = false)
        {
            base.Update(hidden || this.Obscured);
        }

        /// <inheritdoc />
        internal override RootElement GetRootImpl()
        {
            return this;
        }
    }
}
