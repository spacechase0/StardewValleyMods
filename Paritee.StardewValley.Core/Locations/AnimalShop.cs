// Decompiled with JetBrains decompiler
// Type: Paritee.StardewValley.Core.Locations.AnimalShop
// Assembly: Paritee.StardewValley.Core, Version=2.0.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 5A2FE3D9-1A06-4344-9586-3DF16623F5C9
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\Paritee's Better Farm Animal Variety\Paritee.StardewValley.Core.dll

using System;
using System.Linq;
using Paritee.StardewValley.Core.Characters;
using Paritee.StardewValley.Core.Utilities;
using SDV = StardewValley;

namespace Paritee.StardewValley.Core.Locations
{
  public class AnimalShop
  {
    public const int PurchaseAnimalStockParentSheetIndex = 100;
    public const int PurchaseAnimalStockQuantity = 1;

    public static SDV.Object FormatAsAnimalAvailableForPurchase(
      SDV.Farm farm,
      string name,
      string displayName,
      string[] types,
      string[] buildings)
    {
      string type;
      RequiredBuildingIsBuilt(farm, buildings, out type);
      var @object = new SDV.Object(100, 1, price: (int) Math.Ceiling(FarmAnimal.GetCheapestPrice(types.ToList()) / 2.0))
      {
        Type = type,
        displayName = displayName
      };
      @object.Name = name;
      return @object;
    }

    public static bool RequiredBuildingIsBuilt(SDV.Farm farm, string[] buildings, out string type)
    {
      if (buildings.Where(name => Location.IsBuildingConstructed(farm, name)).Any())
      {
        type = null;
        return true;
      }

      var dataValue = Content.GetDataValue<string, string>(Content.DataBlueprintsContentPath, buildings.First(), 8);
      type = Content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5926").Replace("Coop", dataValue);
      return false;
    }

    public static bool IsBlueChickenAvailableForPurchase(SDV.Farmer farmer)
    {
      return FarmAnimal.RollBlueChickenChance(farmer);
    }
  }
}