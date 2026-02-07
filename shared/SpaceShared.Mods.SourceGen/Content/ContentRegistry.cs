using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SpaceShared.Attributes;
using StardewModdingAPI.Events;

namespace SpaceShared.Content;

internal static partial class ContentRegistry
{
    private static Dictionary<string, Type> _customDictionaryAssets;
    public static Dictionary<string, Type> CustomDictionaryAssets
    {
        get
        {
            _customDictionaryAssets ??= new();
            return _customDictionaryAssets;
        }
    }

    private static Dictionary<string, List<Type>> _dictionaryData;
    public static Dictionary<string, List<Type>> DictionaryData
    {
        get
        {
            _dictionaryData ??= new();
            return _dictionaryData;
        }
    }

    private class CustomDictionaryAssetRegisterer
    {
        public CustomDictionaryAssetRegisterer(string assetName, Type modelType)
        {
            CustomDictionaryAssets[assetName] = modelType;
        }
    }

    private class DictionaryDataRegisterer
    {
        public DictionaryDataRegisterer(string assetName, Type populatorType)
        {
            var customAttr = populatorType.CustomAttributes.First(ca => ca.AttributeType.Name.StartsWith($"{nameof(DictionaryAssetDataAttribute<>)}`"));
            var attr = populatorType.GetCustomAttribute(customAttr.AttributeType) as DictionaryAssetDataAttributeBase;

            if (attr.OwnedAsset)
                assetName = $"$$MODID$$/{assetName}";

            if (!DictionaryData.TryGetValue(assetName, out var data))
                DictionaryData.Add(assetName, data = new());

            data.Add(populatorType);
        }
    }

    internal static StardewModdingAPI.Mod Mod { get; set; }
    public static void Init(StardewModdingAPI.Mod mod)
    {
        Mod = mod;

        mod.Helper.Events.Content.AssetRequested += Content_AssetRequested;
        mod.Helper.Events.Content.AssetsInvalidated += Content_AssetsInvalidated;

#if DEBUG
        mod.Helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
        HotReloadHandler.PendingHotReloads += Reload;
#endif
    }

#if DEBUG
    private static ConcurrentBag<string> toInvalidate = new();
    [EventPriority(EventPriority.High)]
    private static void GameLoop_UpdateTicking(object sender, UpdateTickingEventArgs e)
    {
        foreach (var entry in toInvalidate)
            Mod.Helper.GameContent.InvalidateCache(entry);
        toInvalidate.Clear();
    }

    internal static void Reload(Type type)
    {
        var customAttr = type.CustomAttributes.First(ca => ca.AttributeType.Name.StartsWith($"{nameof(DictionaryAssetDataAttribute<>)}`"));
        var attr = type.GetCustomAttribute(customAttr.AttributeType) as DictionaryAssetDataAttributeBase;

        string assetName = attr.OwnedAsset ? $"{Mod.ModManifest.UniqueID}/{attr.AssetName}" : attr.AssetName;

        toInvalidate.Add(assetName);
    }
#endif

    private static void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
    {
        foreach (var entry in CustomDictionaryAssets)
        {
            string assetName = $"{Mod.ModManifest.UniqueID}/{entry.Key}";

            if (!e.NameWithoutLocale.IsEquivalentTo(assetName))
                continue;

            var dictType = typeof(Dictionary<,>).MakeGenericType(typeof(string), entry.Value);
            var dict = dictType.GetConstructor([]).Invoke([]);

            e.LoadFrom(() => dict, AssetLoadPriority.Exclusive);
        }

        foreach (var entry in DictionaryData)
        {
            string assetName = entry.Key.Replace("$$MODID$$", Mod.ModManifest.UniqueID);

            if (!e.NameWithoutLocale.IsEquivalentTo(assetName))
                continue;
            if (!e.DataType.IsGenericType || e.DataType.GetGenericTypeDefinition() != typeof(Dictionary<,>))
                continue;

            Dictionary<string, object> valuesToAdd = new();
            foreach (var dataEntry in entry.Value)
            {
                var customAttr = dataEntry.CustomAttributes.First(ca => ca.AttributeType.Name.StartsWith($"{nameof(DictionaryAssetDataAttribute<>)}`"));
                var attr = dataEntry.GetCustomAttribute(customAttr.AttributeType) as DictionaryAssetDataAttributeBase;

                var data = dataEntry.GetConstructor([]).Invoke([]) as BaseDictionaryAssetData;
                data.ModId = Mod.ModManifest.UniqueID;

                foreach (var member in dataEntry.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    Type rawType = null;
                    object rawVal = null;
                    if (member is FieldInfo field)
                    {
                        rawType = field.FieldType;
                        rawVal = field.GetValue(data);
                    }
                    else if (member is PropertyInfo property)
                    {
                        rawType = property.PropertyType;
                        rawVal = property.GetValue(data);
                    }
                    if ( rawType == null)
                        continue;

                    Type type = rawType;
                    KeyValuePair<string, object>[] values;
                    if (rawType.IsArray)
                    {
                        type = rawType.GetElementType();

                        PropertyInfo valKey = type.GetProperty("Key");
                        PropertyInfo valVal = type.GetProperty("Value");

                        var arr = rawVal as IList;
                        values = new KeyValuePair<string, object>[arr.Count];
                        for (int i = 0; i < arr.Count; ++i)
                            values[i] = new(valKey.GetValue(arr[i]) as string, valVal.GetValue(arr[i]));
                    }
                    else
                    {
                        values = [new(member.Name.StartsWith('_') ? member.Name.Substring(1) : member.Name, rawVal)];
                    }

                    if (!type.IsAssignableTo(customAttr.AttributeType.GenericTypeArguments[0]))
                        continue;

                    var keyOverrideAttr = member.GetCustomAttribute<DictionaryAssetDataKeyAttribute>();

                    foreach (var valEntry in values)
                    {
                        string key = keyOverrideAttr?.Key ?? attr.KeyPattern;
                        if (!(keyOverrideAttr?.IgnoreSubstitutions ?? false))
                        {
                            for (int ic = 0; ic < key.Length; ++ic)
                            {
                                foreach (var subst in data.KeySubstitutions)
                                {
                                    if (key.Substring(ic, subst.Key.Length) != subst.Key)
                                        continue;

                                    var newVal = subst.Value(entry.Key, valEntry.Key);
                                    key = key.Remove(ic, subst.Key.Length).Insert(ic, newVal);
                                    ic += newVal.Length - 1;

                                    break;
                                }
                            }
                        }

                        valuesToAdd[key] = valEntry.Value;
                    }
                }
            }

            if (valuesToAdd.Count > 0)
            {
                e.Edit(ad =>
                {
                    var dict = ad.Data as IDictionary;
                    foreach ( var entry in valuesToAdd )
                        dict[entry.Key] = entry.Value;
                }, AssetEditPriority.Early);
            }
        }
    }

    private static void Content_AssetsInvalidated(object sender, AssetsInvalidatedEventArgs e)
    {
        var methBase = Mod.Helper.GameContent.GetType().GetMethod("Load", [ typeof( string ) ]);

        foreach (var entry in CustomDictionaryAssets)
        {
            string assetName = $"{Mod.ModManifest.UniqueID}/{entry.Key}";
            if (!e.NamesWithoutLocale.Any(an => an.IsEquivalentTo(assetName)))
                continue;

            //var methLoad = methBase.MakeGenericMethod(typeof(Dictionary<,>).MakeGenericType(typeof(string), entry.Value));
            //entry.Value.GetProperty("AssetInstance").SetValue(null, methLoad.Invoke(Mod.Helper.GameContent, [assetName]));
            entry.Value.GetMethod("RefreshData", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, [false]);
        }
    }
}
