using System.CodeDom.Compiler;
using System.Xml.Serialization;
using DynamicGameAssets.PackData;
using Netcode;

namespace DynamicGameAssets.Game
{
    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomBasicFurniture : IDGAItem
    {
        public readonly NetString _sourcePack = new();
        public readonly NetString _id = new();
        [XmlIgnore]
        public string SourcePack => this._sourcePack.Value;
        [XmlIgnore]
        public string Id => this._id.Value;
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";
        [XmlIgnore]
        public FurniturePackData Data => Mod.Find(this.FullId) as FurniturePackData ?? new FurniturePackData() { pack = Mod.DummyContentPack };

        public CustomBasicFurniture()
        {
            this.DoInit();
        }

        public CustomBasicFurniture(FurniturePackData data)
            : this()
        {
            this._sourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this._id.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(FurniturePackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomBedFurniture : IDGAItem
    {
        public readonly NetString _sourcePack = new();
        public readonly NetString _id = new();
        [XmlIgnore]
        public string SourcePack => this._sourcePack.Value;
        [XmlIgnore]
        public string Id => this._id.Value;
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";
        [XmlIgnore]
        public FurniturePackData Data => Mod.Find(this.FullId) as FurniturePackData ?? new FurniturePackData() { pack = Mod.DummyContentPack };

        public CustomBedFurniture()
        {
            this.DoInit();
        }

        public CustomBedFurniture(FurniturePackData data)
            : this()
        {
            this._sourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this._id.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(FurniturePackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomBigCraftable : IDGAItem
    {
        public readonly NetString _sourcePack = new();
        public readonly NetString _id = new();
        [XmlIgnore]
        public string SourcePack => this._sourcePack.Value;
        [XmlIgnore]
        public string Id => this._id.Value;
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";
        [XmlIgnore]
        public BigCraftablePackData Data => Mod.Find(this.FullId) as BigCraftablePackData ?? new BigCraftablePackData() { pack = Mod.DummyContentPack };

        public CustomBigCraftable()
        {
            this.DoInit();
        }

        public CustomBigCraftable(BigCraftablePackData data)
            : this()
        {
            this._sourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this._id.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(BigCraftablePackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomBoots : IDGAItem
    {
        public readonly NetString _sourcePack = new();
        public readonly NetString _id = new();
        [XmlIgnore]
        public string SourcePack => this._sourcePack.Value;
        [XmlIgnore]
        public string Id => this._id.Value;
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";
        [XmlIgnore]
        public BootsPackData Data => Mod.Find(this.FullId) as BootsPackData ?? new BootsPackData() { pack = Mod.DummyContentPack };

        public CustomBoots()
        {
            this.DoInit();
        }

        public CustomBoots(BootsPackData data)
            : this()
        {
            this._sourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this._id.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(BootsPackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomCraftingRecipe : IDGAItem
    {
        public readonly NetString _sourcePack = new();
        public readonly NetString _id = new();
        [XmlIgnore]
        public string SourcePack => this._sourcePack.Value;
        [XmlIgnore]
        public string Id => this._id.Value;
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";
        [XmlIgnore]
        public CraftingRecipePackData Data => Mod.Find(this.FullId) as CraftingRecipePackData ?? new CraftingRecipePackData() { pack = Mod.DummyContentPack };

        public CustomCraftingRecipe()
        {
            this.DoInit();
        }

        public CustomCraftingRecipe(CraftingRecipePackData data)
            : this()
        {
            this._sourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this._id.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(CraftingRecipePackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomCrop : IDGAItem
    {
        public readonly NetString _sourcePack = new();
        public readonly NetString _id = new();
        [XmlIgnore]
        public string SourcePack => this._sourcePack.Value;
        [XmlIgnore]
        public string Id => this._id.Value;
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";
        [XmlIgnore]
        public CropPackData Data => Mod.Find(this.FullId) as CropPackData ?? new CropPackData() { pack = Mod.DummyContentPack };

        public CustomCrop()
        {
            this.DoInit();
        }

        public CustomCrop(CropPackData data)
            : this()
        {
            this._sourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this._id.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(CropPackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomFence : IDGAItem
    {
        public readonly NetString _sourcePack = new();
        public readonly NetString _id = new();
        [XmlIgnore]
        public string SourcePack => this._sourcePack.Value;
        [XmlIgnore]
        public string Id => this._id.Value;
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";
        [XmlIgnore]
        public FencePackData Data => Mod.Find(this.FullId) as FencePackData ?? new FencePackData() { pack = Mod.DummyContentPack };

        public CustomFence()
        {
            this.DoInit();
        }

        public CustomFence(FencePackData data)
            : this()
        {
            this._sourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this._id.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(FencePackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomFishTankFurniture : IDGAItem
    {
        public readonly NetString _sourcePack = new();
        public readonly NetString _id = new();
        [XmlIgnore]
        public string SourcePack => this._sourcePack.Value;
        [XmlIgnore]
        public string Id => this._id.Value;
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";
        [XmlIgnore]
        public FurniturePackData Data => Mod.Find(this.FullId) as FurniturePackData ?? new FurniturePackData() { pack = Mod.DummyContentPack };

        public CustomFishTankFurniture()
        {
            this.DoInit();
        }

        public CustomFishTankFurniture(FurniturePackData data)
            : this()
        {
            this._sourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this._id.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(FurniturePackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomFruitTree : IDGAItem
    {
        public readonly NetString _sourcePack = new();
        public readonly NetString _id = new();
        [XmlIgnore]
        public string SourcePack => this._sourcePack.Value;
        [XmlIgnore]
        public string Id => this._id.Value;
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";
        [XmlIgnore]
        public FruitTreePackData Data => Mod.Find(this.FullId) as FruitTreePackData ?? new FruitTreePackData() { pack = Mod.DummyContentPack };

        public CustomFruitTree()
        {
            this.DoInit();
        }

        public CustomFruitTree(FruitTreePackData data)
            : this()
        {
            this._sourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this._id.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(FruitTreePackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomGiantCrop : IDGAItem
    {
        public readonly NetString _sourcePack = new();
        public readonly NetString _id = new();
        [XmlIgnore]
        public string SourcePack => this._sourcePack.Value;
        [XmlIgnore]
        public string Id => this._id.Value;
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";
        [XmlIgnore]
        public CropPackData Data => Mod.Find(this.FullId) as CropPackData ?? new CropPackData() { pack = Mod.DummyContentPack };

        public CustomGiantCrop()
        {
            this.DoInit();
        }

        public CustomGiantCrop(CropPackData data)
            : this()
        {
            this._sourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this._id.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(CropPackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomHat : IDGAItem
    {
        public readonly NetString _sourcePack = new();
        public readonly NetString _id = new();
        [XmlIgnore]
        public string SourcePack => this._sourcePack.Value;
        [XmlIgnore]
        public string Id => this._id.Value;
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";
        [XmlIgnore]
        public HatPackData Data => Mod.Find(this.FullId) as HatPackData ?? new HatPackData() { pack = Mod.DummyContentPack };

        public CustomHat()
        {
            this.DoInit();
        }

        public CustomHat(HatPackData data)
            : this()
        {
            this._sourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this._id.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(HatPackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomMeleeWeapon : IDGAItem
    {
        public readonly NetString _sourcePack = new();
        public readonly NetString _id = new();
        [XmlIgnore]
        public string SourcePack => this._sourcePack.Value;
        [XmlIgnore]
        public string Id => this._id.Value;
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";
        [XmlIgnore]
        public MeleeWeaponPackData Data => Mod.Find(this.FullId) as MeleeWeaponPackData ?? new MeleeWeaponPackData() { pack = Mod.DummyContentPack };

        public CustomMeleeWeapon()
        {
            this.DoInit();
        }

        public CustomMeleeWeapon(MeleeWeaponPackData data)
            : this()
        {
            this._sourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this._id.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(MeleeWeaponPackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomObject : IDGAItem
    {
        public readonly NetString _sourcePack = new();
        public readonly NetString _id = new();
        [XmlIgnore]
        public string SourcePack => this._sourcePack.Value;
        [XmlIgnore]
        public string Id => this._id.Value;
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";
        [XmlIgnore]
        public ObjectPackData Data => Mod.Find(this.FullId) as ObjectPackData ?? new ObjectPackData() { pack = Mod.DummyContentPack };

        public CustomObject()
        {
            this.DoInit();
        }

        public CustomObject(ObjectPackData data)
            : this()
        {
            this._sourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this._id.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(ObjectPackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomPants : IDGAItem
    {
        public readonly NetString _sourcePack = new();
        public readonly NetString _id = new();
        [XmlIgnore]
        public string SourcePack => this._sourcePack.Value;
        [XmlIgnore]
        public string Id => this._id.Value;
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";
        [XmlIgnore]
        public PantsPackData Data => Mod.Find(this.FullId) as PantsPackData ?? new PantsPackData() { pack = Mod.DummyContentPack };

        public CustomPants()
        {
            this.DoInit();
        }

        public CustomPants(PantsPackData data)
            : this()
        {
            this._sourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this._id.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(PantsPackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomShirt : IDGAItem
    {
        public readonly NetString _sourcePack = new();
        public readonly NetString _id = new();
        [XmlIgnore]
        public string SourcePack => this._sourcePack.Value;
        [XmlIgnore]
        public string Id => this._id.Value;
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";
        [XmlIgnore]
        public ShirtPackData Data => Mod.Find(this.FullId) as ShirtPackData ?? new ShirtPackData() { pack = Mod.DummyContentPack };

        public CustomShirt()
        {
            this.DoInit();
        }

        public CustomShirt(ShirtPackData data)
            : this()
        {
            this._sourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this._id.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(ShirtPackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomStorageFurniture : IDGAItem
    {
        public readonly NetString _sourcePack = new();
        public readonly NetString _id = new();
        [XmlIgnore]
        public string SourcePack => this._sourcePack.Value;
        [XmlIgnore]
        public string Id => this._id.Value;
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";
        [XmlIgnore]
        public FurniturePackData Data => Mod.Find(this.FullId) as FurniturePackData ?? new FurniturePackData() { pack = Mod.DummyContentPack };

        public CustomStorageFurniture()
        {
            this.DoInit();
        }

        public CustomStorageFurniture(FurniturePackData data)
            : this()
        {
            this._sourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this._id.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(FurniturePackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomTVFurniture : IDGAItem
    {
        public readonly NetString _sourcePack = new();
        public readonly NetString _id = new();
        [XmlIgnore]
        public string SourcePack => this._sourcePack.Value;
        [XmlIgnore]
        public string Id => this._id.Value;
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";
        [XmlIgnore]
        public FurniturePackData Data => Mod.Find(this.FullId) as FurniturePackData ?? new FurniturePackData() { pack = Mod.DummyContentPack };

        public CustomTVFurniture()
        {
            this.DoInit();
        }

        public CustomTVFurniture(FurniturePackData data)
            : this()
        {
            this._sourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this._id.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(FurniturePackData data);
    }

}
