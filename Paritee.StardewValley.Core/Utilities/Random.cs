// Decompiled with JetBrains decompiler
// Type: Paritee.StardewValley.Core.Utilities.Random
// Assembly: Paritee.StardewValley.Core, Version=2.0.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 5A2FE3D9-1A06-4344-9586-3DF16623F5C9
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\Paritee's Better Farm Animal Variety\Paritee.StardewValley.Core.dll

using StardewValley;

namespace Paritee.StardewValley.Core.Utilities
{
  public class Random
  {
    public static System.Random GetNumberGenerator()
    {
      return Game1.random;
    }

    public static System.Random Seed(int seed)
    {
      return new System.Random(seed);
    }

    public static double NextDouble()
    {
      return GetNumberGenerator().NextDouble();
    }

    public static int Next()
    {
      return GetNumberGenerator().Next();
    }

    public static int Next(int maxValue)
    {
      return GetNumberGenerator().Next(maxValue);
    }

    public static int Next(int minValue, int maxValue)
    {
      return GetNumberGenerator().Next(minValue, maxValue);
    }
  }
}