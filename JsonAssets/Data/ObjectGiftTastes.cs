using System.Collections.Generic;

namespace JsonAssets.Data
{
    public class ObjectGiftTastes
    {
        /*********
        ** Accessors
        *********/
        public IList<string> Love { get; set; } = new List<string>();
        public IList<string> Like { get; set; } = new List<string>();
        public IList<string> Neutral { get; set; } = new List<string>();
        public IList<string> Dislike { get; set; } = new List<string>();
        public IList<string> Hate { get; set; } = new List<string>();
    }
}
