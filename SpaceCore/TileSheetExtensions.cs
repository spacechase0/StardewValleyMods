using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SpaceShared;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;

#nullable enable

namespace SpaceCore
{
    public static class TileSheetExtensions
    {

        public const int MAXTILESHEETHEIGHT = 4096;

        [DebuggerDisplay("{AssetPath}")]
        internal class ExtensionData
        {
            public ExtensionData(string assetPath, int unitSize)
            {
                this.AssetPath = assetPath;
                this.UnitSize = unitSize;
            }

            public string AssetPath { get; }
            public int UnitSize { get; }
            public Texture2D? BaseTileSheet { get; set; }
            public List<Texture2D> Extensions { get; } = new();
        }

        internal static Dictionary<string, ExtensionData> ExtendedTextureAssets = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<Texture2D, ExtensionData> ExtendedTextures = new();

        internal static void Init()
        {
            SpaceCore.Instance.Helper.Events.Content.AssetRequested += Load;
            SpaceCore.Instance.Helper.Events.Content.AssetReady += Ready;
        }

        private static void Load(object? _, AssetRequestedEventArgs e)
        {
            // all assets we want to load will end with a number.
            if (char.IsDigit(e.NameWithoutLocale.BaseName[^1]))
            {
                foreach (var (assetName, extdata) in TileSheetExtensions.ExtendedTextureAssets)
                {
                    if (extdata.Extensions.Count > 0 && e.NameWithoutLocale.BaseName.StartsWith(assetName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(e.NameWithoutLocale.BaseName.AsSpan(assetName.Length), out int pos)
                            && pos >= 2 && pos < extdata.Extensions.Count + 2)
                        {
                            e.LoadFrom(() => extdata.Extensions[pos - 2], AssetLoadPriority.Exclusive);
                        }
                        else
                        {
                            Log.Error($"Failed to find extension for {e.NameWithoutLocale.BaseName}.");
                        }
                        break;
                    }
                }
            }
        }

        [EventPriority((EventPriority)int.MaxValue)]
        private static void Ready(object? sender, AssetReadyEventArgs e)
        {
            if (ExtendedTextureAssets.TryGetValue(e.NameWithoutLocale.BaseName, out ExtensionData? data))
            {
                if (data.BaseTileSheet is not null)
                    ExtendedTextures.Remove(data.BaseTileSheet);
                data.BaseTileSheet = Game1.content.Load<Texture2D>(e.NameWithoutLocale.BaseName);
                ExtendedTextures[data.BaseTileSheet] = data;
                ExtendedTextureAssets[e.NameWithoutLocale.BaseName] = data;
            }
        }

        public static void RegisterExtendedTileSheet(string asset, int unitSize)
        {
            if (TileSheetExtensions.ExtendedTextureAssets.TryGetValue(asset, out var extension))
            {
                if (extension.UnitSize != unitSize)
                    Log.Error($"{asset} previously registered with unitSize {extension.UnitSize}, cannot be re-registered with differing unit size {unitSize}");
                return;
            }

            var data = new ExtensionData(asset, unitSize)
            {
                BaseTileSheet = Game1.content.Load<Texture2D>(asset)
            };
            TileSheetExtensions.ExtendedTextureAssets.Add(asset, data);
            TileSheetExtensions.ExtendedTextures.Add(data.BaseTileSheet, data);
        }

        public static int GetTileSheetUnitSize(Texture2D tex)
        {
            return TileSheetExtensions.ExtendedTextures.TryGetValue(tex, out ExtensionData? data)
                ? data.UnitSize
                : -1;
        }

        public static int GetTileSheetUnitSize(string asset)
        {
            return TileSheetExtensions.ExtendedTextureAssets.TryGetValue(asset, out ExtensionData? data)
                ? data.UnitSize
                : -1;
        }

        public static Texture2D GetTileSheet(Texture2D tex, int index)
        {
            if (!TileSheetExtensions.ExtendedTextures.TryGetValue(tex, out ExtensionData? data))
                return tex;

            while (data.Extensions.Count <= index - 1)
            {
                Log.DebugOnlyLog($"Adding extended tilesheet {data.Extensions.Count} for {tex.Name ?? "unknown texture"}");
                data.Extensions.Add(new Texture2D(Game1.graphics.GraphicsDevice, tex.Width, TileSheetExtensions.MAXTILESHEETHEIGHT));
            }

            return index == 0
                ? tex
                : data.Extensions[index - 1];
        }

        /// <summary>
        /// Attempts to get the relevant extended tilesheet for a given assetname.
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="index"></param>
        /// <returns>Tilesheet if possible, null otherwise.</returns>
        public static Texture2D? GetTileSheet(string assetName, int index)
        {
            if (!TileSheetExtensions.ExtendedTextureAssets.TryGetValue(assetName, out var extData) || extData.BaseTileSheet is null)
                return null;

            return GetTileSheet(extData.BaseTileSheet, index);
        }

        [DebuggerDisplay("ts = {TileSheet}, y = {Y}")]
        public readonly struct AdjustedTarget
        {
            public readonly int TileSheet;
            public readonly int Y;

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
            if (unit <= 0 || sourceRect.Y == 0)
                return new AdjustedTarget(0, sourceRect.Y); // Something went wrong or this tilesheet isn't affected
            /*if (sourceRect.Height != unit || sourceRect.Y % unit != 0)
            {
                Log.warn("Unsupported use case for automatic tilesheet expansion");
                return new AdjustedTarget(0, sourceRect.Y);
            }*/

            int index = sourceRect.Y / unit;
            int extra = sourceRect.Y % unit;

            int tileSheet = 0;
            int maxTileIndexPer = MAXTILESHEETHEIGHT / unit;
            while (index >= maxTileIndexPer)
            {
                index -= maxTileIndexPer;
                ++tileSheet;
            }

            return new AdjustedTarget(tileSheet, index * unit + extra);
        }

        public static void PatchExtendedTileSheet(this IAssetDataForImage asset, Texture2D source, Rectangle? sourceArea = null, Rectangle? targetArea = null, PatchMode patchMode = PatchMode.Replace, bool includeLocale = false)
        {
            string assetName = (includeLocale ? asset.Name : asset.NameWithoutLocale).BaseName;
            if (!TileSheetExtensions.ExtendedTextureAssets.TryGetValue(assetName, out ExtensionData? assetData) || !targetArea.HasValue)
            {
                asset.PatchImage(source, sourceArea, targetArea, patchMode);
                return;
            }

            if (assetData.BaseTileSheet != asset.Data)
            {
                if (assetData.BaseTileSheet is not null)
                    TileSheetExtensions.ExtendedTextures.Remove(assetData.BaseTileSheet);
                TileSheetExtensions.ExtendedTextures.Add(asset.Data, assetData);
                assetData.BaseTileSheet = asset.Data;
            }

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
                dataProp!.SetValue(asset, TileSheetExtensions.GetTileSheet(oldData, adjustedTarget.TileSheet));

                Rectangle r = targetArea.Value;
                r.Y = adjustedTarget.Y;
                //Log.trace($"Ext-patching on {assetName}={extendedTextureAssets[assetName].AssetPath}: {r}/{asset.Data.Width}x{asset.Data.Height}");
                asset.PatchImage(source, sourceArea, r, patchMode);
            }
            finally
            {
                dataProp!.SetValue(asset, oldData);
            }
        }

        public static void PatchExtendedTileSheet(this IAssetDataForImage asset, IRawTextureData source, Rectangle? sourceArea = null, Rectangle? targetArea = null, PatchMode patchMode = PatchMode.Replace, bool includeLocale = false)
        {
            string assetName = (includeLocale ? asset.Name : asset.NameWithoutLocale).BaseName;
            if (!TileSheetExtensions.ExtendedTextureAssets.TryGetValue(assetName, out ExtensionData? assetData) || !targetArea.HasValue)
            {
                asset.PatchImage(source, sourceArea, targetArea, patchMode);
                return;
            }

            if (assetData.BaseTileSheet != asset.Data)
            {
                if (assetData.BaseTileSheet is not null)
                    TileSheetExtensions.ExtendedTextures.Remove(assetData.BaseTileSheet);
                TileSheetExtensions.ExtendedTextures.Add(asset.Data, assetData);
                assetData.BaseTileSheet = asset.Data;
            }

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
                dataProp!.SetValue(asset, TileSheetExtensions.GetTileSheet(oldData, adjustedTarget.TileSheet));

                Rectangle r = targetArea.Value;
                r.Y = adjustedTarget.Y;
                //Log.trace($"Ext-patching on {assetName}={extendedTextureAssets[assetName].AssetPath}: {r}/{asset.Data.Width}x{asset.Data.Height}");
                asset.PatchImage(source, sourceArea, r, patchMode);
            }
            finally
            {
                dataProp!.SetValue(asset, oldData);
            }
        }

        /// <summary>Update tilesheet texture references if needed.</summary>
        /// <returns>Returns the dereferenced tilesheet textures.</returns>
        internal static IEnumerable<Texture2D> UpdateReferences()
        {
            foreach (var asset in TileSheetExtensions.ExtendedTextureAssets)
            {
                Texture2D? oldTexture = asset.Value.BaseTileSheet;

                if (oldTexture is null)
                    continue;

                try
                {
                    asset.Value.BaseTileSheet = Game1.content.Load<Texture2D>(asset.Key);
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed updating tilesheet reference '{asset.Key}'. Technical details:\n{ex}");
                    TileSheetExtensions.ExtendedTextures.Remove(oldTexture);
                    continue;
                }

                if (asset.Value.BaseTileSheet is null)
                {
                    Log.Error("WHAT? null " + asset.Key);
                    TileSheetExtensions.ExtendedTextures.Remove(oldTexture);
                    yield return oldTexture;
                }
                else
                {
                    TileSheetExtensions.ExtendedTextures[asset.Value.BaseTileSheet] = asset.Value;
                    if (oldTexture != asset.Value.BaseTileSheet)
                        yield return oldTexture;
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

    /*
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
            foreach (KeyValuePair<string, TileSheetExtensions.ExtensionData> extAsset in TileSheetExtensions.ExtendedTextureAssets)
            {
                for (int i = 0; i < extAsset.Value.Extensions.Count; ++i)
                    if (asset.AssetNameEquals(extAsset.Key + (i + 2).ToString()))
                        return (T)(object)extAsset.Value.Extensions[i];
            }
            return default;
        }
    } */
}
