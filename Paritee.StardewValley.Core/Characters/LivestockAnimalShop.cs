// Decompiled with JetBrains decompiler
// Type: Paritee.StardewValley.Core.Characters.LivestockAnimalShop
// Assembly: Paritee.StardewValley.Core, Version=2.0.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 5A2FE3D9-1A06-4344-9586-3DF16623F5C9
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\Paritee's Better Farm Animal Variety\Paritee.StardewValley.Core.dll

using System.Collections.Generic;

namespace Paritee.StardewValley.Core.Characters
{
  public class LivestockAnimalShop
  {
    public readonly int Price;
    public string Description;
    public List<Livestock> Exclude;
    public string Name;

    public LivestockAnimalShop(
      int price,
      string name,
      string description,
      List<Livestock> exclude)
    {
      Price = price;
      Name = name;
      Description = description;
      Exclude = exclude;
    }
  }
}