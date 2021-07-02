// Decompiled with JetBrains decompiler
// Type: Paritee.StardewValley.Core.Menus.PurchaseAnimals
// Assembly: Paritee.StardewValley.Core, Version=2.0.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 5A2FE3D9-1A06-4344-9586-3DF16623F5C9
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\Paritee's Better Farm Animal Variety\Paritee.StardewValley.Core.dll

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paritee.StardewValley.Core.Utilities;
using StardewValley.Buildings;
using StardewValley.Menus;
using SDV = StardewValley;

namespace Paritee.StardewValley.Core.Menus
{
  public class PurchaseAnimals : Menu
  {
    private const int iconsPerRow = 5;

    public static int GetIconsPerRow(PurchaseAnimalsMenu menu)
    {
      return 5;
    }

    public static bool IsFrozen(PurchaseAnimalsMenu menu)
    {
      return Reflection.GetFieldValue<bool>(menu, "freeze");
    }

    public static bool IsNamingAnimal(PurchaseAnimalsMenu menu)
    {
      return Reflection.GetFieldValue<bool>(menu, "namingAnimal");
    }

    public static void SetNamingAnimal(PurchaseAnimalsMenu menu, bool namingAnimal)
    {
      Reflection.GetField(menu, nameof(namingAnimal)).SetValue(menu, namingAnimal);
    }

    public static bool IsOnFarm(PurchaseAnimalsMenu menu)
    {
      return Reflection.GetFieldValue<bool>(menu, "onFarm");
    }

    public static void SetOnFarm(PurchaseAnimalsMenu menu, bool onFarm)
    {
      Reflection.GetField(menu, nameof(onFarm)).SetValue(menu, onFarm);
    }

    public static SDV.FarmAnimal GetAnimalBeingPurchased(PurchaseAnimalsMenu menu)
    {
      return Reflection.GetFieldValue<SDV.FarmAnimal>(menu, "animalBeingPurchased");
    }

    public static void SetAnimalBeingPurchased(PurchaseAnimalsMenu menu, SDV.FarmAnimal animal)
    {
      Reflection.GetField(menu, "animalBeingPurchased").SetValue(menu, animal);
    }

    public static void SetUpAnimalsToPurchase(
      PurchaseAnimalsMenu menu,
      List<SDV.Object> stock,
      Dictionary<string, Texture2D> icons,
      out int iconHeight)
    {
      var purchaseComponents = GetAnimalsToPurchaseComponents(menu, stock, icons, out iconHeight);
      SetAnimalsToPurchase(menu, purchaseComponents);
    }

    public static int GetPriceOfAnimal(PurchaseAnimalsMenu menu)
    {
      return Reflection.GetFieldValue<int>(menu, "priceOfAnimal");
    }

    public static void SetPriceOfAnimal(PurchaseAnimalsMenu menu, int price)
    {
      Reflection.GetField(menu, "priceOfAnimal").SetValue(menu, price);
    }

    public static Building GetNewAnimalHome(PurchaseAnimalsMenu menu)
    {
      return Reflection.GetFieldValue<Building>(menu, "newAnimalHome");
    }

    public static void SetNewAnimalHome(PurchaseAnimalsMenu menu, Building home)
    {
      Reflection.GetField(menu, "newAnimalHome").SetValue(menu, home);
    }

    public static ClickableTextureComponent GetOkButton(
      PurchaseAnimalsMenu menu)
    {
      return menu.okButton;
    }

    public static bool HasOkButton(PurchaseAnimalsMenu menu)
    {
      return GetOkButton(menu) != null;
    }

    public static bool HasTappedOkButton(PurchaseAnimalsMenu menu, int x, int y)
    {
      return HasOkButton(menu) && TappedOnButton(GetOkButton(menu), x, y);
    }

    public static bool IsReadyToClose(PurchaseAnimalsMenu menu)
    {
      return menu.readyToClose();
    }

    public static List<ClickableTextureComponent> GetAnimalsToPurchase(
      PurchaseAnimalsMenu menu)
    {
      return menu.animalsToPurchase;
    }

    public static void SetAnimalsToPurchase(
      PurchaseAnimalsMenu menu,
      List<ClickableTextureComponent> animalsToPurchase)
    {
      menu.animalsToPurchase = animalsToPurchase;
    }

    public static void SetHeight(PurchaseAnimalsMenu menu, int height)
    {
      menu.height = height;
    }

    public static void SetWidth(PurchaseAnimalsMenu menu, int width)
    {
      menu.width = width;
    }

    public static List<ClickableTextureComponent> GetAnimalsToPurchaseComponents(
      PurchaseAnimalsMenu menu,
      List<SDV.Object> stock,
      Dictionary<string, Texture2D> icons,
      out int iconHeight)
    {
      iconHeight = 0;
      var textureComponentList = new List<ClickableTextureComponent>();
      for (var index = 0; index < stock.Count; ++index)
      {
        var @object = stock[index];
        var name1 = @object.salePrice().ToString();
        string label = null;
        var name2 = @object.Name;
        var bounds = new Rectangle(menu.xPositionOnScreen + IClickableMenu.borderWidth + index % 5 * 64 * 2,
          menu.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth / 2 +
          index / 5 * 85, 128, 64);
        var icon = icons[@object.Name];
        var sourceRect = new Rectangle(0, 0, icon.Width, icon.Height);
        var scale = 4f;
        var drawShadow = @object.Type == null;
        var textureComponent1 =
          new ClickableTextureComponent(name1, bounds, label, name2, icon, sourceRect, scale, drawShadow);
        textureComponent1.item = @object;
        textureComponent1.myID = index;
        textureComponent1.rightNeighborID = index % 5 == 2 ? -1 : index + 1;
        textureComponent1.leftNeighborID = index % 5 == 0 ? -1 : index - 1;
        textureComponent1.downNeighborID = index + 5;
        textureComponent1.upNeighborID = index - 5;
        var textureComponent2 = textureComponent1;
        textureComponentList.Add(textureComponent2);
        iconHeight = icon.Height > iconHeight ? icon.Height : iconHeight;
      }

      return textureComponentList;
    }

    public static int GetRows(PurchaseAnimalsMenu menu)
    {
      return (int) Math.Ceiling(GetAnimalsToPurchase(menu).Count / 5.0);
    }

    public static void AdjustHeightBasedOnIcons(PurchaseAnimalsMenu menu, int iconHeight)
    {
      SetHeight(menu,
        (int) (iconHeight * 2.0) + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth / 2 +
        GetRows(menu) * 85);
    }
  }
}