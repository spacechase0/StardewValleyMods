// Decompiled with JetBrains decompiler
// Type: Paritee.StardewValley.Core.Locations.Weather
// Assembly: Paritee.StardewValley.Core, Version=2.0.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 5A2FE3D9-1A06-4344-9586-3DF16623F5C9
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\Paritee's Better Farm Animal Variety\Paritee.StardewValley.Core.dll

using StardewValley;

namespace Paritee.StardewValley.Core.Locations
{
  public class Weather
  {
    public enum Name
    {
      Sunny,
      Rain,
      Debris,
      Lightning,
      Festival,
      Snow,
      Wedding
    }

    public static bool IsWeather(Name weather, Name target)
    {
      return IsWeather((int) weather, (int) target);
    }

    public static bool IsWeather(int weather, int target)
    {
      return weather == target;
    }

    public static int GetToday()
    {
      return Game1.weatherIcon;
    }

    public static bool IsToday(Name weather)
    {
      return IsToday((int) weather);
    }

    public static bool IsToday(int weather)
    {
      return IsWeather(GetToday(), weather);
    }

    public static int GetTomorrow()
    {
      return Game1.weatherForTomorrow;
    }

    public static bool IsTomorrow(Name weather)
    {
      return IsTomorrow((int) weather);
    }

    public static bool IsTomorrow(int weather)
    {
      return IsWeather(GetTomorrow(), weather);
    }

    public static bool IsRaining()
    {
      return Game1.isRaining || IsToday(Name.Rain);
    }

    public static bool IsSnowing()
    {
      return Game1.isSnowing || IsToday(Name.Snow);
    }

    public static bool IsLightning()
    {
      return Game1.isLightning || IsToday(Name.Lightning);
    }

    public static bool IsDebris()
    {
      return Game1.isDebrisWeather || IsToday(Name.Debris);
    }

    public static bool IsWedding()
    {
      return Game1.weddingToday || IsToday(Name.Wedding);
    }

    public static bool IsFestival()
    {
      return Game1.isFestival() || IsToday(Name.Festival);
    }

    public static string GetFestivalLocation()
    {
      return Game1.whereIsTodaysFest;
    }
  }
}