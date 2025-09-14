using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace JsonAssets
{
    public interface IApi
    {
        /// <summary>Load a folder as a Json Assets content pack.</summary>
        /// <param name="path">The absolute path to the content pack folder.</param>
        void LoadAssets(string path);

        /// <summary>Load a folder as a Json Assets content pack.</summary>
        /// <param name="path">The absolute path to the content pack folder.</param>
        /// <param name="translations">The translations to use for <c>TranslationKey</c> fields, or <c>null</c> to load the content pack's <c>i18n</c> folder if present.</param>
        void LoadAssets(string path, ITranslationHelper translations);

        string GetObjectId(string name);
        string GetCropId(string name);
        string GetFruitTreeId(string name);
        string GetBigCraftableId(string name);
        string GetHatId(string name);
        string GetWeaponId(string name);
        string GetClothingId(string name);
        string GetPantsId(string name);
        string GetShirtId(string name);

        IDictionary<string,string> GetAllObjectIds();
        IDictionary<string, string> GetAllCropIds();
        IDictionary<string, string> GetAllFruitTreeIds();
        IDictionary<string, string> GetAllBigCraftableIds();
        IDictionary<string, string> GetAllHatIds();
        IDictionary<string, string> GetAllWeaponIds();
        IDictionary<string, string> GetAllClothingIds();
        IDictionary<string, string> GetAllPantsIds();
        IDictionary<string, string> GetAllShirtIds();
        IDictionary<string, string> GetAllBootsIds();

        List<string> GetAllObjectsFromContentPack(string cp);
        List<string> GetAllCropsFromContentPack(string cp);
        List<string> GetAllFruitTreesFromContentPack(string cp);
        List<string> GetAllBigCraftablesFromContentPack(string cp);
        List<string> GetAllHatsFromContentPack(string cp);
        List<string> GetAllWeaponsFromContentPack(string cp);
        List<string> GetAllClothingFromContentPack(string cp);
        List<string> GetAllBootsFromContentPack(string cp);

        event EventHandler ItemsRegistered;
        event EventHandler AddedItemsToShop;
        event EventHandler IdsAssigned;
    }
}
