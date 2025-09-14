using System;
using System.Collections.Generic;
using System.IO;
using DynamicGameAssets.PackData;
using HarmonyLib;
using Newtonsoft.Json;
using SpaceShared;

namespace DynamicGameAssets.Framework.ContentPacks
{
    internal class ContentPackLoaderV2 : IContentPackLoader
    {
        private readonly ContentPack pack;

        public ContentPackLoaderV2(ContentPack thePack)
        {
            this.pack = thePack;
        }

        public void Load()
        {
            /*
            List<BasePackData> data = new();
            data.Add( new ObjectPackData() );
            data.Add( new BigCraftablePackData() );
            var conv = new BasePackDataListConverter();
            //Log.Debug( JsonConvert.SerializeObject( data, Formatting.Indented, new BasePackDataListConverter() ) );
            */
            this.LoadIndex("content.json");
        }

        private void LoadIndex(string json, ContentIndexPackData parent = null)
        {
            if (!this.pack.smapiPack.HasFile(json))
            {
                if (parent != null)
                    Log.Warn("Missing json file: " + json);
                return;
            }
            if (parent == null)
            {
                parent = new ContentIndexPackData()
                {
                    pack = this.pack,
                    parent = null,
                    ContentType = "ContentIndex",
                    FilePath = json,
                };
                parent.original = (ContentIndexPackData)parent.Clone();
                parent.original.original = parent.original;
            }

            try
            {
                var colorConverter = (JsonConverter)AccessTools.Constructor(AccessTools.TypeByName("StardewModdingAPI.Framework.Serialization.ColorConverter")).Invoke(Array.Empty<object>());
                var vec2Converter = (JsonConverter)AccessTools.Constructor(AccessTools.TypeByName("StardewModdingAPI.Framework.Serialization.Vector2Converter")).Invoke(Array.Empty<object>());
                var rectConverter = (JsonConverter)AccessTools.Constructor(AccessTools.TypeByName("StardewModdingAPI.Framework.Serialization.RectangleConverter")).Invoke(Array.Empty<object>());
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    Converters = new[] { new BasePackDataListConverter(), colorConverter, vec2Converter, rectConverter }
                };
                var data = JsonConvert.DeserializeObject<List<BasePackData>>(File.ReadAllText(Path.Combine(this.pack.smapiPack.DirectoryPath, json)), settings);
                foreach (var d in data)
                {
                    if (d is CommonPackData cd && this.pack.items.ContainsKey(cd.ID))
                    {
                        Log.Error("Duplicate found! " + cd.ID);
                        continue;
                    }
                    Log.Trace("Loading data< " + d.GetType() + " >...");

                    if (!this.pack.enableIndex.ContainsKey(parent))
                        this.pack.enableIndex.Add(parent, new());
                    this.pack.enableIndex[parent].Add(d);
                    d.pack = this.pack;
                    d.parent = parent;
                    d.original = (BasePackData)d.Clone();
                    d.original.original = d.original;

                    if (d is CommonPackData cdata)
                    {
                        this.pack.items.Add(cdata.ID, cdata);
                        Mod.itemLookup.Add($"{this.pack.smapiPack.Manifest.UniqueID}/{cdata.ID}".GetDeterministicHashCode(), $"{this.pack.smapiPack.Manifest.UniqueID}/{cdata.ID}");
                    }
                    else
                    {
                        this.pack.others.Add(d);
                    }
                    d.PostLoad();

                    if (d is ContentIndexPackData cidata)
                    {
                        this.LoadIndex(cidata.FilePath, cidata);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception loading content index: \"" + json + "\": " + e);
            }
        }
    }
}
