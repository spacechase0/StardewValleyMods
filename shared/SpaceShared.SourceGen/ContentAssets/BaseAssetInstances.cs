using System;
using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared.Attributes;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using Microsoft.CodeAnalysis.CSharp.Syntax;

#if HOTRELOAD_SHADERS
using MonoGame.Framework.Utilities;
using static Microsoft.Xna.Framework.Graphics.Effect;
#endif

namespace SpaceShared.ContentAssets;

#nullable enable

public partial class BaseAssetInstances
{
    protected class PropData
    {
        public string AssetName = null!;
        public Type Type = null!;
        public string? PipelineName;

        internal Func<object, object> GetValue = null!;
        internal Action<object, object> SetValue = null!;
    }

    private static Dictionary<string, PropData>? _props;
    private static Dictionary<string, PropData> Properties
    {
        get
        {
            _props ??= new();
            return _props;
        }
    }

    internal class FileAssetRegisterer
    {
        public FileAssetRegisterer(string propName, string assetName, Type type, string? pipelineName)
        {
            Properties[propName] = new PropData()
            {
                AssetName = PathUtilities.NormalizePath(assetName),
                Type = type,
                PipelineName = PathUtilities.NormalizeAssetName(pipelineName),
            };
        }
    }
#if DEBUG
    private AssetChangedWatcher watcher;
#endif
    internal StardewModdingAPI.Mod Mod { get; set; } = null!;
    public BaseAssetInstances(StardewModdingAPI.Mod mod)
    {
        Mod = mod;

        mod.Helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
        mod.Helper.Events.Content.AssetRequested += Content_AssetRequested;
        mod.Helper.Events.Content.AssetsInvalidated += Content_AssetsInvalidated;

        foreach (var propData in Properties)
        {
            var prop = this.GetType().GetProperty(propData.Key);
            propData.Value.GetValue = obj => prop.GetValue(obj);
            propData.Value.SetValue = (obj, val) => prop.SetValue(obj, val);
            ApplyLoadToProperty(propData.Key, propData.Value, first: true);

            string? pipelineName = propData.Value.PipelineName;
            if (!string.IsNullOrEmpty(pipelineName))
            {
                pipelineName = pipelineName.Replace("$$MODID$$", Mod.ModManifest.UniqueID);
                mod.Helper.GameContent.InvalidateCache(pipelineName);
            }
        }

#if DEBUG
        if (mod is IDevAssetsSource devAssets)
            watcher = new AssetChangedWatcher(devAssets, this);
#endif
    }

    private void ApplyLoadToProperty(string name, PropData data, bool first = false)
    {
        object obj;
        if (string.IsNullOrEmpty(data.PipelineName))
            obj = Load(name, data);
        else
            obj = Game1.content.Load<object>(name);

        if (!first)
        {
            if (data.Type == typeof(Texture2D))
            {
                Texture2D replacement = (Texture2D)obj;
                Texture2D existing = (Texture2D)data.GetValue(this);
                if (existing != null)
                {
                    Color[] cols = ArrayPool<Color>.Shared.Rent(existing.Width * existing.Height);
                    try
                    {
                        replacement.GetData<Color>(cols);
                        existing.SetData(cols);
                        replacement.Dispose();
                        return;
                    }
                    finally
                    {
                        ArrayPool<Color>.Shared.Return(cols);
                    }
                }
            }
#if HOTRELOAD_SHADERS
            else if (data.Type.IsAssignableTo(typeof(Effect)))
            {
                Effect replacement = (Effect)obj;
                Effect existing = (Effect)data.GetValue(this);

                string currTechName = existing.CurrentTechnique.Name;
                var existingTech = existing.Techniques;
                existing.Techniques = replacement.Techniques;
                replacement.Techniques = existingTech;
                existing.CurrentTechnique = existing.Techniques[currTechName];
                foreach (var tech in existing.Techniques)
                {
                    for (int i = 0; i < tech.Passes.Count; ++i)
                    {
                        tech.Passes._passes[i] = new(existing, tech.Passes[i]);
                    }
                }

                var existingBuff = existing.ConstantBuffers;
                existing.ConstantBuffers = replacement.ConstantBuffers;
                replacement.ConstantBuffers = existingBuff;

                EffectParameter[] replaceParams = (EffectParameter[]) replacement.Parameters._parameters.Clone();
                for (int rpi = 0; rpi < replaceParams.Length; ++rpi)
                {
                    EffectParameter replFx = replaceParams[rpi];
                    for (int epi = 0; epi < existing.Parameters.Count; ++epi)
                    {
                        EffectParameter existFx = existing.Parameters[epi];
                        string ename = existFx.Name;
                        if (existFx.Name != replFx.Name)
                            continue;

                        var existingSemantic = existFx.Semantic;
                        existFx.Semantic = replFx.Semantic;
                        replFx.Semantic = existingSemantic;

                        var existingParameterClass = existFx.ParameterClass;
                        existFx.ParameterClass = replFx.ParameterClass;
                        replFx.ParameterClass = existingParameterClass;

                        var existingParameterType = existFx.ParameterType;
                        existFx.ParameterType = replFx.ParameterType;
                        replFx.ParameterType = existingParameterType;

                        var existingRowCount = existFx.RowCount;
                        existFx.RowCount = replFx.RowCount;
                        replFx.RowCount = existingRowCount;

                        var existingColumnCount = existFx.ColumnCount;
                        existFx.ColumnCount = replFx.ColumnCount;
                        replFx.ColumnCount = existingColumnCount;

                        var existingElements = existFx.Elements;
                        existFx.Elements = replFx.Elements;
                        replFx.Elements = existingElements;

                        var existingStructureMembers = existFx.StructureMembers;
                        existFx.StructureMembers = replFx.StructureMembers;
                        replFx.StructureMembers = existingStructureMembers;

                        var existingAnnotations = existFx.Annotations;
                        existFx.Annotations = replFx.Annotations;
                        replFx.Annotations = existingAnnotations;

                        var existingStateKey = existFx.StateKey;
                        existFx.StateKey = replFx.StateKey;
                        replFx.StateKey = existingStateKey;
                        break;
                    }
                }

                Util.Swap(ref existing._shaders, ref replacement._shaders);

                replacement.Dispose();
                return;
            }
#endif
        }

        data.SetValue(this, obj);
    }

    protected virtual object Load(string propertyName, PropData propertyData)
    {
        if (propertyData.Type == typeof(Texture2D))
        {
#if DEBUG
            if (Mod is IDevAssetsSource devAssets)
                return Texture2D.FromFile(Game1.graphics.GraphicsDevice, Path.Combine(devAssets.DevAssetsFolder, propertyData.AssetName));
#endif
            return Mod.Helper.ModContent.Load<Texture2D>(Path.Combine("assets", propertyData.AssetName));
        }
        else if (propertyData.Type.IsAssignableTo(typeof(Effect)))
        {
            string path = Path.Combine(Mod.Helper.DirectoryPath, "assets", propertyData.AssetName);
#if DEBUG
            if (Mod is IDevAssetsSource devAssets)
                path = Path.Combine(devAssets.DevAssetsFolder, propertyData.AssetName);
#endif

            ConstructorInfo constr = propertyData.Type.GetConstructor([typeof(GraphicsDevice), typeof(byte[])]);
            return constr.Invoke([Game1.graphics.GraphicsDevice, File.ReadAllBytes(path)]);
        }

        return null;
    }

    private ConcurrentBag<string> toInvalidate = new();
    [EventPriority(EventPriority.High)]
    private void GameLoop_UpdateTicking(object? sender, UpdateTickingEventArgs e)
    {
#if DEBUG
        if (Game1.currentGameTime != null)
            watcher?.Update(Game1.currentGameTime.ElapsedGameTime);
#endif

        var toInvalidate = this.toInvalidate.ToArray();
        this.toInvalidate.Clear();
        foreach (var propName in toInvalidate)
        {
            if (!Properties.TryGetValue(propName, out var data))
                continue;

            ApplyLoadToProperty(propName, data);
        }
    }

#if DEBUG
    internal protected virtual void Reload(string asset)
    {
        asset = PathUtilities.NormalizePath(asset);
        var prop = Properties.FirstOrDefault(kvp => kvp.Value.AssetName == asset);
        if (!string.IsNullOrEmpty(prop.Key))
            toInvalidate.Add(prop.Key);
    }
#endif

    private void Content_AssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        foreach (var entry in Properties)
        {
            string? pipelineName = entry.Value.PipelineName;
            if (string.IsNullOrEmpty(pipelineName))
                continue;

            pipelineName = pipelineName.Replace("$$MODID$$", Mod.ModManifest.UniqueID);

            if (!e.NameWithoutLocale.IsEquivalentTo(pipelineName))
                continue;

            e.LoadFrom(() => Load(pipelineName, entry.Value), AssetLoadPriority.Exclusive);
        }
    }

    private void Content_AssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        foreach (var entry in Properties)
        {
            string? pipelineName = entry.Value.PipelineName;
            if (string.IsNullOrEmpty(pipelineName))
                continue;

            pipelineName = pipelineName.Replace("$$MODID$$", Mod.ModManifest.UniqueID);

            if (!e.NamesWithoutLocale.Any(n => n.IsEquivalentTo(pipelineName)))
                continue;

            ApplyLoadToProperty(entry.Key, entry.Value);
        }
    }
}
