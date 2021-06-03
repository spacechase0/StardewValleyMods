namespace JsonAssets.Data
{
    public abstract class DataNeedsId
    {
        public string Name { get; set; }

        public string EnableWithMod { get; set; }
        public string DisableWithMod { get; set; }

        internal int id = -1;
    }
}
