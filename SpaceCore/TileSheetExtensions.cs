using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace SpaceCore
{
    public static class TileSheetExtensions
    {
        internal class ExtensionData
        {
            public ExtensionData(string assetPath, int unitSize)
            {
                this.AssetPath = assetPath;
                this.UnitSize = unitSize;
            }

            public string AssetPath { get; }
            public int UnitSize { get; }
            public Texture2D BaseTileSheet { get; set; }
            public List<Texture2D> Extensions { get; } = new();
        }

        internal static Dictionary<string, ExtensionData> ExtendedTextureAssets = new();
        private static readonly Dictionary<Texture2D, ExtensionData> ExtendedTextures = new();

        internal static void Init()
        {
            SpaceCore.Instance.Helper.Content.AssetLoaders.Add(new ExtendedTileSheetLoader());
        }

        public static void RegisterExtendedTileSheet(string asset, int unitSize)
        {
            if (TileSheetExtensions.ExtendedTextureAssets.ContainsKey(asset))
                return;

            var data = new ExtensionData(asset, unitSize);
            data.BaseTileSheet = Game1.content.Load<Texture2D>(asset);
            TileSheetExtensions.ExtendedTextureAssets.Add(asset, data);
            TileSheetExtensions.ExtendedTextures.Add(data.BaseTileSheet, data);
        }

        public static int GetTileSheetUnitSize(Texture2D tex)
        {
            if (TileSheetExtensions.ExtendedTextures.ContainsKey(tex))
                return TileSheetExtensions.ExtendedTextures[tex].UnitSize;
            return -1;
        }

        public static int GetTileSheetUnitSize(string asset)
        {
            if (TileSheetExtensions.ExtendedTextureAssets.ContainsKey(asset))
                return TileSheetExtensions.ExtendedTextureAssets[asset].UnitSize;
            return -1;
        }

        public static Texture2D GetTileSheet(Texture2D tex, int index)
        {
            if (!TileSheetExtensions.ExtendedTextures.ContainsKey(tex))
                return tex;
            var data = TileSheetExtensions.ExtendedTextures[tex];

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
                this.TileSheet = ts;
                this.Y = y;
            }
        }

        public static AdjustedTarget GetAdjustedTileSheetTarget(Texture2D tex, Rectangle sourceRect)
        {
            int unit = TileSheetExtensions.GetTileSheetUnitSize(tex);
            return TileSheetExtensions.GetAdjustedTileSheetTargetImpl(unit, sourceRect);
        }

        public static AdjustedTarget GetAdjustedTileSheetTarget(string asset, Rectangle sourceRect)
        {
            int unit = TileSheetExtensions.GetTileSheetUnitSize(asset);
            return TileSheetExtensions.GetAdjustedTileSheetTargetImpl(unit, sourceRect);
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

            int index = sourceRect.Y / unit;
            int extra = sourceRect.Y % unit;

            int tileSheet = 0;
            int maxTileIndexPer = 4096 / unit;
            while (index >= maxTileIndexPer)
            {
                index -= maxTileIndexPer;
                ++tileSheet;
            }

            return new AdjustedTarget(tileSheet, index * unit + extra);
        }

        public static void PatchExtendedTileSheet(this IAssetDataForImage asset, Texture2D source, Rectangle? sourceArea = null, Rectangle? targetArea = null, PatchMode patchMode = PatchMode.Replace)
        {
            string assetName = asset.AssetName.Replace('/', '\\');
            if (!TileSheetExtensions.ExtendedTextureAssets.ContainsKey(assetName) || !targetArea.HasValue)
            {
                asset.PatchImage(source, sourceArea, targetArea, patchMode);
                return;
            }
            TileSheetExtensions.ExtendedTextures.Remove(TileSheetExtensions.ExtendedTextureAssets[assetName].BaseTileSheet);
            TileSheetExtensions.ExtendedTextureAssets[assetName].BaseTileSheet = asset.Data;
            TileSheetExtensions.ExtendedTextures.Add(TileSheetExtensions.ExtendedTextureAssets[assetName].BaseTileSheet, TileSheetExtensions.ExtendedTextureAssets[assetName]);

            var adjustedTarget = TileSheetExtensions.GetAdjustedTileSheetTarget(asset.Data, targetArea.Value);
            //Log.trace("Tilesheet target:" + adjustedTarget.TileSheet + " " + adjustedTarget.Y);
            if (adjustedTarget.TileSheet == 0)
            {
                asset.PatchImage(source, sourceArea, targetArea, patchMode);
                return;
            }

            // Cheaty hack so I don't have to reimplement patch
            var oldData = asset.Data;
            var dataProp = asset.GetType().GetProperty("Data");
            try
            {
                dataProp.SetValue(asset, TileSheetExtensions.GetTileSheet(oldData, adjustedTarget.TileSheet));

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
            Texture2D oldTS;
            foreach (var asset in TileSheetExtensions.ExtendedTextureAssets)
            {
                oldTS = asset.Value.BaseTileSheet;
                asset.Value.BaseTileSheet = Game1.content.Load<Texture2D>(asset.Key);
                if (asset.Value.BaseTileSheet == null) {
                    Log.Error("WHAT? null " + asset.Key);
                    TileSheetExtensions.ExtendedTextures.Remove(oldTS);
                    oldTS.Dispose();
                    }
                else {
                    TileSheetExtensions.ExtendedTextures[asset.Value.BaseTileSheet] = asset.Value;
                    if (oldTS != asset.Value.BaseTileSheet) oldTS.Dispose();
                    }
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
            foreach (var extAsset in TileSheetExtensions.ExtendedTextureAssets)
            {
                for (int i = 0; i < extAsset.Value.Extensions.Count; ++i)
                    if (asset.AssetNameEquals(extAsset.Key + (i + 2).ToString()))
                        return true;
            }

            return false;
        }

        public T Load<T>(IAssetInfo asset)
        {
            foreach (var extAsset in TileSheetExtensions.ExtendedTextureAssets)
            {
                for (int i = 0; i < extAsset.Value.Extensions.Count; ++i)
                    if (asset.AssetNameEquals(extAsset.Key + (i + 2).ToString()))
                        return (T)(object)extAsset.Value.Extensions[i];
            }

            return default;
        }
    }
}
