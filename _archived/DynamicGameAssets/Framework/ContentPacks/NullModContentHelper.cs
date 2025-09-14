using StardewModdingAPI;

namespace DynamicGameAssets.Framework.ContentPacks
{
    internal class NullModContentHelper : IModContentHelper
    {
        public string ModID => "null";

        public IAssetName GetInternalAssetName(string relativePath)
        {
            return null; // Probably should implement this kinda...
        }

        public IAssetData GetPatchHelper<T>(T data, string relativePath = null) where T : notnull
        {
            return null; // Probably should implement this kinda...
        }

        public T Load<T>(string relativePath) where T : notnull
        {
            return default(T);
        }
    }
}
