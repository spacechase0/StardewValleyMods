using System.Collections.Generic;
using Newtonsoft.Json;
using StardewModdingAPI;

namespace JA_to_DGA.Framework
{
    internal class Manifest : IManifest
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Author { get; set; }

        public ISemanticVersion Version { get; set; }

        public ISemanticVersion MinimumApiVersion { get; set; }

        public string UniqueID { get; set; }

        public string EntryDll { get; set; }

        public IManifestContentPackFor ContentPackFor { get; set; }

        public IManifestDependency[] Dependencies { get; set; }

        public string[] UpdateKeys { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> ExtraFields { get; set; }
    }
}
