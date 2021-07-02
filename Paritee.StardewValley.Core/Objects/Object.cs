// Decompiled with JetBrains decompiler
// Type: Paritee.StardewValley.Core.Objects.Object
// Assembly: Paritee.StardewValley.Core, Version=2.0.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 5A2FE3D9-1A06-4344-9586-3DF16623F5C9
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\Paritee's Better Farm Animal Variety\Paritee.StardewValley.Core.dll

using Paritee.StardewValley.Core.Utilities;
using SDV = StardewValley;

namespace Paritee.StardewValley.Core.Objects
{
  public class Object
  {
    public enum DataValueIndex
    {
      Name,
      Price,
      Edibility,
      TypeAndCategory,
      DisplayName,
      SetOutdoors,
      SetIndoors,
      Fragility,
      IsLamp
    }

    public enum Quality
    {
      Low = 0,
      Medium = 1,
      High = 2,
      Best = 4
    }

    public const int NoIndex = -1;

    public static string GetName(SDV.Object obj)
    {
      return obj.Name;
    }

    public static bool IsIncubator(SDV.Object obj)
    {
      return IsBigCraftable(obj) && GetName(obj).Contains("Incubator");
    }

    public static bool IsHoldingObject(SDV.Object obj)
    {
      return obj.heldObject.Value != null;
    }

    public static int GetMinutesUntilReady(SDV.Object obj)
    {
      return obj.MinutesUntilReady;
    }

    public static bool IsReady(SDV.Object obj)
    {
      return GetMinutesUntilReady(obj) <= 0;
    }

    public static bool IsAutoGrabber(SDV.Object obj)
    {
      return IsItem(obj, 165) && IsBigCraftable(obj);
    }

    public static bool IsItem(SDV.Object obj, int itemIndex)
    {
      return obj.ParentSheetIndex == itemIndex;
    }

    public static bool IsBigCraftable(SDV.Object obj)
    {
      return obj.bigCraftable.Value;
    }

    public static bool TryParse(string name, out int index)
    {
      index = -1;
      foreach (var keyValuePair in Content.LoadData<int, string>(Content.DataObjectInformationContentPath))
        if (Content.ParseDataValue(keyValuePair.Value)[0] == name)
        {
          index = keyValuePair.Key;
          return true;
        }

      return false;
    }

    public static bool ObjectExists(int index)
    {
      return Content.LoadData<int, string>(Content.DataObjectInformationContentPath).ContainsKey(index);
    }
  }
}