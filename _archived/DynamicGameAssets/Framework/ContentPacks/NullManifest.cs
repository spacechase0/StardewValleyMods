using System.Collections.Generic;
using StardewModdingAPI;

namespace DynamicGameAssets.Framework.ContentPacks
{
    internal class NullManifest : IManifest
    {
        public string Name => "null";

        public string Description => "null";

        public string Author => "null";

        public ISemanticVersion Version => new SemanticVersion("1.0.0");

        public ISemanticVersion MinimumApiVersion => null;

        public string UniqueID => "null";

        public string EntryDll => null;

        public IManifestContentPackFor ContentPackFor => null;

        public IManifestDependency[] Dependencies => null;

        public string[] UpdateKeys => null;

        public IDictionary<string, object> ExtraFields => null;
    }
}
