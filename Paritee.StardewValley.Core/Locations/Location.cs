// Decompiled with JetBrains decompiler
// Type: Paritee.StardewValley.Core.Locations.Location
// Assembly: Paritee.StardewValley.Core, Version=2.0.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 5A2FE3D9-1A06-4344-9586-3DF16623F5C9
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\Paritee's Better Farm Animal Variety\Paritee.StardewValley.Core.dll

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Network;

namespace Paritee.StardewValley.Core.Locations
{
  public class Location
  {
    public static IList<GameLocation> All()
    {
      return Game1.locations;
    }

    public static bool IsBuildingConstructed(Farm farm, string name)
    {
      return farm.isBuildingConstructed(name);
    }

    public static void RemoveAnimal(Farm farm, FarmAnimal animal)
    {
      farm.animals.Remove(animal.myID.Value);
    }

    public static bool IsOutdoors(GameLocation location)
    {
      return location.IsOutdoors;
    }

    public static void SpawnObject(GameLocation location, Vector2 tileLocation, Object obj)
    {
      Utility.spawnObjectAround(tileLocation, obj, location);
    }

    public static FarmerCollection GetFarmers(GameLocation location)
    {
      return location.farmers;
    }

    public static bool AnyFarmers(GameLocation location)
    {
      return GetFarmers(location).Any();
    }

    public static bool IsLocation(GameLocation location, GameLocation target)
    {
      return location == target;
    }
  }
}