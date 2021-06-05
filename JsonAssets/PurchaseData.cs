using System.Collections.Generic;

namespace JsonAssets
{
    public class PurchaseData
    {
        public int PurchasePrice { get; set; }
        public string PurchaseFrom { get; set; } = "Pierre";
        public IList<string> PurchaseRequirements { get; set; } = new List<string>();
    }
}
