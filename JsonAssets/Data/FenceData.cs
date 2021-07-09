using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceShared;

namespace JsonAssets.Data
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = DiagnosticMessages.IsPublicApi)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.IsPublicApi)]
    public class FenceData : DataNeedsIdWithTexture
    {
        /*********
        ** Accessors
        *********/
        [JsonIgnore]
        public Texture2D ObjectTexture { get; set; }

        [JsonIgnore]
        internal ObjectData CorrespondingObject { get; set; }

        public string Description { get; set; }

        public int MaxHealth { get; set; } = 1;
        public object RepairMaterial { get; set; }
        public FenceBreakToolType BreakTool { get; set; }
        public string PlacementSound { get; set; }
        public string RepairSound { get; set; }

        public int Price { get; set; }
        public FenceRecipe Recipe { get; set; }

        public bool CanPurchase { get; set; } = false;
        public int PurchasePrice { get; set; }
        public string PurchaseFrom { get; set; } = "Robin";
        public IList<string> PurchaseRequirements { get; set; } = new List<string>();
        public IList<PurchaseData> AdditionalPurchaseData { get; set; } = new List<PurchaseData>();

        public Dictionary<string, string> NameLocalization { get; set; } = new();
        public Dictionary<string, string> DescriptionLocalization { get; set; } = new();


        /*********
        ** Public methods
        *********/
        public int GetObjectId()
        {
            return this.Id;
        }
    }
}
