using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewValley;
using StardewValley.TerrainFeatures;

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
        List<string> GetAllBootsFromContentPack(string cp);

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
        private readonly Action<string> LoadFolder;

        public Api(Action<string> loadFolder)
        {
            this.LoadFolder = loadFolder;
        }

        public void LoadAssets(string path)
        {
            this.LoadFolder(path);
        }

        public int GetObjectId(string name)
        {
            if (Mod.instance.ObjectIds == null)
                return -1;
            return Mod.instance.ObjectIds.ContainsKey(name) ? Mod.instance.ObjectIds[name] : -1;
        }

        public int GetCropId(string name)
        {
            if (Mod.instance.CropIds == null)
                return -1;
            return Mod.instance.CropIds.ContainsKey(name) ? Mod.instance.CropIds[name] : -1;
        }

        public int GetFruitTreeId(string name)
        {
            if (Mod.instance.FruitTreeIds == null)
                return -1;
            return Mod.instance.FruitTreeIds.ContainsKey(name) ? Mod.instance.FruitTreeIds[name] : -1;
        }

        public int GetBigCraftableId(string name)
        {
            if (Mod.instance.BigCraftableIds == null)
                return -1;
            return Mod.instance.BigCraftableIds.ContainsKey(name) ? Mod.instance.BigCraftableIds[name] : -1;
        }

        public int GetHatId(string name)
        {
            if (Mod.instance.HatIds == null)
                return -1;
            return Mod.instance.HatIds.ContainsKey(name) ? Mod.instance.HatIds[name] : -1;
        }

        public int GetWeaponId(string name)
        {
            if (Mod.instance.WeaponIds == null)
                return -1;
            return Mod.instance.WeaponIds.ContainsKey(name) ? Mod.instance.WeaponIds[name] : -1;
        }

        public int GetClothingId(string name)
        {
            if (Mod.instance.ClothingIds == null)
                return -1;
            return Mod.instance.ClothingIds.ContainsKey(name) ? Mod.instance.ClothingIds[name] : -1;
        }

        public IDictionary<string, int> GetAllObjectIds()
        {
            if (Mod.instance.ObjectIds == null)
                return new Dictionary<string, int>();
            return new Dictionary<string, int>(Mod.instance.ObjectIds);
        }

        public IDictionary<string, int> GetAllCropIds()
        {
            if (Mod.instance.CropIds == null)
                return new Dictionary<string, int>();
            return new Dictionary<string, int>(Mod.instance.CropIds);
        }

        public IDictionary<string, int> GetAllFruitTreeIds()
        {
            if (Mod.instance.FruitTreeIds == null)
                return new Dictionary<string, int>();
            return new Dictionary<string, int>(Mod.instance.FruitTreeIds);
        }

        public IDictionary<string, int> GetAllBigCraftableIds()
        {
            if (Mod.instance.BigCraftableIds == null)
                return new Dictionary<string, int>();
            return new Dictionary<string, int>(Mod.instance.BigCraftableIds);
        }

        public IDictionary<string, int> GetAllHatIds()
        {
            if (Mod.instance.HatIds == null)
                return new Dictionary<string, int>();
            return new Dictionary<string, int>(Mod.instance.HatIds);
        }

        public IDictionary<string, int> GetAllWeaponIds()
        {
            if (Mod.instance.WeaponIds == null)
                return new Dictionary<string, int>();
            return new Dictionary<string, int>(Mod.instance.WeaponIds);
        }

        public IDictionary<string, int> GetAllClothingIds()
        {
            if (Mod.instance.ClothingIds == null)
                return new Dictionary<string, int>();
            return new Dictionary<string, int>(Mod.instance.ClothingIds);
        }

        public List<string> GetAllObjectsFromContentPack(string cp)
        {
            foreach (var entry in Mod.instance.ObjectsByContentPack)
                if (entry.Key.UniqueID == cp)
                    return new List<string>(entry.Value);
            return null;
        }

        public List<string> GetAllCropsFromContentPack(string cp)
        {
            foreach (var entry in Mod.instance.CropsByContentPack)
                if (entry.Key.UniqueID == cp)
                    return new List<string>(entry.Value);
            return null;
        }

        public List<string> GetAllFruitTreesFromContentPack(string cp)
        {
            foreach (var entry in Mod.instance.FruitTreesByContentPack)
                if (entry.Key.UniqueID == cp)
                    return new List<string>(entry.Value);
            return null;
        }

        public List<string> GetAllBigCraftablesFromContentPack(string cp)
        {
            foreach (var entry in Mod.instance.BigCraftablesByContentPack)
                if (entry.Key.UniqueID == cp)
                    return new List<string>(entry.Value);
            return null;
        }

        public List<string> GetAllHatsFromContentPack(string cp)
        {
            foreach (var entry in Mod.instance.HatsByContentPack)
                if (entry.Key.UniqueID == cp)
                    return new List<string>(entry.Value);
            return null;
        }

        public List<string> GetAllWeaponsFromContentPack(string cp)
        {
            foreach (var entry in Mod.instance.WeaponsByContentPack)
                if (entry.Key.UniqueID == cp)
                    return new List<string>(entry.Value);
            return null;
        }

        public List<string> GetAllClothingFromContentPack(string cp)
        {
            foreach (var entry in Mod.instance.ClothingByContentPack)
                if (entry.Key.UniqueID == cp)
                    return new List<string>(entry.Value);
            return null;
        }

        public List<string> GetAllBootsFromContentPack(string cp)
        {
            foreach (var entry in Mod.instance.BootsByContentPack)
                if (entry.Key.UniqueID == cp)
                    return new List<string>(entry.Value);
            return null;
        }

        public event EventHandler ItemsRegistered;
        internal void InvokeItemsRegistered()
        {
            Log.Trace("Event: ItemsRegistered");
            if (this.ItemsRegistered == null)
                return;
            Util.InvokeEvent("JsonAssets.Api.ItemsRegistered", this.ItemsRegistered.GetInvocationList(), null);
        }

        public event EventHandler IdsAssigned;
        internal void InvokeIdsAssigned()
        {
            Log.Trace("Event: IdsAssigned");
            if (this.IdsAssigned == null)
                return;
            Util.InvokeEvent("JsonAssets.Api.IdsAssigned", this.IdsAssigned.GetInvocationList(), null);
        }

        public event EventHandler AddedItemsToShop;
        internal void InvokeAddedItemsToShop()
        {
            Log.Trace("Event: AddedItemsToShop");
            if (this.AddedItemsToShop == null)
                return;
            Util.InvokeEvent("JsonAssets.Api.AddedItemsToShop", this.AddedItemsToShop.GetInvocationList(), null);
        }

        public event EventHandler IdsFixed;
        internal void InvokeIdsFixed()
        {
            Log.Trace("Event: IdsFixed");
            if (this.IdsFixed == null)
                return;
            Util.InvokeEvent("JsonAssets.Api.IdsFixed", this.IdsFixed.GetInvocationList(), null);
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
            Texture2D tex = null;
            Rectangle rect = new Rectangle();
            if (entity is FruitTree fruitTree)
            {
                tex = FruitTree.texture;
                if (fruitTree.stump.Value)
                {
                    rect = new Rectangle(384, fruitTree.treeType.Value * 5 * 16 + 48, 48, 32);
                }
                else if (fruitTree.growthStage.Value <= 3)
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
    }
}
