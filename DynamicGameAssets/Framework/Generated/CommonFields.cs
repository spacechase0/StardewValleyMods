using System.CodeDom.Compiler;
using System.Xml.Serialization;
using DynamicGameAssets.PackData;
using Netcode;

// ReSharper disable once CheckNamespace -- match partial classes
namespace DynamicGameAssets.Game
{
    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomBasicFurniture : IDGAItem
    {
        /// <summary>The backing field for <see cref="SourcePack"/>.</summary>
        public readonly NetString NetSourcePack = new();

        /// <summary>The backing field for <see cref="Id"/>.</summary>
        public readonly NetString NetId = new();

        /// <inheritdoc />
        [XmlIgnore]
        public string SourcePack => this.NetSourcePack.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string Id => this.NetId.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";

        /// <summary>The content pack data for the item.</summary>
        [XmlIgnore]
        public FurniturePackData Data => Mod.Find(this.FullId) as FurniturePackData ?? new FurniturePackData { pack = Mod.DummyContentPack };

        /// <summary>Construct an instance.</summary>
        public CustomBasicFurniture()
        {
            this.DoInit();
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="data">The content pack data for the item.</param>
        public CustomBasicFurniture(FurniturePackData data)
            : this()
        {
            this.NetSourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this.NetId.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(FurniturePackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomBedFurniture : IDGAItem
    {
        /// <summary>The backing field for <see cref="SourcePack"/>.</summary>
        public readonly NetString NetSourcePack = new();

        /// <summary>The backing field for <see cref="Id"/>.</summary>
        public readonly NetString NetId = new();

        /// <inheritdoc />
        [XmlIgnore]
        public string SourcePack => this.NetSourcePack.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string Id => this.NetId.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";

        /// <summary>The content pack data for the item.</summary>
        [XmlIgnore]
        public FurniturePackData Data => Mod.Find(this.FullId) as FurniturePackData ?? new FurniturePackData { pack = Mod.DummyContentPack };

        /// <summary>Construct an instance.</summary>
        public CustomBedFurniture()
        {
            this.DoInit();
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="data">The content pack data for the item.</param>
        public CustomBedFurniture(FurniturePackData data)
            : this()
        {
            this.NetSourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this.NetId.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(FurniturePackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomBigCraftable : IDGAItem
    {
        /// <summary>The backing field for <see cref="SourcePack"/>.</summary>
        public readonly NetString NetSourcePack = new();

        /// <summary>The backing field for <see cref="Id"/>.</summary>
        public readonly NetString NetId = new();

        /// <inheritdoc />
        [XmlIgnore]
        public string SourcePack => this.NetSourcePack.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string Id => this.NetId.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";

        /// <summary>The content pack data for the item.</summary>
        [XmlIgnore]
        public BigCraftablePackData Data => Mod.Find(this.FullId) as BigCraftablePackData ?? new BigCraftablePackData { pack = Mod.DummyContentPack };

        /// <summary>Construct an instance.</summary>
        public CustomBigCraftable()
        {
            this.DoInit();
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="data">The content pack data for the item.</param>
        public CustomBigCraftable(BigCraftablePackData data)
            : this()
        {
            this.NetSourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this.NetId.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(BigCraftablePackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomBoots : IDGAItem
    {
        /// <summary>The backing field for <see cref="SourcePack"/>.</summary>
        public readonly NetString NetSourcePack = new();

        /// <summary>The backing field for <see cref="Id"/>.</summary>
        public readonly NetString NetId = new();

        /// <inheritdoc />
        [XmlIgnore]
        public string SourcePack => this.NetSourcePack.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string Id => this.NetId.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";

        /// <summary>The content pack data for the item.</summary>
        [XmlIgnore]
        public BootsPackData Data => Mod.Find(this.FullId) as BootsPackData ?? new BootsPackData { pack = Mod.DummyContentPack };

        /// <summary>Construct an instance.</summary>
        public CustomBoots()
        {
            this.DoInit();
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="data">The content pack data for the item.</param>
        public CustomBoots(BootsPackData data)
            : this()
        {
            this.NetSourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this.NetId.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(BootsPackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomCraftingRecipe : IDGAItem
    {
        /// <summary>The backing field for <see cref="SourcePack"/>.</summary>
        public readonly NetString NetSourcePack = new();

        /// <summary>The backing field for <see cref="Id"/>.</summary>
        public readonly NetString NetId = new();

        /// <inheritdoc />
        [XmlIgnore]
        public string SourcePack => this.NetSourcePack.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string Id => this.NetId.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";

        /// <summary>The content pack data for the item.</summary>
        [XmlIgnore]
        public CraftingRecipePackData Data => Mod.Find(this.FullId) as CraftingRecipePackData ?? new CraftingRecipePackData { pack = Mod.DummyContentPack };

        /// <summary>Construct an instance.</summary>
        public CustomCraftingRecipe()
        {
            this.DoInit();
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="data">The content pack data for the item.</param>
        public CustomCraftingRecipe(CraftingRecipePackData data)
            : this()
        {
            this.NetSourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this.NetId.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(CraftingRecipePackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomCrop : IDGAItem
    {
        /// <summary>The backing field for <see cref="SourcePack"/>.</summary>
        public readonly NetString NetSourcePack = new();

        /// <summary>The backing field for <see cref="Id"/>.</summary>
        public readonly NetString NetId = new();

        /// <inheritdoc />
        [XmlIgnore]
        public string SourcePack => this.NetSourcePack.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string Id => this.NetId.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";

        /// <summary>The content pack data for the item.</summary>
        [XmlIgnore]
        public CropPackData Data => Mod.Find(this.FullId) as CropPackData ?? new CropPackData { pack = Mod.DummyContentPack };

        /// <summary>Construct an instance.</summary>
        public CustomCrop()
        {
            this.DoInit();
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="data">The content pack data for the item.</param>
        public CustomCrop(CropPackData data)
            : this()
        {
            this.NetSourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this.NetId.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(CropPackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomFence : IDGAItem
    {
        /// <summary>The backing field for <see cref="SourcePack"/>.</summary>
        public readonly NetString NetSourcePack = new();

        /// <summary>The backing field for <see cref="Id"/>.</summary>
        public readonly NetString NetId = new();

        /// <inheritdoc />
        [XmlIgnore]
        public string SourcePack => this.NetSourcePack.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string Id => this.NetId.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";

        /// <summary>The content pack data for the item.</summary>
        [XmlIgnore]
        public FencePackData Data => Mod.Find(this.FullId) as FencePackData ?? new FencePackData { pack = Mod.DummyContentPack };

        /// <summary>Construct an instance.</summary>
        public CustomFence()
        {
            this.DoInit();
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="data">The content pack data for the item.</param>
        public CustomFence(FencePackData data)
            : this()
        {
            this.NetSourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this.NetId.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(FencePackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomFishTankFurniture : IDGAItem
    {
        /// <summary>The backing field for <see cref="SourcePack"/>.</summary>
        public readonly NetString NetSourcePack = new();

        /// <summary>The backing field for <see cref="Id"/>.</summary>
        public readonly NetString NetId = new();

        /// <inheritdoc />
        [XmlIgnore]
        public string SourcePack => this.NetSourcePack.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string Id => this.NetId.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";

        /// <summary>The content pack data for the item.</summary>
        [XmlIgnore]
        public FurniturePackData Data => Mod.Find(this.FullId) as FurniturePackData ?? new FurniturePackData { pack = Mod.DummyContentPack };

        /// <summary>Construct an instance.</summary>
        public CustomFishTankFurniture()
        {
            this.DoInit();
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="data">The content pack data for the item.</param>
        public CustomFishTankFurniture(FurniturePackData data)
            : this()
        {
            this.NetSourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this.NetId.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(FurniturePackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomFruitTree : IDGAItem
    {
        /// <summary>The backing field for <see cref="SourcePack"/>.</summary>
        public readonly NetString NetSourcePack = new();

        /// <summary>The backing field for <see cref="Id"/>.</summary>
        public readonly NetString NetId = new();

        /// <inheritdoc />
        [XmlIgnore]
        public string SourcePack => this.NetSourcePack.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string Id => this.NetId.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";

        /// <summary>The content pack data for the item.</summary>
        [XmlIgnore]
        public FruitTreePackData Data => Mod.Find(this.FullId) as FruitTreePackData ?? new FruitTreePackData { pack = Mod.DummyContentPack };

        /// <summary>Construct an instance.</summary>
        public CustomFruitTree()
        {
            this.DoInit();
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="data">The content pack data for the item.</param>
        public CustomFruitTree(FruitTreePackData data)
            : this()
        {
            this.NetSourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this.NetId.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(FruitTreePackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomGiantCrop : IDGAItem
    {
        /// <summary>The backing field for <see cref="SourcePack"/>.</summary>
        public readonly NetString NetSourcePack = new();

        /// <summary>The backing field for <see cref="Id"/>.</summary>
        public readonly NetString NetId = new();

        /// <inheritdoc />
        [XmlIgnore]
        public string SourcePack => this.NetSourcePack.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string Id => this.NetId.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";

        /// <summary>The content pack data for the item.</summary>
        [XmlIgnore]
        public CropPackData Data => Mod.Find(this.FullId) as CropPackData ?? new CropPackData { pack = Mod.DummyContentPack };

        /// <summary>Construct an instance.</summary>
        public CustomGiantCrop()
        {
            this.DoInit();
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="data">The content pack data for the item.</param>
        public CustomGiantCrop(CropPackData data)
            : this()
        {
            this.NetSourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this.NetId.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(CropPackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomHat : IDGAItem
    {
        /// <summary>The backing field for <see cref="SourcePack"/>.</summary>
        public readonly NetString NetSourcePack = new();

        /// <summary>The backing field for <see cref="Id"/>.</summary>
        public readonly NetString NetId = new();

        /// <inheritdoc />
        [XmlIgnore]
        public string SourcePack => this.NetSourcePack.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string Id => this.NetId.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";

        /// <summary>The content pack data for the item.</summary>
        [XmlIgnore]
        public HatPackData Data => Mod.Find(this.FullId) as HatPackData ?? new HatPackData { pack = Mod.DummyContentPack };

        /// <summary>Construct an instance.</summary>
        public CustomHat()
        {
            this.DoInit();
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="data">The content pack data for the item.</param>
        public CustomHat(HatPackData data)
            : this()
        {
            this.NetSourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this.NetId.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(HatPackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomMeleeWeapon : IDGAItem
    {
        /// <summary>The backing field for <see cref="SourcePack"/>.</summary>
        public readonly NetString NetSourcePack = new();

        /// <summary>The backing field for <see cref="Id"/>.</summary>
        public readonly NetString NetId = new();

        /// <inheritdoc />
        [XmlIgnore]
        public string SourcePack => this.NetSourcePack.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string Id => this.NetId.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";

        /// <summary>The content pack data for the item.</summary>
        [XmlIgnore]
        public MeleeWeaponPackData Data => Mod.Find(this.FullId) as MeleeWeaponPackData ?? new MeleeWeaponPackData { pack = Mod.DummyContentPack };

        /// <summary>Construct an instance.</summary>
        public CustomMeleeWeapon()
        {
            this.DoInit();
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="data">The content pack data for the item.</param>
        public CustomMeleeWeapon(MeleeWeaponPackData data)
            : this()
        {
            this.NetSourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this.NetId.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(MeleeWeaponPackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomObject : IDGAItem
    {
        /// <summary>The backing field for <see cref="SourcePack"/>.</summary>
        public readonly NetString NetSourcePack = new();

        /// <summary>The backing field for <see cref="Id"/>.</summary>
        public readonly NetString NetId = new();

        /// <inheritdoc />
        [XmlIgnore]
        public string SourcePack => this.NetSourcePack.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string Id => this.NetId.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";

        /// <summary>The content pack data for the item.</summary>
        [XmlIgnore]
        public ObjectPackData Data => Mod.Find(this.FullId) as ObjectPackData ?? new ObjectPackData { pack = Mod.DummyContentPack };

        /// <summary>Construct an instance.</summary>
        public CustomObject()
        {
            this.DoInit();
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="data">The content pack data for the item.</param>
        public CustomObject(ObjectPackData data)
            : this()
        {
            this.NetSourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this.NetId.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(ObjectPackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomPants : IDGAItem
    {
        /// <summary>The backing field for <see cref="SourcePack"/>.</summary>
        public readonly NetString NetSourcePack = new();

        /// <summary>The backing field for <see cref="Id"/>.</summary>
        public readonly NetString NetId = new();

        /// <inheritdoc />
        [XmlIgnore]
        public string SourcePack => this.NetSourcePack.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string Id => this.NetId.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";

        /// <summary>The content pack data for the item.</summary>
        [XmlIgnore]
        public PantsPackData Data => Mod.Find(this.FullId) as PantsPackData ?? new PantsPackData { pack = Mod.DummyContentPack };

        /// <summary>Construct an instance.</summary>
        public CustomPants()
        {
            this.DoInit();
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="data">The content pack data for the item.</param>
        public CustomPants(PantsPackData data)
            : this()
        {
            this.NetSourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this.NetId.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(PantsPackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomShirt : IDGAItem
    {
        /// <summary>The backing field for <see cref="SourcePack"/>.</summary>
        public readonly NetString NetSourcePack = new();

        /// <summary>The backing field for <see cref="Id"/>.</summary>
        public readonly NetString NetId = new();

        /// <inheritdoc />
        [XmlIgnore]
        public string SourcePack => this.NetSourcePack.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string Id => this.NetId.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";

        /// <summary>The content pack data for the item.</summary>
        [XmlIgnore]
        public ShirtPackData Data => Mod.Find(this.FullId) as ShirtPackData ?? new ShirtPackData { pack = Mod.DummyContentPack };

        /// <summary>Construct an instance.</summary>
        public CustomShirt()
        {
            this.DoInit();
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="data">The content pack data for the item.</param>
        public CustomShirt(ShirtPackData data)
            : this()
        {
            this.NetSourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this.NetId.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(ShirtPackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomStorageFurniture : IDGAItem
    {
        /// <summary>The backing field for <see cref="SourcePack"/>.</summary>
        public readonly NetString NetSourcePack = new();

        /// <summary>The backing field for <see cref="Id"/>.</summary>
        public readonly NetString NetId = new();

        /// <inheritdoc />
        [XmlIgnore]
        public string SourcePack => this.NetSourcePack.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string Id => this.NetId.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";

        /// <summary>The content pack data for the item.</summary>
        [XmlIgnore]
        public FurniturePackData Data => Mod.Find(this.FullId) as FurniturePackData ?? new FurniturePackData { pack = Mod.DummyContentPack };

        /// <summary>Construct an instance.</summary>
        public CustomStorageFurniture()
        {
            this.DoInit();
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="data">The content pack data for the item.</param>
        public CustomStorageFurniture(FurniturePackData data)
            : this()
        {
            this.NetSourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this.NetId.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(FurniturePackData data);
    }

    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    public partial class CustomTVFurniture : IDGAItem
    {
        /// <summary>The backing field for <see cref="SourcePack"/>.</summary>
        public readonly NetString NetSourcePack = new();

        /// <summary>The backing field for <see cref="Id"/>.</summary>
        public readonly NetString NetId = new();

        /// <inheritdoc />
        [XmlIgnore]
        public string SourcePack => this.NetSourcePack.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string Id => this.NetId.Value;

        /// <inheritdoc />
        [XmlIgnore]
        public string FullId => $"{this.SourcePack}/{this.Id}";

        /// <summary>The content pack data for the item.</summary>
        [XmlIgnore]
        public FurniturePackData Data => Mod.Find(this.FullId) as FurniturePackData ?? new FurniturePackData { pack = Mod.DummyContentPack };

        /// <summary>Construct an instance.</summary>
        public CustomTVFurniture()
        {
            this.DoInit();
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="data">The content pack data for the item.</param>
        public CustomTVFurniture(FurniturePackData data)
            : this()
        {
            this.NetSourcePack.Value = data.pack.smapiPack.Manifest.UniqueID;
            this.NetId.Value = data.ID;
            this.DoInit(data);
        }

        partial void DoInit();

        partial void DoInit(FurniturePackData data);
    }

}
