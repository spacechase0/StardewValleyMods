namespace SpaceShared.UI
{
    internal class RootElement : Container
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
        public override void Update(bool isOffScreen = false)
        {
            base.Update(isOffScreen || this.Obscured);
        }

        /// <inheritdoc />
        internal override RootElement GetRootImpl()
        {
            return this;
        }
    }
}
