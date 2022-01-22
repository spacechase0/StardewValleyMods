namespace JsonAssets.Data
{
    public abstract class DataNeedsId
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The default item name.</summary>
        public string Name { get; set; }

        public string EnableWithMod { get; set; }
        public string DisableWithMod { get; set; }
    }
}
