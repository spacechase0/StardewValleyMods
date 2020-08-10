using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore
{
    public static class TileSheetExtensions
    {
        internal class ExtensionData
        {
            public ExtensionData(string assetPath, int unitSize)
            {
                AssetPath = assetPath;
                UnitSize = unitSize;
            }

            public string AssetPath { get; }
            public int UnitSize { get; }
            public Texture2D BaseTileSheet { get; set; }
            public List<Texture2D> Extensions { get; } = new List<Texture2D>();
        }

        internal static Dictionary<string, ExtensionData> extendedTextureAssets = new Dictionary<string, ExtensionData>();
        private static Dictionary<Texture2D, ExtensionData> extendedTextures = new Dictionary<Texture2D, ExtensionData>();

        internal static void init()
        {
            SpaceCore.instance.Helper.Content.AssetLoaders.Add(new ExtendedTileSheetLoader());
        }

        public static void RegisterExtendedTileSheet(string asset, int unitSize)
        {
            if (extendedTextureAssets.ContainsKey(asset))
                return;

            var data = new ExtensionData(asset, unitSize);
            data.BaseTileSheet = Game1.content.Load<Texture2D>(asset);
            extendedTextureAssets.Add(asset, data);
            extendedTextures.Add(data.BaseTileSheet, data);
        }

        public static int GetTileSheetUnitSize(Texture2D tex)
        {
            if (extendedTextures.ContainsKey(tex))
                return extendedTextures[tex].UnitSize;
            return -1;
        }

        public static int GetTileSheetUnitSize(string asset)
        {
            if (extendedTextureAssets.ContainsKey(asset))
                return extendedTextureAssets[asset].UnitSize;
            return -1;
        }

        public static Texture2D GetTileSheet(Texture2D tex, int index)
        {
            if (!extendedTextures.ContainsKey(tex))
                return tex;
            var data = extendedTextures[tex];

            while (data.Extensions.Count <= index - 1)
                data.Extensions.Add(new Texture2D(Game1.graphics.GraphicsDevice, tex.Width, 4096));

            return index == 0 ? tex : data.Extensions[index - 1];
        }

        public struct AdjustedTarget
        {
            public int TileSheet;
            public int Y;

            public AdjustedTarget(int ts, int y)
            {
                TileSheet = ts;
                Y = y;
            }
        }

        public static AdjustedTarget GetAdjustedTileSheetTarget(Texture2D tex, Rectangle sourceRect )
        {
            int unit = GetTileSheetUnitSize(tex);
            return GetAdjustedTileSheetTargetImpl(unit, sourceRect);
        }

        public static AdjustedTarget GetAdjustedTileSheetTarget(string asset, Rectangle sourceRect)
        {
            int unit = GetTileSheetUnitSize(asset);
            return GetAdjustedTileSheetTargetImpl(unit, sourceRect);
        }

        private static AdjustedTarget GetAdjustedTileSheetTargetImpl(int unit, Rectangle sourceRect)
        {
            if (unit <= 0)
                return new AdjustedTarget(0, sourceRect.Y); // Something went wrong or this tilesheet isn't affected
            /*if (sourceRect.Height != unit || sourceRect.Y % unit != 0)
            {
                Log.warn("Unsupported use case for automatic tilesheet expansion");
                return new AdjustedTarget(0, sourceRect.Y);
            }*/
            
            int index = (int)sourceRect.Y / unit;
            int extra = (int)sourceRect.Y % unit;

            int tileSheet = 0;
            int maxTileIndexPer = 4096 / unit;
            while (index >= maxTileIndexPer)
            {
                index -= maxTileIndexPer;
                ++tileSheet;
            }

            return new AdjustedTarget(tileSheet, index * unit + extra);
        }

        public static void PatchExtendedTileSheet(this IAssetDataForImage asset, Texture2D source, Rectangle? sourceArea = null, Rectangle? targetArea = null, PatchMode patchMode = PatchMode.Replace )
        {
            string assetName = asset.AssetName.Replace('/', '\\');
            if (!extendedTextureAssets.ContainsKey(assetName) || !targetArea.HasValue )
            {
                asset.PatchImage(source, sourceArea, targetArea, patchMode);
                return;
            }
            extendedTextures.Remove(extendedTextureAssets[assetName].BaseTileSheet);
            extendedTextureAssets[assetName].BaseTileSheet = asset.Data;
            extendedTextures.Add(extendedTextureAssets[assetName].BaseTileSheet, extendedTextureAssets[assetName]);

            var adjustedTarget = GetAdjustedTileSheetTarget(asset.Data, targetArea.Value);
            //Log.trace("Tilesheet target:" + adjustedTarget.TileSheet + " " + adjustedTarget.Y);
            if ( adjustedTarget.TileSheet == 0 )
            {
                asset.PatchImage(source, sourceArea, targetArea, patchMode);
                return;
            }

            // Cheaty hack so I don't have to reimplement patch
            var oldData = asset.Data;
            var dataProp = asset.GetType().GetProperty("Data");
            try
            {
                dataProp.SetValue(asset, GetTileSheet(oldData, adjustedTarget.TileSheet));

                Rectangle r = targetArea.Value;
                r.Y = adjustedTarget.Y;
                //Log.trace($"Ext-patching on {assetName}={extendedTextureAssets[assetName].AssetPath}: {r}/{asset.Data.Width}x{asset.Data.Height}");
                asset.PatchImage(source, sourceArea, r, patchMode);
            }
            finally
            {
                dataProp.SetValue(asset, oldData);
            }
        }

        internal static void UpdateReferences()
        {
            foreach ( var asset in extendedTextureAssets )
            {
                extendedTextures.Remove(asset.Value.BaseTileSheet);
                asset.Value.BaseTileSheet = Game1.content.Load<Texture2D>(asset.Key);
                extendedTextures.Add(asset.Value.BaseTileSheet, asset.Value);
            }
        }

        // Sometimes when an asset is invalidated the whole texture object is replaced.
        // So we need to remove old textures and re-add them in case they changed
        /*
        public static void OnAssetInvalidated(string asset)
        {
            if (extendedTextureAssets.ContainsKey(asset))
            {
                var data = extendedTextureAssets[asset];
                extendedTextures.Remove(data.BaseTilesheet);
                data.BaseTilesheet = Game1.content.Load<Texture2D>(asset);
                extendedTextures.Add(data.BaseTilesheet, data);
            }
        }
        */
    }

    public class ExtendedTileSheetLoader : IAssetLoader
    {
        public bool CanLoad<T>(IAssetInfo asset)
        {
            foreach ( var extAsset in TileSheetExtensions.extendedTextureAssets )
            {
                for (int i = 0; i < extAsset.Value.Extensions.Count; ++i)
                    if (asset.AssetNameEquals(extAsset.Key + (i + 2).ToString()))
                        return true;
            }

            return false;
        }

        public T Load<T>(IAssetInfo asset)
        {
            foreach (var extAsset in TileSheetExtensions.extendedTextureAssets)
            {
                for (int i = 0; i < extAsset.Value.Extensions.Count; ++i)
                    if (asset.AssetNameEquals(extAsset.Key + (i + 2).ToString()))
                        return (T) (object) extAsset.Value.Extensions[i];
            }

            return default(T);
        }
    }
}
