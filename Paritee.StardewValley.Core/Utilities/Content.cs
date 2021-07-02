// Decompiled with JetBrains decompiler
// Type: Paritee.StardewValley.Core.Utilities.Content
// Assembly: Paritee.StardewValley.Core, Version=2.0.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 5A2FE3D9-1A06-4344-9586-3DF16623F5C9
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\Paritee's Better Farm Animal Variety\Paritee.StardewValley.Core.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace Paritee.StardewValley.Core.Utilities
{
  public class Content
  {
    public const string AnimalsContentDirectory = "Animals";
    public const char DataValueDelimiter = '/';
    public const string None = "none";
    public const int StartingFrame = 0;

    public static string DataFarmAnimalsContentPath => BuildPath(new string[2]
    {
      "Data",
      "FarmAnimals"
    });

    public static string DataBlueprintsContentPath => BuildPath(new string[2]
    {
      "Data",
      "Blueprints"
    });

    public static string DataObjectInformationContentPath => BuildPath(new string[2]
    {
      "Data",
      "ObjectInformation"
    });

    public static GraphicsDevice GetGraphicsDevice()
    {
      return Game1.game1.GraphicsDevice;
    }

    public static bool Exists<T>(string name)
    {
      try
      {
        Load<T>(name);
        return true;
      }
      catch
      {
        return false;
      }
    }

    public static T Load<T>(string path)
    {
      return Game1.content.Load<T>(path);
    }

    public static string LoadString(string path)
    {
      return Game1.content.LoadString(path);
    }

    public static string LoadString(string path, object sub1)
    {
      return Game1.content.LoadString(path, sub1);
    }

    public static string LoadString(string path, object sub1, object sub2)
    {
      return Game1.content.LoadString(path, sub1, sub2);
    }

    public static string LoadString(string path, object sub1, object sub2, object sub3)
    {
      return Game1.content.LoadString(path, sub1, sub2, sub3);
    }

    public static string LoadString(string path, params string[] substitutions)
    {
      return Game1.content.LoadString(path, substitutions);
    }

    public static KeyValuePair<T, U> GetDataEntry<T, U>(Dictionary<T, U> data, T id)
    {
      return data.FirstOrDefault(kvp => kvp.Key.Equals(id));
    }

    public static U GetDataValue<T, U>(string path, T id, int index)
    {
      var dataEntry = GetDataEntry(LoadData<T, U>(path), id);
      return (object) dataEntry.Key == null || dataEntry.Key.Equals(default(T))
        ? default
        : (U) Convert.ChangeType(ParseDataValue(dataEntry.Value.ToString())[index], typeof(U));
    }

    public static KeyValuePair<T, U> LoadDataEntry<T, U>(string path, T id)
    {
      return GetDataEntry(Load<Dictionary<T, U>>(path), id);
    }

    public static Dictionary<T, U> LoadData<T, U>(string path)
    {
      return Load<Dictionary<T, U>>(path);
    }

    public static string BuildPath(string[] parts)
    {
      return Path.Combine(parts);
    }

    public static string[] ParseDataValue(string str)
    {
      return str.Split('/');
    }

    public static int GetWidthOfString(string str, int widthContraint = 9999999)
    {
      return SpriteText.getWidthOfString(str);
    }

    public static string FormatMoneyString(int amount)
    {
      return "$" + LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", amount);
    }
  }
}