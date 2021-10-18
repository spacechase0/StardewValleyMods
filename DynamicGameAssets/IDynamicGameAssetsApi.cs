using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace DynamicGameAssets
{
    public interface IDynamicGameAssetsApi
    {
        /// <summary>
        /// Get the DGA item ID of this item, if it has one.
        /// </summary>
        /// <param name="item">The item to get the DGA item ID of.</param>
        /// <returns>The DGA item ID if it has one, otherwise null.</returns>
        string GetDGAItemId(object item);

        /// <summary>
        /// Get the DGA item ID of this item, if it has one.
        /// </summary>
        /// <param name="item">The Fake Index to get the DGA item ID of.</param>
        /// <returns>The DGA item ID if it has one, otherwise null.</returns>
        string GetDGAItemId(int fakeIndex);

        /// <summary>
        /// Get the DGA Fake Index of this item, if it has one.
        /// </summary>
        /// <param name="item">The item to get the DGA item ID of.</param>
        /// <returns>The DGA Fake Index if it has one, otherwise null.</returns>
        int? GetDGAFakeIndex(object item);

        /// <summary>
        /// Get the DGA Fake Index of the item referenced with its full ID ("mod.id/ItemId")..
        /// </summary>
        /// <param name="fullId">The full ID of the item to obtain the Fake Index.</param>
        /// <returns>The DGA Fake Index.</returns>
        int? GetDGAFakeIndex(string fullId);

        /// <summary>
        /// Get the DGA Fake Object Information of the item referenced with its Fake Index.
        /// </summary>
        /// <param name="fakeIndex">The Fake Index to get the Fake information of.</param>
        /// <returns>The DGA Fake Index if it has one, otherwise null.</returns>
        string GetDGAFakeObjectInformation(int fakeIndex);

        /// <summary>
        /// Spawn a DGA item, referenced with its full ID ("mod.id/ItemId").
        /// Some items, such as crafting recipes or crops, don't have an item representation.
        /// </summary>
        /// <param name="fullId">The full ID of the item to spawn.</param>
        /// <param name="color">The color of the item.</param>
        /// <returns></returns>
        object SpawnDGAItem(string fullId, Color? color);

        /// <summary>
        /// Spawn a DGA item, referenced with its full ID ("mod.id/ItemId").
        /// Some items, such as crafting recipes or crops, don't have an item representation.
        /// </summary>
        /// <param name="fullId">The full ID of the item to spawn.</param>
        /// <returns></returns>
        object SpawnDGAItem(string fullId);

        /// <summary>
        /// Register a DGA pack embedded in another mod.
        /// Needs the standard DGA fields in the manifest. (See documentation.)
        /// Probably shouldn't use config-schema.json for these, because if you do it will overwrite your mod's config.json.
        /// </summary>
        /// <param name="manifest">The mod manifest.</param>
        /// <param name="dir">The absolute path to the directory of the pack.</param>
        void AddEmbeddedPack(IManifest manifest, string dir);
    }
}
