// Decompiled with JetBrains decompiler
// Type: Paritee.StardewValley.Core.Menus.Menu
// Assembly: Paritee.StardewValley.Core, Version=2.0.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 5A2FE3D9-1A06-4344-9586-3DF16623F5C9
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\Paritee's Better Farm Animal Variety\Paritee.StardewValley.Core.dll

using StardewValley.Menus;

namespace Paritee.StardewValley.Core.Menus
{
  public class Menu
  {
    public static bool TappedOnButton(ClickableTextureComponent button, int x, int y)
    {
      return button.containsPoint(x, y);
    }
  }
}