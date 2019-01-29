using System;

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

        event EventHandler IdsAssigned;
        event EventHandler AddedItemsToShop;
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
            return Mod.instance.objectIds.ContainsKey(name) ? Mod.instance.objectIds[name] : -1;
        }

        public int GetCropId(string name)
        {
            return Mod.instance.cropIds.ContainsKey(name) ? Mod.instance.cropIds[name] : -1;
        }

        public int GetFruitTreeId(string name)
        {
            return Mod.instance.fruitTreeIds.ContainsKey(name) ? Mod.instance.fruitTreeIds[name] : -1;
        }

        public int GetBigCraftableId(string name)
        {
            return Mod.instance.bigCraftableIds.ContainsKey(name) ? Mod.instance.bigCraftableIds[name] : -1;
        }

        public int GetHatId(string name)
        {
            return Mod.instance.hatIds.ContainsKey(name) ? Mod.instance.hatIds[name] : -1;
        }

        public int GetWeaponId(string name)
        {
            return Mod.instance.weaponIds.ContainsKey(name) ? Mod.instance.weaponIds[name] : -1;
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
    }
}
