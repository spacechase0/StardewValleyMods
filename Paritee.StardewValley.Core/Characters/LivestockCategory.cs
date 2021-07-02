// Decompiled with JetBrains decompiler
// Type: Paritee.StardewValley.Core.Characters.LivestockCategory
// Assembly: Paritee.StardewValley.Core, Version=2.0.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 5A2FE3D9-1A06-4344-9586-3DF16623F5C9
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\Paritee's Better Farm Animal Variety\Paritee.StardewValley.Core.dll

using System.Collections.Generic;
using Paritee.StardewValley.Core.Utilities;

namespace Paritee.StardewValley.Core.Characters
{
  public class LivestockCategory : PropertyConstant
  {
    public readonly int Order;
    public LivestockAnimalShop AnimalShop;
    public List<string> Buildings = new List<string>();
    public List<Livestock> Types = new List<Livestock>();

    public LivestockCategory(string name)
      : base(name)
    {
    }

    public LivestockCategory(
      string name,
      int order,
      List<Livestock> types,
      List<string> buildings,
      LivestockAnimalShop animalShop)
      : base(name)
    {
      Order = order;
      Types = types;
      Buildings = buildings;
      AnimalShop = animalShop;
    }

    public bool CanBePurchased()
    {
      return AnimalShop != null;
    }

    public static string LoadDisplayName(string id)
    {
      return Content.LoadString("Strings\\StringsFromCSFiles:Utility.cs." + id);
    }

    public static string LoadDescription(string id)
    {
      return Content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs." + id);
    }
  }
}