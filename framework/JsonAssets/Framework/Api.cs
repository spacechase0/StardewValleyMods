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
            return name.FixIdJA("O");
        }

        public string GetCropId(string name)
        {
            return name.FixIdJA("Crop");
        }
        public string GetFruitTreeId(string name)
        {
            return name.FixIdJA("FruitTree");
        }
        public string GetBigCraftableId(string name)
        {
            return name.FixIdJA("BC");
        }
        public string GetHatId(string name)
        {
            return name.FixIdJA("H");
        }
        public string GetWeaponId(string name)
        {
            return name.FixIdJA("W");
        }
        public string GetClothingId(string name)
        {
            if (name.FixIdJA("S") == null)
                return name.FixIdJA("P");
            else
                return name.FixIdJA();
        }
        public string GetShirtId(string name)
        {
            return name.FixIdJA("S");
        }
        public string GetPantsId(string name)
        {
            return name.FixIdJA("P");
        }
        public string GetBootId(string name)
        {
            return name.FixIdJA("B");
        }

        public IDictionary<string, string> GetAllObjectIds()
        {
            Dictionary<string, string> objectIds = new();
            foreach (Data.ObjectData obj in Mod.instance.Objects)
            {
                objectIds.Add(obj.Name, obj.Name.FixIdJA("O"));
            }
            return objectIds;
        }

        public IDictionary<string, string> GetAllCropIds()
        {
            Dictionary<string, string> cropIds = new();
            foreach (Data.CropData crop in Mod.instance.Crops)
            {
                cropIds.Add(crop.Name, crop.Name.FixIdJA("Crop"));
            }
            return cropIds;
        }

        public IDictionary<string, string> GetAllFruitTreeIds()
        {
            Dictionary<string, string> fruitTreeIds = new();
            foreach (Data.FruitTreeData ft in Mod.instance.FruitTrees)
            {
                fruitTreeIds.Add(ft.Name, ft.Name.FixIdJA("FruitTree"));
            }
            return fruitTreeIds;
        }

        public IDictionary<string, string> GetAllBigCraftableIds()
        {
            Dictionary<string, string> bcIds = new();
            foreach (Data.BigCraftableData bc in Mod.instance.BigCraftables)
            {
                bcIds.Add(bc.Name, bc.Name.FixIdJA("BC"));
            }
            return bcIds;
        }

        public IDictionary<string, string> GetAllHatIds()
        {
            Dictionary<string, string> hatIds = new();
            foreach (Data.HatData hat in Mod.instance.Hats)
            {
                hatIds.Add(hat.Name, hat.Name.FixIdJA("H"));
            }
            return hatIds;
        }

        public IDictionary<string, string> GetAllWeaponIds()
        {
            Dictionary<string, string> weaponIds = new();
            foreach (Data.WeaponData weapon in Mod.instance.Weapons)
            {
                weaponIds.Add(weapon.Name, weapon.Name.FixIdJA("W"));
            }
            return weaponIds;
        }

        public IDictionary<string, string> GetAllClothingIds()
        {
            var pants = this.GetAllPantsIds();
            var shirts = this.GetAllShirtIds();
            foreach (KeyValuePair<string,string> kvp in shirts)
                pants.Add(kvp.Key, kvp.Value);
            return pants;
        }

        public IDictionary<string, string> GetAllShirtIds()
        {
            Dictionary<string, string> shirtIds = new();
            foreach (Data.ShirtData shirt in Mod.instance.Shirts)
            {
                shirtIds.Add(shirt.Name, shirt.Name.FixIdJA("S"));
            }
            return shirtIds;
        }

        public IDictionary<string, string> GetAllPantsIds()
        {
            Dictionary<string, string> pantsIds = new();
            foreach (Data.PantsData pant in Mod.instance.Pants)
            {
                pantsIds.Add(pant.Name, pant.Name.FixIdJA("P"));
            }
            return pantsIds;
        }

        public IDictionary<string, string> GetAllBootsIds()
        {
            Dictionary<string, string> bootsIds = new();
            foreach (Data.BootsData boot in Mod.instance.Boots)
            {
                bootsIds.Add(boot.Name, boot.Name.FixIdJA("P"));
            }
            return bootsIds;
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

        internal void InvokeIdsAssigned()
        {
            Log.Trace("Event: IdsAssigned");
            if (this.IdsAssigned == null)
                return;
            Util.InvokeEvent("JsonAssets.Api.IdsAssigned", this.IdsAssigned.GetInvocationList(), null);
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
