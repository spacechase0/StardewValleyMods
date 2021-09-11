using StardewModdingAPI;

namespace JA_to_DGA.Framework
{
    internal class ManifestContentPackFor : IManifestContentPackFor
    {
        public string UniqueID { get; set; }

        public ISemanticVersion MinimumVersion { get; set; }
    }
}
