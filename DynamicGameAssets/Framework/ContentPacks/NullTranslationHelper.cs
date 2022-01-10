using System;
using System.Collections.Generic;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace DynamicGameAssets.Framework.ContentPacks
{
    internal class NullTranslationHelper : ITranslationHelper
    {
        public string Locale => this.LocaleEnum.ToString();

        public LocalizedContentManager.LanguageCode LocaleEnum => LocalizedContentManager.CurrentLanguageCode;

        public string ModID => "null";

        public Translation Get(string key)
        {
            return (Translation)AccessTools.Constructor(typeof(Translation), new[] { typeof(string), typeof(string), typeof(string) }).Invoke(new object[] { "null", "null", "null" });
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
            return Array.Empty<Translation>();
        }
    }
}
