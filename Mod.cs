using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using System.IO;
using JsonAssets.Data;
using StardewModdingAPI.Events;
using System.Reflection;

// TODO: Show seeds at stores

namespace JsonAssets
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            instance = this;

            Log.info("Checking content packs...");
            foreach (var dir in Directory.EnumerateDirectories(Path.Combine(Helper.DirectoryPath, "ContentPacks")))
            {
                if (!File.Exists(Path.Combine(dir, "content-pack.json")))
                    continue;
                var packInfo = Helper.ReadJsonFile<ContentPackData>(Path.Combine(dir, "content-pack.json"));
                Log.info($"\t{packInfo.Name} {packInfo.Version} by {packInfo.Author} - {packInfo.Description}");

                if (Directory.Exists(Path.Combine(dir, "Objects")))
                {
                    foreach (var objDir in Directory.EnumerateDirectories(Path.Combine(dir, "Objects")))
                    {
                        if (!File.Exists(Path.Combine(objDir, "object.json")))
                            continue;
                        var objInfo = Helper.ReadJsonFile<ObjectData>(Path.Combine(objDir, "object.json"));
                        objInfo.directory = Path.Combine("ContentPacks", Path.GetFileName(dir), "Objects", Path.GetFileName(objDir));
                        objects.Add(objInfo);
                    }
                }
                if (Directory.Exists(Path.Combine(dir, "Crops")))
                {
                    foreach (var cropDir in Directory.EnumerateDirectories(Path.Combine(dir, "Crops")))
                    {
                        if (!File.Exists(Path.Combine(cropDir, "crop.json")))
                            continue;
                        var cropInfo = Helper.ReadJsonFile<CropData>(Path.Combine(cropDir, "crop.json"));
                        cropInfo.directory = Path.Combine("ContentPacks", Path.GetFileName(dir), "Crops", Path.GetFileName(cropDir));
                        crops.Add(cropInfo);
                        var obj = new ObjectData();
                        obj.directory = cropInfo.directory;
                        obj.imageName = "seeds.png";
                        obj.Name = cropInfo.SeedName;
                        obj.Description = cropInfo.SeedDescription;
                        obj.Category = ObjectData.Category_.Seeds;
                        obj.Price = cropInfo.SeedPurchasePrice;
                        cropInfo.seed = obj;
                        objects.Add(obj);
                    }
                }
            }

            objectIds = AssignIds("objects", StartingObjectId, objects.ToList<DataNeedsId>());
            cropIds = AssignIds("crops", StartingCropId, crops.ToList<DataNeedsId>());
            
            var editors = ((IList<IAssetEditor>)helper.Content.GetType().GetProperty("AssetEditors", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).GetValue(Helper.Content));
            editors.Add(new ContentInjector());
        }

        private const int StartingObjectId = 2000;
        private const int StartingCropId = 100;
        internal IList<ObjectData> objects = new List<ObjectData>();
        internal IList<CropData> crops = new List<CropData>();
        private IDictionary<string, int> objectIds;
        private IDictionary<string, int> cropIds;

        public int ResolveObjectId( object data )
        {
            if (data.GetType() == typeof(long))
                return (int)(long)data;
            else
                return objectIds[ (string) data ];
        }

        private Dictionary<string, int> AssignIds( string type, int starting, IList<DataNeedsId> data )
        {
            var saved = Helper.ReadJsonFile<Dictionary<string, int>>(Path.Combine(Helper.DirectoryPath,$"ids-{type}.json"));
            Dictionary<string, int> ids = new Dictionary<string, int>();

            int currId = starting;
            // First, populate saved IDs
            foreach ( var d in data )
            {
                if (saved != null && saved.ContainsKey(d.Name))
                {
                    ids.Add(d.Name, saved[d.Name]);
                    currId = Math.Max(currId, saved[d.Name] + 1);
                    d.id = ids[d.Name];
                }
            }
            // Next, add in new IDs
            foreach (var d in data)
            {
                if (d.id == -1)
                {
                    ids.Add(d.Name, currId++);
                    if (type == "objects" && ((ObjectData)d).IsColored)
                        ++currId;
                    d.id = ids[d.Name];
                }
            }

            Helper.WriteJsonFile(Path.Combine(Helper.DirectoryPath, $"ids-{type}.json"), ids);
            return ids;
        }
    }
}
