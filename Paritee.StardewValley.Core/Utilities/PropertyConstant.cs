// Decompiled with JetBrains decompiler
// Type: Paritee.StardewValley.Core.Utilities.PropertyConstant
// Assembly: Paritee.StardewValley.Core, Version=2.0.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 5A2FE3D9-1A06-4344-9586-3DF16623F5C9
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\Paritee's Better Farm Animal Variety\Paritee.StardewValley.Core.dll

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Paritee.StardewValley.Core.Utilities
{
  public class PropertyConstant
  {
    protected PropertyConstant(string name)
    {
      Name = name;
    }

    private string Name { get; }

    public override string ToString()
    {
      return Name;
    }

    private static string Parse(string str)
    {
      return str.Replace(" ", "");
    }

    protected static bool Exists<T>(string str)
    {
      return str != null && Reflection.GetProperty(typeof(T), Parse(str), BindingFlags.Static | BindingFlags.Public) !=
        null;
    }

    protected static List<T> All<T>()
    {
      return typeof(T).GetProperties(BindingFlags.Static | BindingFlags.Public)
        .Select(p => (T) p.GetValue(typeof(T))).ToList();
    }
  }
}