// Decompiled with JetBrains decompiler
// Type: Paritee.StardewValley.Core.Characters.Farmer
// Assembly: Paritee.StardewValley.Core, Version=2.0.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 5A2FE3D9-1A06-4344-9586-3DF16623F5C9
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\Paritee's Better Farm Animal Variety\Paritee.StardewValley.Core.dll

using System;
using System.Linq;
using Paritee.StardewValley.Core.Locations;
using Paritee.StardewValley.Core.Utilities;
using StardewValley.Buildings;
using SDV = StardewValley;

namespace Paritee.StardewValley.Core.Characters
{
  public class Farmer
  {
    public enum Profession
    {
      Rancher,
      Tiller,
      Butcher,
      Shepherd,
      Artisan,
      Agriculturist,
      Fisher,
      Trapper,
      Angler,
      Pirate,
      Baitmaster,
      Mariner,
      Forester,
      Gatherer,
      Lumberjack,
      Tapper,
      Botanist,
      Tracker,
      Miner,
      Geologist,
      Blacksmith,
      Burrower,
      Excavator,
      Gemologist,
      Fighter,
      Scout,
      Brute,
      Defender,
      Acrobat,
      Desperado
    }

    public static bool CanAfford(SDV.Farmer farmer, int amount, Currency.Type currency = Currency.Type.Money)
    {
      switch (currency)
      {
        case Currency.Type.Money:
          return farmer.Money >= amount;
        case Currency.Type.FestivalScore:
          return farmer.festivalScore >= amount;
        case Currency.Type.ClubCoins:
          return farmer.clubCoins >= amount;
        default:
          return false;
      }
    }

    public static void Spend(SDV.Farmer farmer, int amount, Currency.Type currency)
    {
      switch (currency)
      {
        case Currency.Type.Money:
          farmer.Money = Math.Max(0, farmer.Money - amount);
          break;
        case Currency.Type.FestivalScore:
          farmer.festivalScore = Math.Max(0, farmer.festivalScore - amount);
          break;
        case Currency.Type.ClubCoins:
          farmer.clubCoins = Math.Max(0, farmer.clubCoins - amount);
          break;
      }
    }

    public static void SpendMoney(SDV.Farmer farmer, int amount)
    {
      Spend(farmer, amount, Currency.Type.Money);
    }

    public static long GetUniqueId(SDV.Farmer farmer)
    {
      return farmer.UniqueMultiplayerID;
    }

    public static bool HasSeenEvent(SDV.Farmer farmer, int eventId)
    {
      return farmer.eventsSeen.Contains(eventId);
    }

    public static bool HasCompletedQuest(SDV.Farmer farmer, int questId)
    {
      return farmer.questLog.Where(o => o.id.Value.Equals(questId) && o.completed.Value)
        .Any();
    }

    public static int GetLuckLevel(SDV.Farmer farmer)
    {
      return farmer.LuckLevel;
    }

    public static double GetDailyLuck(SDV.Farmer farmer)
    {
      return Game.GetDailyLuck() + GetLuckLevel(farmer);
    }

    public static SDV.GameLocation GetCurrentLocation(SDV.Farmer farmer)
    {
      return farmer.currentLocation;
    }

    public static bool IsCurrentLocation(SDV.Farmer farmer, SDV.GameLocation location)
    {
      return Location.IsLocation(GetCurrentLocation(farmer), location);
    }

    public static SDV.FarmAnimal CreateFarmAnimal(
      SDV.Farmer farmer,
      string type,
      string name = null,
      Building home = null,
      long myId = 0)
    {
      return FarmAnimal.CreateFarmAnimal(type, GetUniqueId(farmer), name, home, myId);
    }

    public static bool HasProfession(SDV.Farmer farmer, Profession profession)
    {
      return farmer.professions.Contains((int) profession);
    }
  }
}