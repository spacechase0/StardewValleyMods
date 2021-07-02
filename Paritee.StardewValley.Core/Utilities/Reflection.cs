// Decompiled with JetBrains decompiler
// Type: Paritee.StardewValley.Core.Utilities.Reflection
// Assembly: Paritee.StardewValley.Core, Version=2.0.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 5A2FE3D9-1A06-4344-9586-3DF16623F5C9
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\Paritee's Better Farm Animal Variety\Paritee.StardewValley.Core.dll

using System;
using System.Reflection;

namespace Paritee.StardewValley.Core.Utilities
{
  public class Reflection
  {
    public static PropertyInfo GetProperty(
      object obj,
      string name,
      BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
    {
      return obj is Type type ? type.GetProperty(name, bindingAttr) : obj.GetType().GetProperty(name, bindingAttr);
    }

    public static FieldInfo GetField(object obj, string name,
      BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
    {
      return obj is Type type ? type.GetField(name, bindingAttr) : obj.GetType().GetField(name, bindingAttr);
    }

    public static T GetFieldValue<T>(object obj, string name,
      BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
    {
      var field = GetField(obj, name, bindingAttr);
      obj = obj is Type _ ? null : obj;
      return (T) field.GetValue(obj);
    }

    public static MethodInfo GetMethod(object obj, string name,
      BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
    {
      return obj is Type type ? type.GetMethod(name, bindingAttr) : obj.GetType().GetMethod(name, bindingAttr);
    }
  }
}