using System.Diagnostics;

namespace JsonAssets.Data
{
    [DebuggerDisplay("name = {Name}, id = {Id}")]
    public abstract class DataNeedsId
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The default item name.</summary>
        public string Name { get; set; }

        public string EnableWithMod { get; set; }
        public string DisableWithMod { get; set; }

        internal int Id { get; set; } = -1;
    }
}
