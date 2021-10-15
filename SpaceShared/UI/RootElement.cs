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
        public override void Update(bool hidden = false)
        {
            base.Update(hidden || this.Obscured);
            if (!hidden)
            {
                foreach (var child in this.Children)
                    child.Update(hidden);
            }
        }

        /// <inheritdoc />
        internal override RootElement GetRootImpl()
        {
            return this;
        }
    }
}
