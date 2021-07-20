using Microsoft.Xna.Framework;
using StardewModdingAPI;
using xTile;

namespace Magic.Framework
{
    /// <summary>An asset editor which adds the altar to Pierre's shop map.</summary>
    internal class AltarMapEditor : IAssetEditor
    {
        /*********
        ** Fields
        *********/
        /// <summary>The SMAPI API for loading content assets.</summary>
        private readonly IContentHelper Content;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="content">The SMAPI API for loading content assets.</param>
        public AltarMapEditor(IContentHelper content)
        {
            this.Content = content;
        }

        /// <inheritdoc />
        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Maps/SeedShop");
        }

        /// <inheritdoc />
        public void Edit<T>(IAssetData asset)
        {
            Map altar = this.Content.Load<Map>("assets/altar.tmx");
            asset.AsMap().PatchMap(altar, targetArea: new Rectangle(36, 15, 3, 3));
        }
    }
}
