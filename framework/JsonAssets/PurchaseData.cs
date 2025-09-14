using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JsonAssets
{
    public class PurchaseData
    {
        /*********
        ** Accessors
        *********/
        public int PurchasePrice { get; set; }
        public string PurchaseFrom { get; set; } = "Pierre";
        public IList<string> PurchaseRequirements { get; set; } = new List<string>();


        /*********
        ** Private methods
        *********/
        /// <summary>Normalize the model after it's deserialized.</summary>
        /// <param name="context">The deserialization context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            this.PurchaseRequirements ??= new List<string>();
        }
    }
}
