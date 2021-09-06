using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicGameAssets.Game;
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
        string GetDGAItemId( object item );

        /// <summary>
        /// Register a DGA pack embedded in another mod.
        /// Needs the standard DGA fields in the manifest. (See documentation.)
        /// Probably shouldn't use config-schema.json for these, because if you do it will overwrite your mod's config.json.
        /// </summary>
        /// <param name="manifest">The mod manifest.</param>
        /// <param name="dir">The absolute path to the directory of the pack.</param>
        void AddEmbeddedPack( IManifest manifest, string dir );
    }

    public class Api : IDynamicGameAssetsApi
    {
        /// <inheritdoc/>
        public string GetDGAItemId( object item_ )
        {
            if ( item_ is IDGAItem item )
                return item.FullId;
            else
                return null;
        }

        /// <inheritdoc/>
        public void AddEmbeddedPack( IManifest manifest, string dir )
        {
            Mod.AddEmbeddedContentPack( manifest, dir );
        }
    }
}
