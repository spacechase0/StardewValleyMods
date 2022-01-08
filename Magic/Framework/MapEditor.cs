using Microsoft.Xna.Framework;
using SpaceShared;
using StardewModdingAPI;
using xTile;
using xTile.Layers;
using xTile.Tiles;

namespace Magic.Framework
{
    /// <summary>An asset editor which makes map changes for Magic.</summary>
    internal class MapEditor : IAssetEditor
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod configuration.</summary>
        private readonly Configuration Config;

        /// <summary>The SMAPI API for loading content assets.</summary>
        private readonly IContentHelper Content;

        /// <summary>Whether the player has Stardew Valley Expanded installed.</summary>
        private readonly bool HasStardewValleyExpanded;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The mod configuration.</param>
        /// <param name="content">The SMAPI API for loading content assets.</param>
        /// <param name="hasStardewValleyExpanded">Whether the player has Stardew Valley Expanded installed.</param>
        public MapEditor(Configuration config, IContentHelper content, bool hasStardewValleyExpanded)
        {
            this.Config = config;
            this.Content = content;
            this.HasStardewValleyExpanded = hasStardewValleyExpanded;
        }

        /// <inheritdoc />
        public bool CanEdit<T>(IAssetInfo asset)
        {
            return
                asset.AssetNameEquals($"Maps/{this.Config.AltarLocation}")
                || asset.AssetNameEquals("Maps/WizardHouse");
        }

        /// <inheritdoc />
        public void Edit<T>(IAssetData asset)
        {
            // add altar
            if (asset.AssetNameEquals($"Maps/{this.Config.AltarLocation}"))
            {
                Map altar = this.Content.Load<Map>("assets/altar.tmx");
                asset.AsMap().PatchMap(altar, targetArea: new Rectangle(this.Config.AltarX, this.Config.AltarY, 3, 3));
            }

            // add radio to Wizard's tower
            else if (asset.AssetNameEquals("Maps/WizardHouse"))
            {
                Map map = asset.AsMap().Data;

                // get buildings layer
                Layer buildingsLayer = map.GetLayer("Buildings");
                if (buildingsLayer == null)
                {
                    Log.Warn("Can't add radio to Wizard's tower: 'Buildings' layer not found.");
                    return;
                }

                // get front layer
                Layer frontLayer = map.GetLayer("Front");
                if (frontLayer == null)
                {
                    Log.Warn("Can't add radio to Wizard's tower: 'Front' layer not found.");
                    return;
                }

                // get tilesheet
                TileSheet tilesheet = map.GetTileSheet("untitled tile sheet");
                if (tilesheet == null)
                {
                    Log.Warn("Can't add radio to Wizard's tower: main tilesheet not found.");
                    return;
                }

                // add radio
                int radioX = 1;
                int radioY = 5;
                if (this.HasStardewValleyExpanded)
                {
                    radioX = 5;
                    radioY = 23;
                }
                frontLayer.Tiles[radioX, radioY] = new StaticTile(frontLayer, tilesheet, BlendMode.Alpha, 512);
                (buildingsLayer.Tiles[radioX, radioY] ?? frontLayer.Tiles[radioX, radioY]).Properties["Action"] = "MagicRadio";
            }
        }
    }
}
