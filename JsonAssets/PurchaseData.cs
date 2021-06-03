using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonAssets
{
    public class PurchaseData
    {
        public int PurchasePrice { get; set; }
        public string PurchaseFrom { get; set; } = "Pierre";
        public IList<string> PurchaseRequirements { get; set; } = new List<string>();
    }
}
