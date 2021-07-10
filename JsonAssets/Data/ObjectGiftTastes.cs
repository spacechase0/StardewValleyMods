using System.Collections.Generic;
using System.Runtime.Serialization;
using JsonAssets.Framework;

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


        /*********
        ** Private methods
        *********/
        /// <summary>Normalize the model after it's deserialized.</summary>
        /// <param name="context">The deserialization context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            this.Love ??= new List<string>();
            this.Like ??= new List<string>();
            this.Neutral ??= new List<string>();
            this.Dislike ??= new List<string>();
            this.Hate ??= new List<string>();

            foreach (var list in new[] { this.Love, this.Like, this.Neutral, this.Dislike, this.Hate })
                list.FilterNulls();
        }
    }
}
