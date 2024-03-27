using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace SpaceShared.APIs
{
    public interface IJsonAssetsApi
    {
        /// <summary>Load a folder as a Json Assets content pack.</summary>
        /// <param name="path">The absolute path to the content pack folder.</param>
        void LoadAssets(string path);

        /// <summary>Load a folder as a Json Assets content pack.</summary>
        /// <param name="path">The absolute path to the content pack folder.</param>
        /// <param name="translations">The translations to use for <c>TranslationKey</c> fields, or <c>null</c> to load the content pack's <c>i18n</c> folder if present.</param>
        void LoadAssets(string path, ITranslationHelper translations);

        List<string> GetAllObjectsFromContentPack(string cp);
        List<string> GetAllCropsFromContentPack(string cp);
        List<string> GetAllFruitTreesFromContentPack(string cp);
        List<string> GetAllBigCraftablesFromContentPack(string cp);
        List<string> GetAllHatsFromContentPack(string cp);
        List<string> GetAllWeaponsFromContentPack(string cp);
        List<string> GetAllClothingFromContentPack(string cp);
        List<string> GetAllBootsFromContentPack(string cp);

        public string GetObjectId(string name);

        event EventHandler ItemsRegistered;
        event EventHandler AddedItemsToShop;
    }
}
