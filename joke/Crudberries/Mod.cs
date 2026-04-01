using SpaceShared;
using SpaceShared.Attributes;
using SpaceShared.Content;
using StardewValley;
using StardewValley.GameData.Crops;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Shops;

namespace Crudberries;

[HasContent]
[HasTranslations]
public partial class Mod : BaseMod<Mod>
{
    protected override void ModEntry()
    {
        Helper.Events.Content.AssetRequested += AddGiftTastes;
    }

    private void AddGiftTastes(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo("Data/NPCGiftTastes"))
        {
            e.Edit(a =>
            {
                var data = a.AsDictionary<string, string>().Data;
                data["Universal_Dislike"] += $" {ModManifest.UniqueID}_CrudberrySeeds";
                data["Universal_Hate"] += $" crudberries";
            });
        }
        else if (e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
        {
            e.Edit(a =>
            {
                var data = a.AsDictionary<string, ShopData>().Data;
                data["SeedShop"].Items.Add(new()
                {
                    Id = $"{ModManifest.UniqueID}_CrudberrySeeds",
                    Condition = "YEAR 1 2",
                    ItemId = $"{ModManifest.UniqueID}_CrudberrySeeds",
                });
            });
        }
    }
}

[DictionaryAssetData<CropData>("Data/Crops", "$_&")]
internal partial class AddedCropData : BaseDictionaryAssetData
{
    private string Texture => Mod.Instance.Helper.ModContent.GetInternalAssetName("assets/crop.png").BaseName;

    public CropData CrudberrySeeds => new CropData()
    {
        Seasons = [ Season.Spring, Season.Summer, Season.Winter, Season.Fall ],
        DaysInPhase = [1, 2, 1, 1, 2],
        RegrowDays = 5,
        HarvestItemId = $"{ModId}_Crudberries",
        HarvestMinStack = 2,
        HarvestMaxStack = 1,
        ExtraHarvestChance = 0.1f,
        Texture = Texture,
        SpriteIndex = 0,
        CountForMonoculture = true,
        CountForPolyculture = false, // the seeds disappear after year 2
    };
}

[DictionaryAssetData<ObjectData>("Data/Objects", "$_&")]
internal partial class AddedObjectData : BaseDictionaryAssetData
{
    private string Texture => Mod.Instance.Helper.ModContent.GetInternalAssetName("assets/objects.png").BaseName;

    public ObjectData CrudberrySeeds => new ObjectData()
    {
        Name = "Crudberry Seeds",
        DisplayName = I18n.Object_CrudberrySeeds_Name(),
        Description = I18n.Object_CrudberrySeeds_Description(),
        Type = "Seeds",
        Category = -74,
        Price = 120,
        Texture = Texture,
        SpriteIndex = 0,
    };
    public ObjectData Crudberries => new ObjectData()
    {
        Name = "Crudberries",
        DisplayName = I18n.Object_Crudberries_Name(),
        Description = I18n.Object_Crudberries_Description(),
        Type = "Basic",
        Category = -79,
        Price = -75,
        Texture = Texture,
        SpriteIndex = 1,
        Edibility = -15,
        ContextTags = [ "color_brown", "dye_strong", "season_spring", "season_summer", "season_fall", "season_winter", "crudberries" ],
    };

    public ObjectData CrudberrySauce => new ObjectData()
    {
        Name = "Crudberry Sauce",
        DisplayName = I18n.Object_CrudberrySauce_Name(),
        Description = I18n.Object_CrudberrySauce_Description(),
        Type = "Cooking",
        Category = -7,
        Price = -120,
        Texture = Texture,
        SpriteIndex = 2,
        Edibility = -50,
        Buffs = [ new() { Id = "Food", Duration = 300, IsDebuff = true, CustomAttributes = new() { MiningLevel = -2 } } ],
        ContextTags = [ "color_brown", "dye_strong", "food_sauce", "crudberries", "food_crudberries"],
    };

    public ObjectData CrudberryCandy => new ObjectData()
    {
        Name = "Crudberry Candy",
        DisplayName = I18n.Object_CrudberryCandy_Name(),
        Description = I18n.Object_CrudberryCandy_Description(),
        Type = "Cooking",
        Category = -7,
        Price = -175,
        Texture = Texture,
        SpriteIndex = 3,
        Edibility = -50,
        IsDrink = true,
        ContextTags = [ "color_brown", "dye_strong", "food_sweet", "crudberries", "food_crudberries"],
    };
}

[DictionaryAssetData<string>("Data/CookingRecipes", "$_&")]
internal partial class AddedRecipeData : BaseDictionaryAssetData
{
    public string CrudberrySauce => $"{ModId}_Crudberries 1 245 1/9 3/{ModId}_CrudberrySauce/default/";
    public string CrudberryCandy => $"{ModId}_Crudberries 1 613 1 245 1/1 10/{ModId}_CrudberryCandy/default/";
}
