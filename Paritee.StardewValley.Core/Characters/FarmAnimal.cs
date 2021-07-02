// Decompiled with JetBrains decompiler
// Type: Paritee.StardewValley.Core.Characters.FarmAnimal
// Assembly: Paritee.StardewValley.Core, Version=2.0.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 5A2FE3D9-1A06-4344-9586-3DF16623F5C9
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\Paritee's Better Farm Animal Variety\Paritee.StardewValley.Core.dll

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Paritee.StardewValley.Core.Locations;
using Paritee.StardewValley.Core.Utilities;
using StardewValley.Buildings;
using Game = Paritee.StardewValley.Core.Utilities.Game;
using Object = Paritee.StardewValley.Core.Objects.Object;
using Random = Paritee.StardewValley.Core.Utilities.Random;
using SDV = StardewValley;

namespace Paritee.StardewValley.Core.Characters
{
  public class FarmAnimal
  {
    public enum DataValueIndex
    {
      DaysToLay,
      AgeWhenMature,
      DefaultProduce,
      DeluxeProduce,
      Sound,
      FrontBackBoundingBoxX,
      FrontBackBoundingBoxY,
      FrontBackBoundingBoxWidth,
      FrontBackBoundingBoxHeight,
      SidewaysBoundingBoxX,
      SidewaysBoundingBoxY,
      SidewaysBoundingBoxWidth,
      SidewaysBoundingBoxHeight,
      HarvestType,
      ShowDifferentTextureWhenReadyForHarvest,
      BuildingTypeILiveIn,
      SpriteWidth,
      SpritHeight,
      SidewaysSourceRectWidth,
      SidewaysSourceRectHeight,
      FullnessDrain,
      HappinessDrain,
      ToolUsedForHarvest,
      MeatIndex,
      Price,
      DisplayType,
      DisplayBuilding
    }

    public enum MoodMessage
    {
      NewHome,
      Happy,
      Fine,
      Sad,
      Hungry,
      DisturbedByDog,
      LeftOutsideAtNight
    }

    public const int LayHarvestType = 0;
    public const int GrabHarvestType = 1;
    public const int ButcherHarvestType = 2;
    public const int NoProduce = -1;
    public const int ShepherdProfessionDaysToLayBonus = -1;
    public const byte MinDaysToLay = 0;
    public const byte MaxDaysToLay = 255;
    public const byte MinDaysSinceLastLay = 0;
    public const byte MaxDaysSinceLastLay = 255;
    public const int DefaultHealth = 3;
    public const byte MinHappiness = 0;
    public const byte MaxHappiness = 255;
    public const byte MinFullness = 0;
    public const byte MaxFullness = 255;
    public const double BlueChickenChance = 0.25;
    public const int MinPauseTimer = 0;
    public const int MinHitGlowTimer = 0;
    public const string BabyPrefix = "Baby";
    public const string ReadyForHarvestPrefix = "Sheared";

    public static Livestock WhiteChicken => new Livestock("White Chicken", 0.0);

    public static Livestock BrownChicken => new Livestock("Brown Chicken", 0.0);

    public static Livestock BlueChicken => new Livestock("Blue Chicken", 0.0);

    public static Livestock VoidChicken => new Livestock("Void Chicken", 0.0);

    public static Livestock WhiteCow => new Livestock("White Cow", 0.0);

    public static Livestock BrownCow => new Livestock("Brown Cow", 0.0);

    public static Livestock Goat => new Livestock(nameof(Goat), 0.0);

    public static Livestock Duck => new Livestock(nameof(Duck), 0.01);

    public static Livestock Sheep => new Livestock(nameof(Sheep), 0.0);

    public static Livestock Rabbit => new Livestock(nameof(Rabbit), 0.02);

    public static Livestock Pig => new Livestock(nameof(Pig), 0.0);

    public static Livestock Dinosaur => new Livestock(nameof(Dinosaur), 0.0);

    public static Livestock Ostrich => new Livestock(nameof(Ostrich), 0.0);

    public static LivestockCategory DairyCowCategory => new LivestockCategory("Dairy Cow", 1, new List<Livestock>
      {
        WhiteCow,
        BrownCow
      }, new List<string>
      {
        AnimalHouse.FormatSize("Barn", AnimalHouse.Size.Small),
        AnimalHouse.FormatSize("Barn", AnimalHouse.Size.Big),
        AnimalHouse.FormatSize("Barn", AnimalHouse.Size.Deluxe)
      },
      new LivestockAnimalShop(1500, LivestockCategory.LoadDisplayName("5927"),
        LivestockCategory.LoadDescription("11343"),
        null));

    public static LivestockCategory ChickenCategory
    {
      get
      {
        var types = new List<Livestock>
        {
          WhiteChicken,
          BrownChicken,
          BlueChicken,
          VoidChicken
        };
        var buildings = new List<string>
        {
          AnimalHouse.FormatSize("Coop", AnimalHouse.Size.Small),
          AnimalHouse.FormatSize("Coop", AnimalHouse.Size.Big),
          AnimalHouse.FormatSize("Coop", AnimalHouse.Size.Deluxe)
        };
        var exclude = new List<Livestock>
        {
          VoidChicken
        };
        var animalShop = new LivestockAnimalShop(800, LivestockCategory.LoadDisplayName("5922"),
          LivestockCategory.LoadDescription("11334"), exclude);
        return new LivestockCategory("Chicken", 0, types, buildings, animalShop);
      }
    }

    public static LivestockCategory SheepCategory => new LivestockCategory("Sheep", 4, new List<Livestock>
      {
        Sheep
      }, new List<string>
      {
        AnimalHouse.FormatSize("Barn", AnimalHouse.Size.Deluxe)
      },
      new LivestockAnimalShop(8000, LivestockCategory.LoadDisplayName("5942"),
        LivestockCategory.LoadDescription("11352"),
        null));

    public static LivestockCategory GoatCategory => new LivestockCategory("Goat", 2, new List<Livestock>
      {
        Goat
      }, new List<string>
      {
        AnimalHouse.FormatSize("Barn", AnimalHouse.Size.Big),
        AnimalHouse.FormatSize("Barn", AnimalHouse.Size.Deluxe)
      },
      new LivestockAnimalShop(4000, LivestockCategory.LoadDisplayName("5933"),
        LivestockCategory.LoadDescription("11349"),
        null));

    public static LivestockCategory PigCategory => new LivestockCategory("Pig", 6, new List<Livestock>
      {
        Pig
      }, new List<string>
      {
        AnimalHouse.FormatSize("Barn", AnimalHouse.Size.Deluxe)
      },
      new LivestockAnimalShop(16000, LivestockCategory.LoadDisplayName("5948"),
        LivestockCategory.LoadDescription("11346"), null));

    public static LivestockCategory DuckCategory => new LivestockCategory("Duck", 3, new List<Livestock>
      {
        Duck
      }, new List<string>
      {
        AnimalHouse.FormatSize("Coop", AnimalHouse.Size.Big),
        AnimalHouse.FormatSize("Coop", AnimalHouse.Size.Deluxe)
      },
      new LivestockAnimalShop(4000, LivestockCategory.LoadDisplayName("5937"),
        LivestockCategory.LoadDescription("11337"),
        null));

    public static LivestockCategory RabbitCategory => new LivestockCategory("Rabbit", 5, new List<Livestock>
      {
        Rabbit
      }, new List<string>
      {
        AnimalHouse.FormatSize("Coop", AnimalHouse.Size.Deluxe)
      },
      new LivestockAnimalShop(8000, LivestockCategory.LoadDisplayName("5945"),
        LivestockCategory.LoadDescription("11340"),
        null));

    public static LivestockCategory DinosaurCategory => new LivestockCategory("Dinosaur", 7, new List<Livestock>
    {
      Dinosaur
    }, new List<string>
    {
      AnimalHouse.FormatSize("Coop", AnimalHouse.Size.Big),
      AnimalHouse.FormatSize("Coop", AnimalHouse.Size.Deluxe)
    }, null);

    public static LivestockCategory OstrichCategory => new LivestockCategory("Ostrich", 8, new List<Livestock>
    {
      Ostrich
    }, new List<string>
    {
      AnimalHouse.FormatSize("Barn", AnimalHouse.Size.Small),
      AnimalHouse.FormatSize("Barn", AnimalHouse.Size.Big),
      AnimalHouse.FormatSize("Barn", AnimalHouse.Size.Deluxe),
    }, null);

    public static string NonProducerTool => "-1";

    public static int MaxPathFindingPerTick => SDV.FarmAnimal.MaxPathfindingPerTick;

    public static bool IsBaby(SDV.FarmAnimal animal) => animal.isBaby();

    public static bool IsMale(SDV.FarmAnimal animal) => animal.isMale();

    public static void AssociateParent(SDV.FarmAnimal animal, SDV.FarmAnimal parent) =>
      AssociateParent(animal, GetUniqueId(parent));

    public static void AssociateParent(SDV.FarmAnimal animal, long parentId) => animal.parentId.Value = parentId;

    public static void UpdateFromData(SDV.FarmAnimal animal, string type)
    {
      var dictionary = Content.LoadData<string, string>(Content.DataFarmAnimalsContentPath);
      var keyValuePair = Content.GetDataEntry(dictionary, type);
      if (keyValuePair.Key == null)
      {
        var defaultType = GetDefaultType(animal);
        keyValuePair = dictionary.FirstOrDefault(kvp => kvp.Key.Equals(defaultType));
        if (keyValuePair.Key == null)
          throw new KeyNotFoundException("Could not find " + defaultType +
                                         " to overwrite custom farm animal for saving. This is a fatal error. Please make sure you have " +
                                         defaultType + " in the game.");
      }

      var flag1 = IsProduceAnItem(GetCurrentProduce(animal));
      var flag2 = IsCurrentlyProducingDeluxe(animal);
      var dataValue = Content.ParseDataValue(keyValuePair.Value);
      animal.type.Value = keyValuePair.Key;
      animal.daysToLay.Value = Convert.ToByte(dataValue[0]);
      animal.ageWhenMature.Value = Convert.ToByte(dataValue[1]);
      animal.defaultProduceIndex.Value = Convert.ToInt32(dataValue[2]);
      animal.deluxeProduceIndex.Value = Convert.ToInt32(dataValue[3]);
      var str1 = dataValue[4];
      animal.sound.Value = IsDataValueNull(str1) ? null : str1;
      var int32_1 = Convert.ToInt32(dataValue[5]);
      var int32_2 = Convert.ToInt32(dataValue[6]);
      var int32_3 = Convert.ToInt32(dataValue[7]);
      var int32_4 = Convert.ToInt32(dataValue[8]);
      animal.frontBackBoundingBox.Value = new Rectangle(int32_1, int32_2, int32_3, int32_4);
      var int32_5 = Convert.ToInt32(dataValue[9]);
      var int32_6 = Convert.ToInt32(dataValue[10]);
      var int32_7 = Convert.ToInt32(dataValue[11]);
      var int32_8 = Convert.ToInt32(dataValue[12]);
      animal.sidewaysBoundingBox.Value = new Rectangle(int32_5, int32_6, int32_7, int32_8);
      animal.harvestType.Value = Convert.ToByte(dataValue[13]);
      animal.showDifferentTextureWhenReadyForHarvest.Value = Convert.ToBoolean(dataValue[14]);
      animal.buildingTypeILiveIn.Value = dataValue[15];
      var int32_9 = Convert.ToInt32(dataValue[16]);
      var int32_10 = Convert.ToInt32(dataValue[17]);
      animal.Sprite = new SDV.AnimatedSprite(BuildSpriteAssetName(animal), 0, int32_9, int32_10);
      animal.frontBackSourceRect.Value = new Rectangle(0, 0, int32_9, int32_10);
      var int32_11 = Convert.ToInt32(dataValue[18]);
      var int32_12 = Convert.ToInt32(dataValue[19]);
      animal.sidewaysSourceRect.Value = new Rectangle(0, 0, int32_11, int32_12);
      animal.fullnessDrain.Value = Convert.ToByte(dataValue[20]);
      animal.happinessDrain.Value = Convert.ToByte(dataValue[21]);
      var str2 = dataValue[22];
      animal.toolUsedForHarvest.Value = IsDataValueNull(str2) ? "" : str2;
      animal.meatIndex.Value = Convert.ToInt32(dataValue[23]);
      animal.price.Value = Convert.ToInt32(dataValue[24]);
      var produceIndex =
        flag1 ? flag2 ? GetDeluxeProduce(animal) : GetDefaultProduce(animal) : -1;
      SetCurrentProduce(animal, produceIndex);
    }

    private static bool IsDataValueNull(string value)
    {
      int num;
      switch (value)
      {
        case "null":
        case "":
        case null:
          num = 1;
          break;
        default:
          num = value == "none" ? 1 : 0;
          break;
      }

      return num != 0;
    }

    public static void DecreaseFriendship(SDV.FarmAnimal animal, int decrease) =>
      IncreaseFriendship(animal, decrease * -1);

    public static void IncreaseFriendship(SDV.FarmAnimal animal, int increase) =>
      SetFriendship(animal, GetFriendship(animal) + increase);

    public static void SetFriendship(SDV.FarmAnimal animal, int newAmount) =>
      animal.friendshipTowardFarmer.Value = Math.Max(0, newAmount);

    public static int GetFriendship(SDV.FarmAnimal animal) => animal.friendshipTowardFarmer.Value;

    public static byte GetFullness(SDV.FarmAnimal animal) => animal.fullness.Value;

    public static void SetFullness(SDV.FarmAnimal animal, byte fullness) =>
      animal.fullness.Value = Math.Max((byte) 0, Math.Min(byte.MaxValue, fullness));

    public static void SetFindGrassPathController(SDV.FarmAnimal animal, SDV.GameLocation location) =>
      animal.controller =
        new SDV.PathFindController(animal, location,
          SDV.FarmAnimal.grassEndPointFunction, -1, false,
          SDV.FarmAnimal.behaviorAfterFindingGrassPatch, 200, Point.Zero);

    public static bool IsEating(SDV.FarmAnimal animal) => Reflection.GetFieldValue<NetBool>(animal, "isEating").Value;

    public static void StopEating(SDV.FarmAnimal animal) =>
      Reflection.GetField(animal, "isEating").SetValue(animal, new NetBool(false));

    public static void SetHealth(SDV.FarmAnimal animal, int health) => animal.health.Value = health;

    public static int GetHealth(SDV.FarmAnimal animal) => animal.health.Value;

    public static byte GetHappiness(SDV.FarmAnimal animal) => animal.happiness.Value;

    public static void SetHappiness(SDV.FarmAnimal animal, byte happiness) =>
      animal.happiness.Value = Math.Max((byte) 0, Math.Min(byte.MaxValue, happiness));

    public static byte GetHappinessDrain(SDV.FarmAnimal animal) => animal.happinessDrain.Value;

    public static void SetMoodMessage(SDV.FarmAnimal animal, MoodMessage moodMessage) =>
      Reflection.GetField(animal, nameof(moodMessage)).SetValue(animal, (int) moodMessage);

    public static bool WasPet(SDV.FarmAnimal animal) => animal.wasPet.Value;

    public static bool IsSheared(SDV.FarmAnimal animal) =>
      animal.showDifferentTextureWhenReadyForHarvest.Value && animal.currentProduce.Value <= 0;

    public static bool HasHarvestType(SDV.FarmAnimal animal, int harvestType) =>
      animal.harvestType.Value == harvestType;

    public static bool HasHarvestType(int harvestType, int target) => harvestType == target;

    public static void FindProduce(SDV.FarmAnimal animal, SDV.Farmer farmer) =>
      Reflection.GetMethod(animal, "findTruffle").Invoke(animal, new object[1]
      {
        farmer
      });

    public static bool CanFindProduce(SDV.FarmAnimal animal) =>
      CanFindProduce(animal.harvestType.Value, animal.toolUsedForHarvest.Value);

    public static bool CanFindProduce(int harvestType, string harvestTool) =>
      RequiresToolForHarvest(harvestType) && IsDataValueNull(harvestTool);

    public static bool CanBeNamed(SDV.FarmAnimal animal) => HasHarvestType(animal, 2);

    public static bool LaysProduce(SDV.FarmAnimal animal) => HasHarvestType(animal, 0);

    public static bool RequiresToolForHarvest(SDV.FarmAnimal animal) => HasHarvestType(animal, 1);

    public static bool RequiresToolForHarvest(int harvestType) => HasHarvestType(harvestType, 1);

    public static string GetToolUsedForHarvest(SDV.FarmAnimal animal) =>
      animal.toolUsedForHarvest.Value.Length > 0 ? animal.toolUsedForHarvest.Value : null;

    public static bool IsToolUsedForHarvest(SDV.FarmAnimal animal, string tool) =>
      IsToolUsedForHarvest(GetToolUsedForHarvest(animal), tool);

    public static bool IsToolUsedForHarvest(string tool, string target) => tool == target;

    public static Building GetHome(SDV.FarmAnimal animal) => animal.home;

    public static bool HasHome(SDV.FarmAnimal animal) => GetHome(animal) != null;

    public static bool IsInHome(SDV.FarmAnimal animal) =>
      HasHome(animal) && AnimalHouse.GetIndoors(GetHome(animal)).animals
        .ContainsKey(GetUniqueId(animal));

    public static void SetFindHomeDoorPathController(SDV.FarmAnimal animal, SDV.GameLocation location)
    {
      if (HasHome(animal))
        return;
      animal.controller = new SDV.PathFindController(animal, location,
        SDV.PathFindController.isAtEndPoint, 0, false,
        null, 200,
        new Point(animal.home.tileX.Value + animal.home.animalDoor.X,
          animal.home.tileY.Value + animal.home.animalDoor.Y));
    }

    public static string GetDisplayHouse(SDV.FarmAnimal animal) => animal.displayHouse;

    public static bool IsCoopDweller(SDV.FarmAnimal animal) =>
      IsCoopDweller(HasHome(animal)
        ? animal.home.buildingType.Value
        : animal.buildingTypeILiveIn.Value);

    public static bool IsCoopDweller(string buildingType) => buildingType != null && buildingType.Contains("Coop");

    public static bool CanLiveIn(SDV.FarmAnimal animal, Building building) =>
      building.buildingType.Value.Contains(animal.buildingTypeILiveIn.Value);

    public static void SetHome(SDV.FarmAnimal animal, Building home)
    {
      animal.home = home;
      animal.homeLocation.Value = home == null ? new Vector2() : new Vector2(home.tileX.Value, home.tileY.Value);
    }

    public static bool ReturnHome(SDV.FarmAnimal animal)
    {
      if (!HasHome(animal))
        return false;
      AnimalHouse.AddAnimal(animal.home, animal);
      SetRandomPositionInHome(animal);
      SetRandomFacingDirection(animal);
      animal.controller = null;
      return true;
    }

    public static bool SetRandomPositionInHome(SDV.FarmAnimal animal)
    {
      if (!HasHome(animal))
        return false;
      animal.setRandomPosition(animal.home.indoors.Value);
      return true;
    }

    public static void AddToBuilding(SDV.FarmAnimal animal, Building building)
    {
      SetHome(animal, building);
      SetRandomPositionInHome(animal);
      AnimalHouse.AddAnimal(building, animal);
    }

    public static void SetUniqueId(SDV.FarmAnimal animal, long id) => animal.myID.Value = id;

    public static long GetUniqueId(SDV.FarmAnimal animal) => animal.myID.Value;

    public static long GetOwnerId(SDV.FarmAnimal animal) => animal.ownerID.Value;

    public static void SetOwner(SDV.FarmAnimal animal, long id) => animal.ownerID.Value = id;

    public static string GetName(SDV.FarmAnimal animal) => animal.Name;

    public static bool HasName(SDV.FarmAnimal animal) => GetName(animal) != null;

    public static string SetRandomName(SDV.FarmAnimal animal)
    {
      var randomName = Dialogue.GetRandomName();
      SetName(animal, randomName);
      return randomName;
    }

    public static void SetName(SDV.FarmAnimal animal, string name)
    {
      animal.Name = name;
      animal.displayName = name;
    }

    public static int GetPrice(SDV.FarmAnimal animal) => animal.price.Value;


    public static int GetSellPrice(SDV.FarmAnimal animal) => animal.getSellPrice();


    public static int GetCheapestPrice(List<string> types)
    {
      var list = Content.LoadData<string, string>(Content.DataFarmAnimalsContentPath)
        .Where(kvp => types.Contains(kvp.Key)).Select(kvp => int.Parse(Content.ParseDataValue(kvp.Value)[24]))
        .ToList();
      list.Sort();
      return list.First();
    }

    public static int GetCurrentProduce(SDV.FarmAnimal animal) => animal.currentProduce.Value;

    public static bool IsCurrentlyProducing(SDV.FarmAnimal animal)
    {
      var currentProduce = GetCurrentProduce(animal);
      return IsProduceAnItem(currentProduce) && currentProduce > 0;
    }

    public static int GetDefaultProduce(SDV.FarmAnimal animal) => animal.defaultProduceIndex.Value;

    public static int GetDeluxeProduce(SDV.FarmAnimal animal) => animal.deluxeProduceIndex.Value;

    public static int GetProduceQuality(SDV.FarmAnimal animal) => animal.produceQuality.Value;

    public static bool HasProduceThatMatchesAll(SDV.FarmAnimal animal, int[] targets) =>
      HasProduceThatMatchesAll(GetDefaultProduce(animal),
        GetDeluxeProduce(animal), targets);

    public static bool HasProduceThatMatchesAll(
      int defaultProduceId,
      int deluxeProduceId,
      int[] targets)
    {
      var numArray = new int[2]
      {
        defaultProduceId,
        deluxeProduceId
      };
      return numArray.Intersect(targets).Count().Equals(numArray.Length);
    }

    public static bool HasProduceThatMatchesAtLeastOne(SDV.FarmAnimal animal, int[] targets) =>
      HasProduceThatMatchesAtLeastOne(GetDefaultProduce(animal),
        GetDeluxeProduce(animal), targets);

    public static bool HasProduceThatMatchesAtLeastOne(
      int defaultProduceId,
      int deluxeProduceId,
      int[] targets) =>
      targets.Where(o => IsProduceAnItem(o)).Intersect(new int[2]
      {
        defaultProduceId,
        deluxeProduceId
      }).Any();

    public static bool AreProduceItems(SDV.FarmAnimal animal) =>
      IsDefaultProduceAnItem(animal) && IsDeluxeProduceAnItem(animal);

    public static bool AreProduceItems(int defaultProduceIndex, int deluxeProduceIndex) =>
      IsProduceAnItem(defaultProduceIndex) && IsProduceAnItem(deluxeProduceIndex);

    public static bool IsAtLeastOneProduceAnItem(SDV.FarmAnimal animal) =>
      IsDefaultProduceAnItem(animal) || IsDeluxeProduceAnItem(animal);

    public static bool IsAtLeastOneProduceAnItem(int defaultProduceIndex, int deluxeProduceIndex) =>
      IsProduceAnItem(defaultProduceIndex) || IsProduceAnItem(deluxeProduceIndex);

    public static bool IsCurrentlyProducingDeluxe(SDV.FarmAnimal animal) =>
      GetCurrentProduce(animal) == GetDeluxeProduce(animal);

    public static bool IsDefaultProduceAnItem(SDV.FarmAnimal animal) =>
      IsProduceAnItem(animal.defaultProduceIndex.Value);

    public static bool IsDeluxeProduceAnItem(SDV.FarmAnimal animal) => IsProduceAnItem(animal.deluxeProduceIndex.Value);

    public static bool IsProduceAnItem(int produceIndex) => produceIndex != -1;

    public static bool IsAProducer(SDV.FarmAnimal animal) =>
      !RequiresToolForHarvest(animal) ||
      !IsToolUsedForHarvest(animal, NonProducerTool);

    public static void SetCurrentProduce(SDV.FarmAnimal animal, int produceIndex) =>
      animal.currentProduce.Value = produceIndex;

    public static void SetProduceQuality(SDV.FarmAnimal animal, Object.Quality quality) =>
      animal.produceQuality.Value = (int) quality;

    public static Object.Quality RollProduceQuality(
      SDV.FarmAnimal animal,
      SDV.Farmer farmer,
      int seed)
    {
      var num = GetFriendship(animal) / 1000.0 - (1.0 - GetHappiness(animal) / 225.0);
      var flag1 = IsCoopDweller(animal);
      var flag2 = Farmer.HasProfession(farmer, Farmer.Profession.Shepherd);
      var flag3 = Farmer.HasProfession(farmer, Farmer.Profession.Butcher);
      if (!flag1 & flag2 || flag1 & flag3)
        num += 0.33;
      var random = new System.Random(seed);
      if (num >= 0.95 && random.NextDouble() < num / 2.0)
        return Object.Quality.Best;
      if (random.NextDouble() < num / 2.0)
        return Object.Quality.High;
      return random.NextDouble() < num ? Object.Quality.Medium : Object.Quality.Low;
    }

    public static int RollProduce(
      SDV.FarmAnimal animal,
      int seed,
      SDV.Farmer farmer = null,
      double deluxeProduceLuck = 0.0)
    {
      var luck = farmer == null ? 0.0 : Farmer.GetDailyLuck(farmer) * deluxeProduceLuck;
      return RollDeluxeProduceChance(animal, luck, seed)
        ? GetDeluxeProduce(animal)
        : GetDefaultProduce(animal);
    }

    public static bool RollDeluxeProduceChance(SDV.FarmAnimal animal, double luck, int seed)
    {
      if (IsBaby(animal) || !IsProduceAnItem(GetDeluxeProduce(animal)))
        return false;
      var random = new System.Random(seed);
      var happiness = GetHappiness(animal);
      if (random.NextDouble() >= happiness / 150.0)
        return false;
      var num = 0.0;
      var friendship = GetFriendship(animal);
      if (happiness > 200)
        num = happiness * 1.5;
      else if (happiness <= 100)
        num = happiness - 100;
      if (luck != 0.0)
        return Random.NextDouble() < (friendship + num) / 5000.0 + luck;
      return friendship >= 200 && Random.NextDouble() < (friendship + num) / 1200.0;
    }

    public static void SetPauseTimer(SDV.FarmAnimal animal, int timer) => animal.pauseTimer = Math.Max(0, timer);

    public static int GetPauseTimer(SDV.FarmAnimal animal) => animal.pauseTimer;

    public static void SetHitGlowTimer(SDV.FarmAnimal animal, int timer) => animal.hitGlowTimer = Math.Max(0, timer);

    public static int GetHitGlowTimer(SDV.FarmAnimal animal) => animal.hitGlowTimer;

    public static bool HasPathController(SDV.FarmAnimal animal) => animal.controller != null;

    public static void ResetPathController(SDV.FarmAnimal animal) => animal.controller = null;

    public static bool MakesSound(SDV.FarmAnimal animal) => GetSound(animal) != null;

    public static string GetSound(SDV.FarmAnimal animal) => animal.sound.Value;

    public static void ReloadSpriteTexture(SDV.FarmAnimal animal) =>
      animal.Sprite.LoadTexture(BuildSpriteAssetName(animal));

    public static string BuildSpriteAssetName(SDV.FarmAnimal animal)
    {
      var isBaby = IsBaby(animal);
      var isSheared = !isBaby && IsSheared(animal);
      string assetName;
      if (!TryBuildSpriteAssetName(GetType(animal), isBaby, isSheared, out assetName))
        assetName = BuildSpriteAssetName(GetDefaultType(IsCoopDweller(animal)),
          isBaby, isSheared);
      return assetName;
    }

    public static string BuildSpriteAssetName(string type, bool isBaby = false, bool isSheared = false)
    {
      var str = "";
      if (isBaby)
        str = "Baby";
      else if (isSheared)
        str = "Sheared";
      return Content.BuildPath(new string[2]
      {
        "Animals",
        str + type
      });
    }

    public static bool TryBuildSpriteAssetName(
      string type,
      bool isBaby,
      bool isSheared,
      out string assetName)
    {
      assetName = BuildSpriteAssetName(type, isBaby, isSheared);
      return Content.Exists<Texture2D>(assetName);
    }

    public static SDV.AnimatedSprite CreateSprite(SDV.FarmAnimal animal) =>
      new SDV.AnimatedSprite(BuildSpriteAssetName(animal), 0, animal.frontBackSourceRect.Width,
        animal.frontBackSourceRect.Height);

    public static void SetRandomFacingDirection(SDV.FarmAnimal animal) => animal.faceDirection(Random.Next(4));

    public static void AnimateFindingProduce(SDV.FarmAnimal animal)
    {
      int frame1;
      int frame2;
      switch (animal.FacingDirection)
      {
        case 0:
          frame1 = 9;
          frame2 = 11;
          break;
        case 1:
          frame1 = 5;
          frame2 = 7;
          break;
        case 2:
          frame1 = 1;
          frame2 = 2;
          break;
        default:
          frame1 = 5;
          frame2 = 7;
          break;
      }

      var frameBehavior = (SDV.AnimatedSprite.endOfAnimationBehavior) Delegate.CreateDelegate(
        typeof(SDV.AnimatedSprite.endOfAnimationBehavior), animal, Reflection.GetMethod(animal, "findTruffle"));
      var animation = new List<SDV.FarmerSprite.AnimationFrame>
      {
        new SDV.FarmerSprite.AnimationFrame(frame1, 250),
        new SDV.FarmerSprite.AnimationFrame(frame2, 250),
        new SDV.FarmerSprite.AnimationFrame(frame1, 250),
        new SDV.FarmerSprite.AnimationFrame(frame2, 250),
        new SDV.FarmerSprite.AnimationFrame(frame1, 250),
        new SDV.FarmerSprite.AnimationFrame(frame2, 250, false, false, frameBehavior)
      };
      animal.Sprite.setCurrentAnimation(animation);
      animal.Sprite.loop = false;
    }

    public static int GetFacingDirection(SDV.FarmAnimal animal) => animal.FacingDirection;

    public static Vector2 GetTileLocation(SDV.FarmAnimal animal) => animal.getTileLocation();

    public static bool HasController(SDV.FarmAnimal animal) => animal.controller != null;

    public static Rectangle GetBoundingBox(SDV.FarmAnimal animal) => animal.GetBoundingBox();

    public static byte GetDaysToLay(SDV.FarmAnimal animal, SDV.Farmer farmer = null)
    {
      var num = animal.daysToLay.Value;
      if (farmer == null)
        return num;
      var flag1 = IsType(animal, Sheep);
      var flag2 = Farmer.HasProfession(farmer, Farmer.Profession.Shepherd);
      return (byte) Math.Min(byte.MaxValue, Math.Max(0, num + (flag1 & flag2 ? -1 : 0)));
    }

    public static byte GetDaysSinceLastLay(SDV.FarmAnimal animal) => animal.daysSinceLastLay.Value;

    public static void SetDaysSinceLastLay(SDV.FarmAnimal animal, byte days) =>
      animal.daysSinceLastLay.Value = Math.Max((byte) 0, Math.Min(byte.MaxValue, days));

    public static int GetMeatIndex(SDV.FarmAnimal animal) => animal.meatIndex.Value;

    public static SDV.FarmAnimal CreateFarmAnimal(
      string type,
      long ownerId,
      string name = null,
      Building home = null,
      long myId = 0)
    {
      if (myId == 0L)
        myId = Game.GetNewId();
      var farmAnimal = new SDV.FarmAnimal(type, myId, ownerId)
      {
        Name = name,
        displayName = name,
        home = home
      };
      var animal = farmAnimal;
      UpdateFromData(animal, type);
      return animal;
    }

    public static void Reload(SDV.FarmAnimal animal, Building home) => animal.reload(home);

    public static void ReloadAll()
    {
      for (var index1 = 0; index1 < SDV.Game1.locations.Count; ++index1)
        if (SDV.Game1.locations[index1] is SDV.Farm location)
        {
          for (var index2 = 0; index2 < location.buildings.Count; ++index2)
            if (location.buildings[index2].indoors.Value is SDV.AnimalHouse animalHouse)
              for (var index3 = 0; index3 < animalHouse.animalsThatLiveHere.Count(); ++index3)
              {
                var key = animalHouse.animalsThatLiveHere.ElementAt(index3);
                if (animalHouse.animals.ContainsKey(key))
                {
                  var animal = animalHouse.animals[key];
                  Reload(animal, animal.home);
                }
              }

          break;
        }
    }

    public static void IncreasePathFindingThisTick(int amount = 1) => SDV.FarmAnimal.NumPathfindingThisTick += amount;

    public static bool UnderMaxPathFindingPerTick() => SDV.FarmAnimal.NumPathfindingThisTick < MaxPathFindingPerTick;

    public static void SetType(SDV.FarmAnimal animal, string type) => animal.type.Value = type;

    public static string GetType(SDV.FarmAnimal animal) => animal.type.Value;

    public static string GetDisplayType(SDV.FarmAnimal animal) => animal.displayType;

    public static bool IsType(SDV.FarmAnimal animal, Livestock type) => IsType(animal, type.ToString());

    public static bool IsType(SDV.FarmAnimal animal, string type) => IsType(animal.type.Value, type);

    public static bool IsType(string source, string type) => source == type;

    public static string GetDefaultType(string buildingType) => GetDefaultType(buildingType == "Coop");

    public static string GetDefaultType(SDV.FarmAnimal animal) => GetDefaultType(IsCoopDweller(animal));

    public static string GetDefaultType(bool isCoop) =>
      isCoop ? GetDefaultCoopDwellerType() : GetDefaultBarnDwellerType();

    public static string GetDefaultCoopDwellerType() => WhiteChicken.ToString();

    public static string GetDefaultBarnDwellerType() => WhiteCow.ToString();

    public static List<string> GetTypesFromProduce(
      int[] produceIndexes,
      Dictionary<string, List<string>> restrictions)
    {
      var stringList1 = new List<string>();
      var stringList2 = new List<string>();
      var dictionary = Content.LoadData<string, string>(Content.DataFarmAnimalsContentPath);
      foreach (var restriction in restrictions)
      foreach (var key in restriction.Value)
      {
        var dataValue = Content.ParseDataValue(dictionary[key]);
        if (HasProduceThatMatchesAtLeastOne(int.Parse(dataValue[2]), int.Parse(dataValue[3]),
          produceIndexes))
        {
          stringList2.Add(key);
          stringList1.Add(restriction.Key);
        }
      }

      return stringList2;
    }

    public static string GetRandomTypeFromProduce(
      SDV.FarmAnimal animal,
      Dictionary<string, List<string>> restrictions) =>
      GetRandomTypeFromProduce(new int[2]
      {
        GetDefaultProduce(animal),
        GetDeluxeProduce(animal)
      }, restrictions);

    public static string GetRandomTypeFromProduce(
      int[] produceIndexes,
      Dictionary<string, List<string>> restrictions)
    {
      var typesFromProduce = GetTypesFromProduce(produceIndexes, restrictions);
      var index = Random.Next(typesFromProduce.Count);
      return typesFromProduce.Any() ? typesFromProduce[index] : null;
    }

    public static string GetRandomTypeFromProduce(
      int produceIndex,
      Dictionary<string, List<string>> restrictions) =>
      GetRandomTypeFromProduce(new int[1]
      {
        produceIndex
      }, restrictions);

    public static bool BlueChickenIsUnlocked(SDV.Farmer farmer) => Farmer.HasSeenEvent(farmer, 3900074);

    public static bool RollBlueChickenChance(SDV.Farmer farmer) =>
      BlueChickenIsUnlocked(farmer) && Random.NextDouble() >= 0.25;

    public static List<string> SanitizeAffordableTypes(List<string> types, SDV.Farmer farmer) =>
      Content.LoadData<string, string>(Content.DataFarmAnimalsContentPath).Where
        (kvp =>
          types.Contains(kvp.Key) &&
          Farmer.CanAfford(farmer, int.Parse(Content.ParseDataValue(kvp.Value)[24])))
        .ToDictionary(
          kvp => kvp.Key,
          kvp => kvp.Value).Keys.ToList();

    public static List<string> SanitizeBlueChickens(List<string> types, SDV.Farmer farmer)
    {
      var str = BlueChicken.ToString();
      if (types.Contains(str) && !AnimalShop.IsBlueChickenAvailableForPurchase(farmer))
        types.Remove(str);
      return types;
    }

    public static List<LivestockCategory> GetVanillaCategories() =>
      new List<LivestockCategory>
      {
        ChickenCategory,
        DairyCowCategory,
        DinosaurCategory,
        DuckCategory,
        GoatCategory,
        PigCategory,
        RabbitCategory,
        SheepCategory,
        OstrichCategory
      }.OrderBy(o => o.Order).ToList();

    public static List<Livestock> GetVanillaTypes() =>
      new List<Livestock>
      {
        WhiteChicken,
        BrownChicken,
        BlueChicken,
        VoidChicken,
        WhiteCow,
        BrownCow,
        Goat,
        Duck,
        Sheep,
        Rabbit,
        Pig,
        Dinosaur,
        Ostrich
      };

    public static bool IsVanilla(SDV.FarmAnimal animal) => IsVanillaType(GetType(animal));

    public static bool IsVanillaType(string type) => GetVanillaTypes().Select(o => o.ToString()).Contains(type);

    public static bool IsVanillaCategory(string category) =>
      GetVanillaCategories().Select(o => o.ToString())
        .Contains(category);
  }
}