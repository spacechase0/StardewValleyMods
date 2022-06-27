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
        /// Gets the names of all installed packs.
        /// </summary>
        /// <returns>Array of all pack names.</returns>
        string[] ListContentPacks();

        /// <summary>
        /// Gets all items provided by a pack.
        /// </summary>
        /// <param name="packname">The name of the pack.</param>
        /// <returns>Namespaced item names.</returns>
        string[]? GetItemsByPack(string packname);

        /// <summary>
        /// Gets all the items (namespaced names)
        /// </summary>
        /// <returns>A list of all items.</returns>
        string[] GetAllItems();

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
