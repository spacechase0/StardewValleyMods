using System;
using System.Collections.Generic;
using System.Reflection;
using DynamicGameAssets.PackData;
using SpaceShared;

namespace DynamicGameAssets.Framework.ContentPacks
{
    internal class ContentPackLoaderV1 : IContentPackLoader
    {
        private readonly ContentPack pack;

        public ContentPackLoaderV1(ContentPack thePack)
        {
            this.pack = thePack;
        }

        public void Load()
        {
            this.LoadAndValidateItems<ObjectPackData>("objects.json");
            this.LoadAndValidateItems<CraftingRecipePackData>("crafting-recipes.json");
            this.LoadAndValidateItems<FurniturePackData>("furniture.json");
            this.LoadAndValidateItems<CropPackData>("crops.json");
            this.LoadAndValidateItems<MeleeWeaponPackData>("melee-weapons.json");
            this.LoadAndValidateItems<BootsPackData>("boots.json");
            this.LoadAndValidateItems<HatPackData>("hats.json");
            this.LoadAndValidateItems<FencePackData>("fences.json");
            this.LoadAndValidateItems<BigCraftablePackData>("big-craftables.json");
            this.LoadAndValidateItems<FruitTreePackData>("fruit-trees.json");
            this.LoadAndValidateItems<ShirtPackData>("shirts.json");
            this.LoadAndValidateItems<PantsPackData>("pants.json");
            this.LoadOthers<ShopEntryPackData>("shop-entries.json");
            this.LoadOthers<ForgeRecipePackData>("forge-recipes.json");
            this.LoadOthers<MachineRecipePackData>("machine-recipes.json");
            this.LoadOthers<TailoringRecipePackData>("tailoring-recipes.json");
            this.LoadOthers<TextureOverridePackData>("texture-overrides.json");
            this.LoadOthers<GiftTastePackData>("gift-tastes.json");

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
                var data = this.pack.smapiPack.LoadAsset<List<ContentIndexPackData>>(json) ?? new List<ContentIndexPackData>();
                foreach (var d in data)
                {
                    Log.Trace("Loading data<" + typeof(ContentIndexPackData) + "> " + d.ContentType + " " + d.FilePath + "...");
                    this.pack.others.Add(d);
                    if (!this.pack.enableIndex.ContainsKey(parent))
                        this.pack.enableIndex.Add(parent, new());
                    this.pack.enableIndex[parent].Add(d);
                    d.pack = this.pack;
                    d.parent = parent;
                    d.original = (ContentIndexPackData)d.Clone();
                    d.original.original = d.original;
                    d.PostLoad();

                    var packDataType = Type.GetType("DynamicGameAssets.PackData." + d.ContentType + "PackData");
                    if (packDataType == null)
                    {
                        Log.Error("Invalid ContentType: " + d.ContentType);
                        continue;
                    }

                    MethodInfo baseMethod = null;
                    if (packDataType == typeof(ContentIndexPackData))
                        baseMethod = typeof(ContentPack).GetMethod(nameof(ContentPackLoaderV1.LoadIndex), BindingFlags.NonPublic | BindingFlags.Instance);
                    else if (packDataType.BaseType == typeof(CommonPackData))
                        baseMethod = typeof(ContentPack).GetMethod(nameof(ContentPackLoaderV1.LoadAndValidateItems), BindingFlags.NonPublic | BindingFlags.Instance);
                    else if (packDataType.BaseType == typeof(BasePackData))
                        baseMethod = typeof(ContentPack).GetMethod(nameof(ContentPackLoaderV1.LoadOthers), BindingFlags.NonPublic | BindingFlags.Instance);
                    else
                        throw new Exception("this should never happen");

                    MethodInfo genMethod = baseMethod.IsGenericMethod ? baseMethod.MakeGenericMethod(packDataType) : baseMethod;
                    genMethod.Invoke(this, new object[] { d.FilePath, d });
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception loading content index: \"" + json + "\": " + e);
            }
        }

        private void LoadAndValidateItems<T>(string json, ContentIndexPackData parent = null) where T : CommonPackData
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
                    ContentType = typeof(T).Name.Substring(0, typeof(T).Name.Length - "PackData".Length),
                    FilePath = json,
                };
                parent.original = (ContentIndexPackData)parent.Clone();
                parent.original.original = parent.original;
            }

            try
            {
                var data = this.pack.smapiPack.LoadAsset<List<T>>(json) ?? new List<T>();
                foreach (var d in data)
                {
                    if (this.pack.items.ContainsKey(d.ID))
                    {
                        Log.Error("Duplicate found! " + d.ID);
                        continue;
                    }
                    try
                    {
                        Log.Trace("Loading data<" + typeof(T) + ">: " + d.ID);
                        this.pack.items.Add(d.ID, d);
                        if (!this.pack.enableIndex.ContainsKey(parent))
                            this.pack.enableIndex.Add(parent, new());
                        this.pack.enableIndex[parent].Add(d);
                        Mod.itemLookup.Add($"{this.pack.smapiPack.Manifest.UniqueID}/{d.ID}".GetDeterministicHashCode(), $"{this.pack.smapiPack.Manifest.UniqueID}/{d.ID}");
                        /*if ( d is ShirtPackData )
                            Mod.itemLookup.Add( $"{smapiPack.Manifest.UniqueID}/{d.ID}".GetDeterministicHashCode() + 1, $"{smapiPack.Manifest.UniqueID}/{d.ID}" );
                        */
                        d.pack = this.pack;
                        d.parent = parent;
                        d.original = (T)d.Clone();
                        d.original.original = d.original;
                        d.PostLoad();
                    }
                    catch (Exception e)
                    {
                        Log.Error("Exception loading item \"" + d.ID + "\": " + e);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception loading data of type " + typeof(T) + ": " + e);
            }
        }

        private void LoadOthers<T>(string json, ContentIndexPackData parent = null) where T : BasePackData
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
                    ContentType = typeof(T).Name.Substring(0, typeof(T).Name.Length - "PackData".Length),
                    FilePath = json,
                };
                parent.original = (ContentIndexPackData)parent.Clone();
                parent.original.original = parent.original;
            }

            try
            {
                var data = this.pack.smapiPack.LoadAsset<List<T>>(json) ?? new List<T>();
                int i = 0;
                foreach (var d in data)
                {
                    Log.Trace("Loading data<" + typeof(T) + ">...");
                    try
                    {
                        this.pack.others.Add(d);
                        if (!this.pack.enableIndex.ContainsKey(parent))
                            this.pack.enableIndex.Add(parent, new());
                        this.pack.enableIndex[parent].Add(d);
                        d.pack = this.pack;
                        d.parent = parent;
                        d.original = (T)d.Clone();
                        d.original.original = d.original;
                        d.PostLoad();
                    }
                    catch (Exception e)
                    {
                        Log.Debug($"Exception loading item entry {i} from {json}: " + e);
                    }
                    ++i;
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception loading data of type " + typeof(T) + ": " + e);
            }
        }
    }
}
