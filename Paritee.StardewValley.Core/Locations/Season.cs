// Decompiled with JetBrains decompiler
// Type: Paritee.StardewValley.Core.Locations.Season
// Assembly: Paritee.StardewValley.Core, Version=2.0.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 5A2FE3D9-1A06-4344-9586-3DF16623F5C9
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\Paritee's Better Farm Animal Variety\Paritee.StardewValley.Core.dll

using System;
using StardewValley;

namespace Paritee.StardewValley.Core.Locations
{
  public class Season
  {
    public enum Name
    {
      Spring,
      Summer,
      Fall,
      Winter
    }

    public static string GetCurrentSeason()
    {
      return Game1.currentSeason;
    }

    public static int ConvertToNumber(string season)
    {
      return Utility.getSeasonNumber(season);
    }

    public static bool IsSpring()
    {
      return Game1.IsSpring || IsCurrentSeason(Name.Spring);
    }

    public static bool IsSummer()
    {
      return Game1.IsSummer || IsCurrentSeason(Name.Summer);
    }

    public static bool IsFall()
    {
      return Game1.IsFall || IsCurrentSeason(Name.Fall);
    }

    public static bool IsWinter()
    {
      return Game1.IsWinter || IsCurrentSeason(Name.Winter);
    }

    public static bool IsCurrentSeason(Name season)
    {
      return IsCurrentSeason(season.ToString());
    }

    public static bool IsCurrentSeason(string season)
    {
      return string.Equals(GetCurrentSeason(), season, StringComparison.CurrentCultureIgnoreCase);
    }
  }
}