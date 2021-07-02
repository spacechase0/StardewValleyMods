// Decompiled with JetBrains decompiler
// Type: Paritee.StardewValley.Core.Utilities.Mod
// Assembly: Paritee.StardewValley.Core, Version=2.0.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 5A2FE3D9-1A06-4344-9586-3DF16623F5C9
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\Paritee's Better Farm Animal Variety\Paritee.StardewValley.Core.dll

using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Paritee.StardewValley.Core.Utilities
{
  public class Mod
  {
    public static string Path => GetPath();

    public static string GetPath()
    {
      return System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    }

    public static string SmapiSaveDataKey(string uniqueModId, string key)
    {
      return "smapi/mod-data/" + (uniqueModId ?? "").ToLower() + "/" + key;
    }

    public static T ReadSaveData<T>(string uniqueModId, string key) where T : new()
    {
      var obj = Game.ReadSaveData<T>(SmapiSaveDataKey(uniqueModId, key));
      return (object) obj == null ? new T() : obj;
    }

    public static void WriteSaveData<T>(string uniqueModId, string key, T data)
    {
      Game.WriteSaveData(SmapiSaveDataKey(uniqueModId, key), data);
    }

    public static T ReadConfig<T>(string modPath, string fileName) where T : new()
    {
      var path = System.IO.Path.Combine(modPath, fileName);
      return !File.Exists(path) ? new T() : JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
    }

    public static Texture2D LoadTexture(string filePath)
    {
      Texture2D texture2D;
      using (var fileStream = new FileStream(filePath, FileMode.Open))
      {
        texture2D = Texture2D.FromStream(Content.GetGraphicsDevice(), fileStream);
      }

      return texture2D;
    }
  }
}