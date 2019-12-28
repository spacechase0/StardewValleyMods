using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;

namespace JsonAssets
{
    public interface IApi
    {
        void LoadAssets(string path);

        int GetObjectId(string name);
        int GetCropId(string name);
        int GetFruitTreeId(string name);
        int GetBigCraftableId(string name);
        int GetHatId(string name);
        int GetWeaponId(string name);
        int GetClothingId(string name);

        IDictionary<string, int> GetAllObjectIds();
        IDictionary<string, int> GetAllCropIds();
        IDictionary<string, int> GetAllFruitTreeIds();
        IDictionary<string, int> GetAllBigCraftableIds();
        IDictionary<string, int> GetAllHatIds();
        IDictionary<string, int> GetAllWeaponIds();
        IDictionary<string, int> GetAllClothingIds();

        List<string> GetAllObjectsFromContentPack(string cp);
        List<string> GetAllCropsFromContentPack(string cp);
        List<string> GetAllFruitTreesFromContentPack(string cp);
        List<string> GetAllBigCraftablesFromContentPack(string cp);
        List<string> GetAllHatsFromContentPack(string cp);
        List<string> GetAllWeaponsFromContentPack(string cp);
        List<string> GetAllClothingFromContentPack(string cp);

        event EventHandler ItemsRegistered;
        event EventHandler IdsAssigned;
        event EventHandler AddedItemsToShop;
        event EventHandler IdsFixed;

        bool FixIdsInItem(Item item);
        void FixIdsInItemList(List<Item> items);
        void FixIdsInLocation(GameLocation location);

        bool TryGetCustomSprite(object entity, out Texture2D texture, out Rectangle sourceRect);
        bool TryGetCustomSpriteSheet(object entity, out Texture2D texture, out Rectangle sourceRect);
    }

    public class Api : IApi
    {
        private readonly Action<string> loadFolder;

        public Api(Action<string> loadFolder)
        {
            this.loadFolder = loadFolder;
        }

        public void LoadAssets(string path)
        {
            this.loadFolder(path);
        }

        public int GetObjectId(string name)
        {
            if (Mod.instance.objectIds == null)
                return -1;
            return Mod.instance.objectIds.ContainsKey(name) ? Mod.instance.objectIds[name] : -1;
        }

        public int GetCropId(string name)
        {
            if (Mod.instance.cropIds == null)
                return -1;
            return Mod.instance.cropIds.ContainsKey(name) ? Mod.instance.cropIds[name] : -1;
        }

        public int GetFruitTreeId(string name)
        {
            if (Mod.instance.fruitTreeIds == null)
                return -1;
            return Mod.instance.fruitTreeIds.ContainsKey(name) ? Mod.instance.fruitTreeIds[name] : -1;
        }

        public int GetBigCraftableId(string name)
        {
            if (Mod.instance.bigCraftableIds == null)
                return -1;
            return Mod.instance.bigCraftableIds.ContainsKey(name) ? Mod.instance.bigCraftableIds[name] : -1;
        }

        public int GetHatId(string name)
        {
            if (Mod.instance.hatIds == null)
                return -1;
            return Mod.instance.hatIds.ContainsKey(name) ? Mod.instance.hatIds[name] : -1;
        }

        public int GetWeaponId(string name)
        {
            if (Mod.instance.weaponIds == null)
                return -1;
            return Mod.instance.weaponIds.ContainsKey(name) ? Mod.instance.weaponIds[name] : -1;
        }

        public int GetClothingId(string name)
        {
            if (Mod.instance.clothingIds == null)
                return -1;
            return Mod.instance.clothingIds.ContainsKey(name) ? Mod.instance.clothingIds[name] : -1;
        }

        public IDictionary<string, int> GetAllObjectIds()
        {
            if (Mod.instance.objectIds == null)
                return new Dictionary<string, int>();
            return new Dictionary<string, int>(Mod.instance.objectIds);
        }

        public IDictionary<string, int> GetAllCropIds()
        {
            if (Mod.instance.cropIds == null)
                return new Dictionary<string, int>();
            return new Dictionary<string, int>(Mod.instance.cropIds);
        }

        public IDictionary<string, int> GetAllFruitTreeIds()
        {
            if (Mod.instance.fruitTreeIds == null)
                return new Dictionary<string, int>();
            return new Dictionary<string, int>(Mod.instance.fruitTreeIds);
        }

        public IDictionary<string, int> GetAllBigCraftableIds()
        {
            if (Mod.instance.bigCraftableIds == null)
                return new Dictionary<string, int>();
            return new Dictionary<string, int>(Mod.instance.bigCraftableIds);
        }

        public IDictionary<string, int> GetAllHatIds()
        {
            if (Mod.instance.hatIds == null)
                return new Dictionary<string, int>();
            return new Dictionary<string, int>(Mod.instance.hatIds);
        }

        public IDictionary<string, int> GetAllWeaponIds()
        {
            if (Mod.instance.weaponIds == null)
                return new Dictionary<string, int>();
            return new Dictionary<string, int>(Mod.instance.weaponIds);
        }

        public IDictionary<string, int> GetAllClothingIds()
        {
            if (Mod.instance.clothingIds == null)
                return new Dictionary<string, int>();
            return new Dictionary<string, int>(Mod.instance.clothingIds);
        }

        public List<string> GetAllObjectsFromContentPack(string cp)
        {
            foreach (var entry in Mod.instance.objectsByContentPack)
                if (entry.Key.UniqueID == cp)
                    return new List<string>(entry.Value);
            return null;
        }

        public List<string> GetAllCropsFromContentPack(string cp)
        {
            foreach (var entry in Mod.instance.cropsByContentPack)
                if (entry.Key.UniqueID == cp)
                    return new List<string>(entry.Value);
            return null;
        }

        public List<string> GetAllFruitTreesFromContentPack(string cp)
        {
            foreach (var entry in Mod.instance.fruitTreesByContentPack)
                if (entry.Key.UniqueID == cp)
                    return new List<string>(entry.Value);
            return null;
        }

        public List<string> GetAllBigCraftablesFromContentPack(string cp)
        {
            foreach (var entry in Mod.instance.bigCraftablesByContentPack)
                if (entry.Key.UniqueID == cp)
                    return new List<string>(entry.Value);
            return null;
        }

        public List<string> GetAllHatsFromContentPack(string cp)
        {
            foreach (var entry in Mod.instance.hatsByContentPack)
                if (entry.Key.UniqueID == cp)
                    return new List<string>(entry.Value);
            return null;
        }

        public List<string> GetAllWeaponsFromContentPack(string cp)
        {
            foreach (var entry in Mod.instance.weaponsByContentPack)
                if (entry.Key.UniqueID == cp)
                    return new List<string>(entry.Value);
            return null;
        }

        public List<string> GetAllClothingFromContentPack(string cp)
        {
            foreach (var entry in Mod.instance.clothingByContentPack)
                if (entry.Key.UniqueID == cp)
                    return new List<string>(entry.Value);
            return null;
        }

        public event EventHandler ItemsRegistered;
        internal void InvokeItemsRegistered()
        {
            Log.trace("Event: ItemsRegistered");
            if (ItemsRegistered == null)
                return;
            Util.invokeEvent("JsonAssets.Api.ItemsRegistered", ItemsRegistered.GetInvocationList(), null);
        }

        public event EventHandler IdsAssigned;
        internal void InvokeIdsAssigned()
        {
            Log.trace("Event: IdsAssigned");
            if (IdsAssigned == null)
                return;
            Util.invokeEvent("JsonAssets.Api.IdsAssigned", IdsAssigned.GetInvocationList(), null);
        }

        public event EventHandler AddedItemsToShop;
        internal void InvokeAddedItemsToShop()
        {
            Log.trace("Event: AddedItemsToShop");
            if (AddedItemsToShop == null)
                return;
            Util.invokeEvent("JsonAssets.Api.AddedItemsToShop", AddedItemsToShop.GetInvocationList(), null);
        }

        public event EventHandler IdsFixed;
        internal void InvokeIdsFixed()
        {
            Log.trace("Event: IdsFixed");
            if (IdsFixed == null)
                return;
            Util.invokeEvent("JsonAssets.Api.IdsFixed", IdsFixed.GetInvocationList(), null);
        }

        public bool FixIdsInItem(Item item)
        {
            return Mod.instance.fixItem(item);
        }

        public void FixIdsInItemList(List<Item> items)
        {
            Mod.instance.fixItemList(items);
        }

        public void FixIdsInLocation(GameLocation location)
        {
            Mod.instance.fixLocation(location);
        }

        public bool TryGetCustomSprite(object entity, out Texture2D texture, out Rectangle sourceRect)
        {
            Texture2D tex = null;
            Rectangle rect = new Rectangle();
            if (entity is FruitTree fruitTree)
            {
                tex = FruitTree.texture;
                if ( fruitTree.stump.Value )
                {
                    rect = new Rectangle(384, fruitTree.treeType.Value * 5 * 16 + 48, 48, 32);
                }
                else if ( fruitTree.growthStage.Value <= 3 )
                {
                    rect = new Rectangle(fruitTree.growthStage.Value * 48, fruitTree.treeType.Value * 5 * 16, 48, 80);
                }
                else
                {
                    rect = new Rectangle((12 + (fruitTree.GreenHouseTree ? 1 : Utility.getSeasonNumber(Game1.currentSeason)) * 3) * 16, fruitTree.treeType.Value * 5 * 16, 48, 16 + 64);
                }
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
            Texture2D tex = null;
            Rectangle rect = new Rectangle();
            if (entity is FruitTree fruitTree)
            {
                tex = FruitTree.texture;
                rect = ContentInjector.fruitTreeRect(fruitTree.treeType.Value);
            }
            else if (entity is Crop crop)
            {
                tex = Game1.cropSpriteSheet;
                rect = ContentInjector.cropRect(crop.rowInSpriteSheet.Value);
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
    }
}
