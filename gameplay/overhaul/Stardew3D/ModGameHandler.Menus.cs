using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley.Menus;

namespace Stardew3D;
public partial class ModGameHandler
{
    private Dictionary<Type, (Func<IClickableMenu, IMenuHandler> createHandlerFunc, bool allowsSubclasses)> menuHandlers = new();
    private Dictionary<Type, List<(Func<IClickableMenu, IMenuHandler> createHandlerFunc, bool allowsSubclasses)>> menuHandlerAddons = new();
    public void SetMenuHandler<MenuType>(Func<IClickableMenu, IMenuHandler> createHandlerFunc, bool includeMenuSubclasses = true)
    {
        if (!typeof(MenuType).IsAssignableTo(typeof(IClickableMenu)))
            throw new ArgumentException("Must inherit from StardewValley.Menus.IClickableMenu", nameof(MenuType));

        menuHandlers[typeof(MenuType)] = new(createHandlerFunc, includeMenuSubclasses);
    }

    public void AddMenuHandlerAddon<MenuType>(Func<IClickableMenu, IMenuHandler> createHandlerFunc, bool includeMenuSubclasses = true)
    {
        if (!typeof(MenuType).IsAssignableTo(typeof(IClickableMenu)))
            throw new ArgumentException("Must inherit from StardewValley.Menus.IClickableMenu", nameof(MenuType));

        menuHandlerAddons.TryAdd(typeof(MenuType), new());
        menuHandlerAddons[typeof(MenuType)].Add(new(createHandlerFunc, includeMenuSubclasses));
    }

    public IMenuHandler[] CreateApplicableMenuHandlers(IClickableMenu menu)
    {
        List<IMenuHandler> ret = new();
        bool didPrimary = false;
        for (Type check = menu.GetType(); check != typeof(IClickableMenu).BaseType; check = check.BaseType)
        {
            if (!didPrimary)
            {
                if (menuHandlers.TryGetValue(check, out var handlerData))
                {
                    if (handlerData.allowsSubclasses || check == menu.GetType())
                    {
                        didPrimary = true;
                        ret.Insert(0, handlerData.createHandlerFunc(menu)); // Insert, not add, so that even if some addons get added first, the main handler goes first
                    }
                }
            }

            if (menuHandlerAddons.TryGetValue(check, out var addonDataList))
            {
                foreach (var addonData in addonDataList)
                {
                    if (addonData.allowsSubclasses || check == menu.GetType())
                    {
                        ret.Insert(didPrimary ? 1 : 0, addonData.createHandlerFunc(menu)); // Insert, not add, so that parent class addons come first
                    }
                }
            }
        }
        return ret.ToArray();
    }
}
