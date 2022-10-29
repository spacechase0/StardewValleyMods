using System;
using System.Collections.Generic;
using System.Linq;

using JsonAssets.Data;
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

        public int GetObjectId(string name)
        {
            return this.GetId(Mod.instance.ObjectIds, name);
        }

        public int GetCropId(string name)
        {
            return this.GetId(Mod.instance.CropIds, name);
        }

        public int GetFruitTreeId(string name)
        {
            return this.GetId(Mod.instance.FruitTreeIds, name);
        }

        public int GetBigCraftableId(string name)
        {
            return this.GetId(Mod.instance.BigCraftableIds, name);
        }

        public int GetHatId(string name)
        {
            return this.GetId(Mod.instance.HatIds, name);
        }

        public int GetWeaponId(string name)
        {
            return this.GetId(Mod.instance.WeaponIds, name);
        }

        public int GetClothingId(string name)
        {
            return this.GetId(Mod.instance.ClothingIds, name);
        }

        public IDictionary<string, int> GetAllObjectIds()
        {
            return this.GetAllIds(Mod.instance.ObjectIds);
        }

        public IDictionary<string, int> GetAllCropIds()
        {
            return this.GetAllIds(Mod.instance.CropIds);
        }

        public IDictionary<string, int> GetAllFruitTreeIds()
        {
            return this.GetAllIds(Mod.instance.FruitTreeIds);
        }

        public IDictionary<string, int> GetAllBigCraftableIds()
        {
            return this.GetAllIds(Mod.instance.BigCraftableIds);
        }

        public IDictionary<string, int> GetAllHatIds()
        {
            return this.GetAllIds(Mod.instance.HatIds);
        }

        public IDictionary<string, int> GetAllWeaponIds()
        {
            return this.GetAllIds(Mod.instance.WeaponIds);
        }

        public IDictionary<string, int> GetAllClothingIds()
        {
            return this.GetAllIds(Mod.instance.ClothingIds);
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

        public bool FixIdsInItem(Item item)
        {
            return Mod.instance.FixItem(item);
        }

        public void FixIdsInItemList(List<Item> items)
        {
            Mod.instance.FixItemList(items);
        }

        public void FixIdsInLocation(GameLocation location)
        {
            Mod.instance.FixLocation(location);
        }

        public bool TryGetCustomSprite(object entity, out Texture2D texture, out Rectangle sourceRect)
        {
            Texture2D tex;
            Rectangle rect;
            if (entity is FruitTree fruitTree)
            {
                tex = FruitTree.texture;
                if (fruitTree.stump.Value)
                    rect = new Rectangle(384, fruitTree.treeType.Value * 5 * 16 + 48, 48, 32);
                else if (fruitTree.growthStage.Value <= 3)
                    rect = new Rectangle(fruitTree.growthStage.Value * 48, fruitTree.treeType.Value * 5 * 16, 48, 80);
                else
                    rect = new Rectangle((12 + (fruitTree.GreenHouseTree ? 1 : Utility.getSeasonNumber(Game1.currentSeason)) * 3) * 16, fruitTree.treeType.Value * 5 * 16, 48, 16 + 64);
            }
            else if (entity is Crop crop)
            {
                tex = Game1.cropSpriteSheet;
                rect = Mod.instance.Helper.Reflection.GetMethod(crop, "getSourceRect").Invoke<Rectangle>(crop.rowInSpriteSheet.Value);
            }
            else
            {
                texture = null;
                sourceRect = new Rectangle();
                return false;
            }

            var target = SpaceCore.TileSheetExtensions.GetAdjustedTileSheetTarget(tex, rect);
            texture = target.TileSheet == 0 ? tex : SpaceCore.TileSheetExtensions.GetTileSheet(tex, target.TileSheet);
            sourceRect = rect;
            sourceRect.Y = target.Y;

            return true;
        }

        public bool TryGetCustomSpriteSheet(object entity, out Texture2D texture, out Rectangle sourceRect)
        {
            Texture2D tex;
            Rectangle rect;
            if (entity is FruitTree fruitTree)
            {
                tex = FruitTree.texture;
                rect = ContentInjector1.FruitTreeRect(fruitTree.treeType.Value);
            }
            else if (entity is Crop crop)
            {
                tex = Game1.cropSpriteSheet;
                rect = ContentInjector1.CropRect(crop.rowInSpriteSheet.Value);
            }
            else
            {
                texture = null;
                sourceRect = new Rectangle();
                return false;
            }

            var target = SpaceCore.TileSheetExtensions.GetAdjustedTileSheetTarget(tex, rect);
            texture = target.TileSheet == 0 ? tex : SpaceCore.TileSheetExtensions.GetTileSheet(tex, target.TileSheet);
            sourceRect = rect;
            sourceRect.Y = target.Y;

            return true;
        }

        public bool TryGetGiantCropSprite(int productID, out Lazy<Texture2D> texture)
            => CropData.giantCropMap.TryGetValue(productID, out texture);

        public int[] GetGiantCropIndexes()
            => CropData.giantCropMap.Keys.ToArray();

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

        internal void InvokeIdsAssigned()
        {
            Log.Trace("Event: IdsAssigned");
            if (this.IdsAssigned == null)
                return;
            Util.InvokeEvent("JsonAssets.Api.IdsAssigned", this.IdsAssigned.GetInvocationList(), null);
        }

        internal void InvokeAddedItemsToShop()
        {
            Log.Trace("Event: AddedItemsToShop");
            if (this.AddedItemsToShop == null)
                return;
            Util.InvokeEvent("JsonAssets.Api.AddedItemsToShop", this.AddedItemsToShop.GetInvocationList(), null);
        }

        internal void InvokeIdsFixed()
        {
            Log.Trace("Event: IdsFixed");
            if (this.IdsFixed == null)
                return;
            Util.InvokeEvent("JsonAssets.Api.IdsFixed", this.IdsFixed.GetInvocationList(), null);
        }

        /// <summary>Get the ID for an object by its name.</summary>
        /// <param name="ids">The name-to-ID lookup.</param>
        /// <param name="name">The name to find.</param>
        private int GetId(IDictionary<string, int> ids, string name)
        {
            return ids != null && ids.TryGetValue(name, out int id)
                ? id
                : -1;
        }

        /// <summary>Get all IDs of a given type.</summary>
        /// <param name="ids">The name-to-ID lookup.</param>
        private IDictionary<string, int> GetAllIds(IDictionary<string, int> ids)
        {
            return ids == null
                ? new Dictionary<string, int>()
                : new Dictionary<string, int>(ids);
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
