// Decompiled with JetBrains decompiler
// Type: Paritee.StardewValley.Core.Utilities.Game
// Assembly: Paritee.StardewValley.Core, Version=2.0.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 5A2FE3D9-1A06-4344-9586-3DF16623F5C9
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\Paritee's Better Farm Animal Variety\Paritee.StardewValley.Core.dll

using System.Reflection;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Menus;
using xTile.Dimensions;
using Location = Paritee.StardewValley.Core.Locations.Location;

namespace Paritee.StardewValley.Core.Utilities
{
  public class Game
  {
    public static Multiplayer GetMultiplayer()
    {
      return Reflection.GetFieldValue<Multiplayer>(typeof(Game1), "multiplayer",
        BindingFlags.Static | BindingFlags.NonPublic);
    }

    public static long GetNewId()
    {
      return GetMultiplayer().getNewID();
    }

    public static Farmer GetMasterPlayer()
    {
      return Game1.MasterPlayer;
    }

    public static Farmer GetPlayer()
    {
      return Game1.player;
    }

    public static Farm GetFarm()
    {
      return Game1.getFarm();
    }

    public static Farmer GetFarmer(long farmerId)
    {
      return Game1.getFarmer(farmerId);
    }

    public static double GetDailyLuck()
    {
      return Game1.player.DailyLuck;
    }

    public static GameLocation GetCurrentLocation()
    {
      return Game1.currentLocation;
    }

    public static bool IsCurrentLocation(GameLocation location)
    {
      return Location.IsLocation(GetCurrentLocation(), location);
    }

    public static bool IsSaveLoaded()
    {
      return Game1.hasLoadedGame;
    }

    public static bool ActiveMenuExists()
    {
      return GetActiveMenu() == null;
    }

    public static IClickableMenu GetActiveMenu()
    {
      return Game1.activeClickableMenu;
    }

    public static void ExitActiveMenu()
    {
      Game1.exitActiveMenu();
    }

    public static Rectangle GetViewport()
    {
      return Game1.viewport;
    }

    public static T ReadSaveData<T>(string key)
    {
      string str;
      return Game1.CustomData.TryGetValue(key, out str) ? JsonConvert.DeserializeObject<T>(str) : default;
    }

    public static void WriteSaveData<T>(string key, T data)
    {
      if (data != null)
        Game1.CustomData[key] = JsonConvert.SerializeObject(data, 0);
      else
        Game1.CustomData.Remove(key);
    }

    public static int GetDaysPlayed()
    {
      return (int) Game1.stats.DaysPlayed;
    }

    public static int GetTimeOfDay(bool afterFade = false)
    {
      return afterFade ? Game1.timeOfDayAfterFade : Game1.timeOfDay;
    }

    public static bool IsEarlierThan(int time)
    {
      return GetTimeOfDay() < time;
    }

    public static bool IsLaterThan(int time)
    {
      return GetTimeOfDay() > time;
    }
  }
}