namespace JsonAssets.Data
{
    public abstract class DataNeedsId
    {
        /*********
        ** Accessors
        *********/
        public string Name { get; set; }

        public string EnableWithMod { get; set; }
        public string DisableWithMod { get; set; }

        internal int Id { get; set; } = -1;
    }
}
