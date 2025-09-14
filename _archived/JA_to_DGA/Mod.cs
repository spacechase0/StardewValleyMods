using System.Collections.Generic;
using System.IO;
using System.Reflection;
using JA_to_DGA.Framework;
using JsonAssets.Data;
using Newtonsoft.Json;
using SpaceShared;
using StardewModdingAPI;

namespace JA_to_DGA
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            Mod.instance = this;
            Log.Monitor = this.Monitor;

            helper.ConsoleCommands.Add("dga_convert", "dga_convert <JA_mod_ID> <new_DGA_mod_id>", this.OnConvertCommand);
        }

        private void OnConvertCommand(string cmd, string[] args)
        {
            if (args.Length != 2 && !(args.Length == 1 && args[0] == "all"))
            {
                Log.Error("Bad usage.");
                return;
            }

            if (args[0] == "all")
            {
                foreach (var cp in JsonAssets.Mod.instance.Helper.ContentPacks.GetOwned())
                    this.Convert(cp, cp.Manifest.UniqueID + ".DGA");
            }
            else
            {
                var mod = this.Helper.ModRegistry.Get(args[0]);
                if (mod == null)
                {
                    Log.Error("No such mod");
                    return;
                }

                IContentPack cp = (IContentPack)mod.GetType().GetProperty("ContentPack", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(mod);
                if (cp == null)
                {
                    Log.Error("Not a content pack");
                    return;
                }

                this.Convert(cp, args[1]);
            }
        }

        public void Convert(IContentPack cp, string newModId)
        {
            string jaPath = (string)cp.GetType().GetProperty("DirectoryPath", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(cp);
            string dgaPath = Path.Combine(Path.GetDirectoryName(this.Helper.DirectoryPath), "[DGA] " + newModId);
            Log.Info("Path: " + jaPath + " -> " + dgaPath);

            if (Directory.Exists(dgaPath))
            {
                Log.Error("Already converted!");
                return;
            }

            Directory.CreateDirectory(dgaPath);

            var i18n = new Dictionary<string, Dictionary<string, string>>();
            var objs = new List<DynamicGameAssets.PackData.ObjectPackData>();
            var crops = new List<DynamicGameAssets.PackData.CropPackData>();
            var fruitTrees = new List<DynamicGameAssets.PackData.FruitTreePackData>();
            var bigs = new List<DynamicGameAssets.PackData.BigCraftablePackData>();
            var hats = new List<DynamicGameAssets.PackData.HatPackData>();
            var weapons = new List<DynamicGameAssets.PackData.MeleeWeaponPackData>();
            var shirts = new List<DynamicGameAssets.PackData.ShirtPackData>();
            var pants = new List<DynamicGameAssets.PackData.PantsPackData>();
            var tailoring = new List<DynamicGameAssets.PackData.TailoringRecipePackData>();
            var boots = new List<DynamicGameAssets.PackData.BootsPackData>();
            var fences = new List<DynamicGameAssets.PackData.FencePackData>();
            var forges = new List<DynamicGameAssets.PackData.ForgeRecipePackData>();
            var crafting = new List<DynamicGameAssets.PackData.CraftingRecipePackData>();
            var shops = new List<DynamicGameAssets.PackData.ShopEntryPackData>();
            var giftTastes = new List<DynamicGameAssets.PackData.GiftTastePackData>();

            Directory.CreateDirectory(Path.Combine(dgaPath, "assets"));

            Directory.CreateDirectory(Path.Combine(dgaPath, "assets", "objects"));
            if (Directory.Exists(Path.Combine(jaPath, "Objects")))
            {
                foreach (string dir_ in Directory.GetDirectories(Path.Combine(jaPath, "Objects")))
                {
                    string dir = Path.GetFileName(dir_);
                    if (!File.Exists(Path.Combine(jaPath, "Objects", dir, "object.json")))
                        continue;
                    Log.Trace("Converting object " + dir + "...");
                    var data = cp.ReadJsonFile<ObjectData>(Path.Combine("Objects", dir, "object.json"));
                    var packData = data.ConvertObject(newModId, i18n, objs, crafting, shops, giftTastes);
                    File.Copy(Path.Combine(jaPath, "Objects", dir, "object.png"), Path.Combine(dgaPath, "assets", "objects", packData.ID + ".png"));
                    if (File.Exists(Path.Combine(jaPath, "Objects", dir, "color.png")))
                        File.Copy(Path.Combine(jaPath, "Objects", dir, "color.png"), Path.Combine(dgaPath, "assets", "objects", packData.ID + "_color.png"));
                }

                // pass 2 - crafting recipes
                // this is necessary because an object could use a later object in its recipe
                foreach (string dir_ in Directory.GetDirectories(Path.Combine(jaPath, "Objects")))
                {
                    string dir = Path.GetFileName(dir_);
                    if (!File.Exists(Path.Combine(jaPath, "Objects", dir, "object.json")))
                        continue;
                    Log.Trace("Converting object crafting recipe " + dir + " (if it exists)...");
                    var data = cp.ReadJsonFile<ObjectData>(Path.Combine("Objects", dir, "object.json"));
                    var packData = data.ConvertCrafting(newModId, i18n, objs, crafting, shops);
                }
            }

            if (Directory.Exists(Path.Combine(jaPath, "Crops")))
            {
                Directory.CreateDirectory(Path.Combine(dgaPath, "assets", "crops"));
                foreach (string dir_ in Directory.GetDirectories(Path.Combine(jaPath, "Crops")))
                {
                    string dir = Path.GetFileName(dir_);
                    if (!File.Exists(Path.Combine(jaPath, "Crops", dir, "crop.json")))
                        continue;
                    Log.Trace("Converting crop " + dir + "...");
                    var data = cp.ReadJsonFile<CropData>(Path.Combine("Crops", dir, "crop.json"));
                    var packData = data.ConvertCrop(newModId, i18n, crops, objs, shops);
                    File.Copy(Path.Combine(jaPath, "Crops", dir, "crop.png"), Path.Combine(dgaPath, "assets", "crops", packData.ID + ".png"));
                    if (File.Exists(Path.Combine(jaPath, "Crops", dir, "giant.png")))
                    {
                        packData.GiantTextureChoices = new string[] { Path.Combine("assets", "crops", packData.ID + "_giant.png") };
                        packData.GiantDrops.Add((DynamicGameAssets.PackData.CropPackData.HarvestedDropData)packData.Phases[packData.Phases.Count - 1].HarvestedDrops[0].Clone());
                        File.Copy(Path.Combine(jaPath, "Crops", dir, "giant.png"), Path.Combine(dgaPath, "assets", "crops", packData.ID + "_giant.png"));
                    }
                    if (File.Exists(Path.Combine(jaPath, "Crops", dir, "seeds.png")))
                        File.Copy(Path.Combine(jaPath, "Crops", dir, "seeds.png"), Path.Combine(dgaPath, "assets", "objects", packData.ID + "_seeds.png"));
                }
            }

            if (Directory.Exists(Path.Combine(jaPath, "FruitTrees")))
            {
                Directory.CreateDirectory(Path.Combine(dgaPath, "assets", "fruit-trees"));
                foreach (string dir_ in Directory.GetDirectories(Path.Combine(jaPath, "FruitTrees")))
                {
                    string dir = Path.GetFileName(dir_);
                    if (!File.Exists(Path.Combine(jaPath, "FruitTrees", dir, "tree.json")))
                        continue;
                    Log.Trace("Converting fruit tree " + dir + "...");
                    var data = cp.ReadJsonFile<FruitTreeData>(Path.Combine("FruitTrees", dir, "tree.json"));
                    var packData = data.ConvertFruitTree(newModId, i18n, fruitTrees, objs, shops);
                    File.Copy(Path.Combine(jaPath, "FruitTrees", dir, "tree.png"), Path.Combine(dgaPath, "assets", "fruit-trees", packData.ID + ".png"));
                    if (File.Exists(Path.Combine(jaPath, "FruitTrees", dir, "sapling.png")))
                        File.Copy(Path.Combine(jaPath, "FruitTrees", dir, "sapling.png"), Path.Combine(dgaPath, "assets", "objects", packData.ID + "_sapling.png"));
                }
            }

            if (Directory.Exists(Path.Combine(jaPath, "BigCraftables")))
            {
                Directory.CreateDirectory(Path.Combine(dgaPath, "assets", "big-craftables"));
                foreach (string dir_ in Directory.GetDirectories(Path.Combine(jaPath, "BigCraftables")))
                {
                    string dir = Path.GetFileName(dir_);
                    if (!File.Exists(Path.Combine(jaPath, "BigCraftables", dir, "big-craftable.json")))
                        continue;
                    Log.Trace("Converting big craftable " + dir + "...");
                    var data = cp.ReadJsonFile<BigCraftableData>(Path.Combine("BigCraftables", dir, "big-craftable.json"));
                    if (data.ReserveNextIndex && data.ReserveExtraIndexCount == 0)
                        data.ReserveExtraIndexCount = 1;
                    var packData = data.ConvertBigCraftable(newModId, i18n, bigs, objs, crafting, shops);
                    File.Copy(Path.Combine(jaPath, "BigCraftables", dir, "big-craftable.png"), Path.Combine(dgaPath, "assets", "big-craftables", packData.ID + "0.png"));
                    for (int i = 0; i < data.ReserveExtraIndexCount; ++i)
                        File.Copy(Path.Combine(jaPath, "BigCraftables", dir, $"big-craftable-{i + 2}.png"), Path.Combine(dgaPath, "assets", "big-craftables", packData.ID + (i + 1) + ".png"));
                }
            }

            if (Directory.Exists(Path.Combine(jaPath, "Hats")))
            {
                Directory.CreateDirectory(Path.Combine(dgaPath, "assets", "hats"));
                foreach (string dir_ in Directory.GetDirectories(Path.Combine(jaPath, "Hats")))
                {
                    string dir = Path.GetFileName(dir_);
                    if (!File.Exists(Path.Combine(jaPath, "Hats", dir, "hat.json")))
                        continue;
                    Log.Trace("Converting hat " + dir + "...");
                    var data = cp.ReadJsonFile<HatData>(Path.Combine("Hats", dir, "hat.json"));
                    var packData = data.ConvertHat(newModId, i18n, hats, shops);
                    File.Copy(Path.Combine(jaPath, "Hats", dir, "hat.png"), Path.Combine(dgaPath, "assets", "hats", packData.ID + ".png"));
                }
            }

            if (Directory.Exists(Path.Combine(jaPath, "Weapons")))
            {
                Directory.CreateDirectory(Path.Combine(dgaPath, "assets", "melee-weapons"));
                foreach (string dir_ in Directory.GetDirectories(Path.Combine(jaPath, "Weapons")))
                {
                    string dir = Path.GetFileName(dir_);
                    if (!File.Exists(Path.Combine(jaPath, "Weapons", dir, "weapon.json")))
                        continue;
                    Log.Trace("Converting melee weapon " + dir + "...");
                    var data = cp.ReadJsonFile<WeaponData>(Path.Combine("Weapons", dir, "weapon.json"));
                    var packData = data.ConvertMeleeWeapon(newModId, i18n, weapons, shops);
                    File.Copy(Path.Combine(jaPath, "Weapons", dir, "weapon.png"), Path.Combine(dgaPath, "assets", "melee-weapons", packData.ID + ".png"));
                }
            }

            if (Directory.Exists(Path.Combine(jaPath, "Shirts")))
            {
                Directory.CreateDirectory(Path.Combine(dgaPath, "assets", "shirts"));
                foreach (string dir_ in Directory.GetDirectories(Path.Combine(jaPath, "Shirts")))
                {
                    string dir = Path.GetFileName(dir_);
                    if (!File.Exists(Path.Combine(jaPath, "Shirts", dir, "shirt.json")))
                        continue;
                    Log.Trace("Converting shirt " + dir + "...");
                    var data = cp.ReadJsonFile<WeaponData>(Path.Combine("Shirts", dir, "shirt.json"));
                    var packData = data.ConvertShirt(newModId, i18n, shirts);
                    File.Copy(Path.Combine(jaPath, "Shirts", dir, "male.png"), Path.Combine(dgaPath, "assets", "shirts", packData.ID + "_male.png"));
                    if (File.Exists(Path.Combine(jaPath, "Shirts", dir, "female.png")))
                        File.Copy(Path.Combine(jaPath, "Shirts", dir, "female.png"), Path.Combine(dgaPath, "assets", "shirts", packData.ID + "_female.png"));
                    if (File.Exists(Path.Combine(jaPath, "Shirts", dir, "male-color.png")))
                        File.Copy(Path.Combine(jaPath, "Shirts", dir, "male-color.png"), Path.Combine(dgaPath, "assets", "shirts", packData.ID + "_male_color.png"));
                    if (File.Exists(Path.Combine(jaPath, "Shirts", dir, "female-color.png")))
                        File.Copy(Path.Combine(jaPath, "Shirts", dir, "female-color.png"), Path.Combine(dgaPath, "assets", "shirts", packData.ID + "_female_color.png"));
                }
            }

            var serializeSettings = new JsonSerializerSettings()
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Formatting = Formatting.Indented,
            };

            if (objs.Count > 0) File.WriteAllText(Path.Combine(dgaPath, "objects.json"), JsonConvert.SerializeObject(objs, serializeSettings));
            if (crops.Count > 0) File.WriteAllText(Path.Combine(dgaPath, "crops.json"), JsonConvert.SerializeObject(crops, serializeSettings));
            if (fruitTrees.Count > 0) File.WriteAllText(Path.Combine(dgaPath, "fruit-trees.json"), JsonConvert.SerializeObject(fruitTrees, serializeSettings));
            if (bigs.Count > 0) File.WriteAllText(Path.Combine(dgaPath, "big-craftables.json"), JsonConvert.SerializeObject(bigs, serializeSettings));
            if (hats.Count > 0) File.WriteAllText(Path.Combine(dgaPath, "hats.json"), JsonConvert.SerializeObject(hats, serializeSettings));
            if (weapons.Count > 0) File.WriteAllText(Path.Combine(dgaPath, "melee-weapons.json"), JsonConvert.SerializeObject(weapons, serializeSettings));
            if (shirts.Count > 0) File.WriteAllText(Path.Combine(dgaPath, "shirts.json"), JsonConvert.SerializeObject(shirts, serializeSettings));
            if (pants.Count > 0) File.WriteAllText(Path.Combine(dgaPath, "pants.json"), JsonConvert.SerializeObject(pants, serializeSettings));
            if (tailoring.Count > 0) File.WriteAllText(Path.Combine(dgaPath, "tailoring-recipes.json"), JsonConvert.SerializeObject(tailoring, serializeSettings));
            if (boots.Count > 0) File.WriteAllText(Path.Combine(dgaPath, "boots.json"), JsonConvert.SerializeObject(boots, serializeSettings));
            if (fences.Count > 0) File.WriteAllText(Path.Combine(dgaPath, "fences.json"), JsonConvert.SerializeObject(fences, serializeSettings));
            if (forges.Count > 0) File.WriteAllText(Path.Combine(dgaPath, "forge-recipes.json"), JsonConvert.SerializeObject(forges, serializeSettings));
            if (crafting.Count > 0) File.WriteAllText(Path.Combine(dgaPath, "crafting-recipes.json"), JsonConvert.SerializeObject(crafting, serializeSettings));
            if (shops.Count > 0) File.WriteAllText(Path.Combine(dgaPath, "shop-entries.json"), JsonConvert.SerializeObject(shops, serializeSettings));
            if (giftTastes.Count > 0) File.WriteAllText(Path.Combine(dgaPath, "gift-tastes.json"), JsonConvert.SerializeObject(giftTastes, serializeSettings));

            Directory.CreateDirectory(Path.Combine(dgaPath, "i18n"));
            foreach (var entry in i18n)
            {
                File.WriteAllText(Path.Combine(dgaPath, "i18n", entry.Key + ".json"), JsonConvert.SerializeObject(entry.Value, serializeSettings));
            }

            var manifest = new Manifest();
            manifest.Name = cp.Manifest.Name + " (DGA version)";
            manifest.Description = cp.Manifest.Description;
            manifest.Author = cp.Manifest.Author;
            manifest.Version = cp.Manifest.Version;
            manifest.MinimumApiVersion = cp.Manifest.MinimumApiVersion;
            manifest.UniqueID = cp.Manifest.UniqueID + ".DGA";
            manifest.ContentPackFor = new ManifestContentPackFor()
            {
                UniqueID = "spacechase0.DynamicGameAssets"
            };
            manifest.Dependencies = cp.Manifest.Dependencies;
            manifest.UpdateKeys = cp.Manifest.UpdateKeys;
            manifest.ExtraFields = cp.Manifest.ExtraFields ?? new Dictionary<string, object>();
            manifest.ExtraFields.Add("DGA.FormatVersion", 1);
            manifest.ExtraFields.Add("DGA.ConditionsFormatVersion", "1.23.0");
            File.WriteAllText(Path.Combine(dgaPath, "manifest.json"), JsonConvert.SerializeObject(manifest, serializeSettings));

            var dga = this.Helper.ModRegistry.GetApi<IDynamicGameAssetsApi>("spacechase0.DynamicGameAssets");
            dga.AddEmbeddedPack(manifest, dgaPath);

            Log.Info("Done!");
            Log.Info("We did some black magic to go ahead and load it without restarting the game, too. :)");
            Log.Info("Please do not upload converted packs for mods that you don't have permission to do!");
            Log.Info("NOTE: Regrowing crops work differently in DGA than in JA! See the making content packs documentation for detail.");
            Log.Info("NOTE: Crafting recipes are incomplete if they used ingredients from a different pack! They need prefixing with \"pack.id/\". (See documentation for details.)");
        }
    }
}
