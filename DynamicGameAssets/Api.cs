using DynamicGameAssets.Framework;
using DynamicGameAssets.Game;
using DynamicGameAssets.PackData;
using Microsoft.Xna.Framework;
using SpaceShared;
using StardewModdingAPI;

namespace DynamicGameAssets
{
    public class Api : IDynamicGameAssetsApi
    {
        /// <inheritdoc/>
        public string GetDGAItemId(object item_)
        {
            if (item_ is IDGAItem item)
                return item.FullId;
            else
                return null;
        }

        /// <inheritdoc/>
        public string GetDGAItemId(int fakeIndex)
        {
            if (Mod.itemLookup.ContainsKey(fakeIndex))
            {
                return Mod.itemLookup[fakeIndex];
            }
            return null;
        }

        /// <inheritdoc/>
        public int? GetDGAFakeIndex(object item_)
        {
            if (item_ is IDGAItem item)
                return item.FullId.GetDeterministicHashCode();
            else
                return null;
        }

        /// <inheritdoc/>
        public int? GetDGAFakeIndex(string fullId)
        {
            return fullId.GetDeterministicHashCode();
        }

        /// <inheritdoc/>
        public string GetDGAFakeObjectInformation(int fakeIndex)
        {
            if (this.GetDGAItemId(fakeIndex) is string fullId)
            {
                return (Mod.Find(fullId) as ObjectPackData)?.GetFakeData();
            }
            return null;
        }

        /// <inheritdoc/>
        public object SpawnDGAItem(string fullId, Color? color)
        {
            object spawnDgaItem = this.SpawnDGAItem(fullId);
            if(color.HasValue && spawnDgaItem is CustomObject obj)
            {
                obj.ObjectColor = color;
            }
            return spawnDgaItem;
        }

        /// <inheritdoc/>
        public object SpawnDGAItem(string fullId)
        {
            return Mod.Find(fullId)?.ToItem();
        }

        /// <inheritdoc/>
        public void AddEmbeddedPack(IManifest manifest, string dir)
        {
            Mod.AddEmbeddedContentPack(manifest, dir);
        }
    }
}
