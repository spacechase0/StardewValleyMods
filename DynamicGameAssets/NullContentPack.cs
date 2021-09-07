using System;
using System.Collections.Generic;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace DynamicGameAssets
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

    internal class NullTranslationHelper : ITranslationHelper
    {
        public string Locale => this.LocaleEnum.ToString();

        public LocalizedContentManager.LanguageCode LocaleEnum => LocalizedContentManager.CurrentLanguageCode;

        public string ModID => "null";

        public Translation Get(string key)
        {
            return (Translation)AccessTools.Constructor(typeof(Translation), new Type[] { typeof(string), typeof(string), typeof(string) }).Invoke(new object[] { "null", "null", "null" });
        }

        public Translation Get(string key, object tokens)
        {
            return this.Get(key);
        }

        public IDictionary<string, Translation> GetInAllLocales(string key, bool withFallback = false)
        {
            return new Dictionary<string, Translation>();
        }

        public IEnumerable<Translation> GetTranslations()
        {
            return new Translation[0];
        }
    }

    internal class NullContentPack : IContentPack
    {
        public string DirectoryPath => null;

        public IManifest Manifest => new NullManifest();

        public ITranslationHelper Translation => new NullTranslationHelper();

        public string GetActualAssetKey(string key)
        {
            return key;
        }

        public bool HasFile(string path)
        {
            return false;
        }

        public T LoadAsset<T>(string key)
        {
            return default(T);
        }

        public TModel ReadJsonFile<TModel>(string path) where TModel : class
        {
            return default(TModel);
        }

        public void WriteJsonFile<TModel>(string path, TModel data) where TModel : class
        {
        }
    }
}
