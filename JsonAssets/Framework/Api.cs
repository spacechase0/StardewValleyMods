using System;
using System.Collections.Generic;
using JsonAssets.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

// ReSharper disable once CheckNamespace -- can't change namespace since it's part of the public API
namespace JsonAssets
{
    public class Api : IApi
    {
        /*********
        ** Fields
        *********/
        /// <summary>Load a folder as a Json Assets content pack.</summary>
        private readonly Action<string, ITranslationHelper> LoadFolder;


        /*********
        ** Accessors
        *********/
        public event EventHandler ItemsRegistered;
        public event EventHandler IdsAssigned;
        public event EventHandler AddedItemsToShop;
        public event EventHandler IdsFixed;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="loadFolder">Load a folder as a Json Assets content pack.</param>
        public Api(Action<string, ITranslationHelper> loadFolder)
        {
            this.LoadFolder = loadFolder;
        }

        /// <inheritdoc />
        public void LoadAssets(string path)
        {
            this.LoadAssets(path, null);
        }

        /// <inheritdoc />
        public void LoadAssets(string path, ITranslationHelper translations)
        {
            this.LoadFolder(path, translations);
        }

        public string GetObjectId(string name)
        {
            return name.FixIdJA();
        }

        public string GetCropId(string name)
        {
            return name.FixIdJA();
        }
        public string GetFruitTreeId(string name)
        {
            return name.FixIdJA();
        }
        public string GetBigCraftableId(string name)
        {
            return name.FixIdJA();
        }
        public string GetHatId(string name)
        {
            return name.FixIdJA();
        }
        public string GetWeaponId(string name)
        {
            return name.FixIdJA();
        }
        public string GetClothingId(string name)
        {
            return name.FixIdJA();
        }

        public List<string> GetAllObjectsFromContentPack(string cp)
        {
            return this.GetAllFromContentPack(Mod.instance.ObjectsByContentPack, cp);
        }

        public List<string> GetAllCropsFromContentPack(string cp)
        {
            return this.GetAllFromContentPack(Mod.instance.CropsByContentPack, cp);
        }

        public List<string> GetAllFruitTreesFromContentPack(string cp)
        {
            return this.GetAllFromContentPack(Mod.instance.FruitTreesByContentPack, cp);
        }

        public List<string> GetAllBigCraftablesFromContentPack(string cp)
        {
            return this.GetAllFromContentPack(Mod.instance.BigCraftablesByContentPack, cp);
        }

        public List<string> GetAllHatsFromContentPack(string cp)
        {
            return this.GetAllFromContentPack(Mod.instance.HatsByContentPack, cp);
        }

        public List<string> GetAllWeaponsFromContentPack(string cp)
        {
            return this.GetAllFromContentPack(Mod.instance.WeaponsByContentPack, cp);
        }

        public List<string> GetAllClothingFromContentPack(string cp)
        {
            return this.GetAllFromContentPack(Mod.instance.ClothingByContentPack, cp);
        }

        public List<string> GetAllBootsFromContentPack(string cp)
        {
            return this.GetAllFromContentPack(Mod.instance.BootsByContentPack, cp);
        }


        /*********
        ** Internal methods
        *********/
        internal void InvokeItemsRegistered()
        {
            Log.Trace("Event: ItemsRegistered");
            if (this.ItemsRegistered == null)
                return;
            Util.InvokeEvent("JsonAssets.Api.ItemsRegistered", this.ItemsRegistered.GetInvocationList(), null);
        }

        internal void InvokeAddedItemsToShop()
        {
            Log.Trace("Event: AddedItemsToShop");
            if (this.AddedItemsToShop == null)
                return;
            Util.InvokeEvent("JsonAssets.Api.AddedItemsToShop", this.AddedItemsToShop.GetInvocationList(), null);
        }

        /// <summary>Get all content of a given type added by a content pack.</summary>
        /// <param name="content">The registered content by content pack ID.</param>
        /// <param name="contentPackId">The content pack ID.</param>
        private List<string> GetAllFromContentPack(IDictionary<IManifest, List<string>> content, string contentPackId)
        {
            foreach (var entry in content)
            {
                if (entry.Key.UniqueID == contentPackId)
                    return new List<string>(entry.Value);
            }

            return null;
        }
    }
}
