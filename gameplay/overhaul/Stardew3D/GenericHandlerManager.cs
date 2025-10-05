global using InputHandlerManager = Stardew3D.GenericHandlerManager<object, Stardew3D.IUpdateHandler>;
global using RenderHandlerManager = Stardew3D.GenericHandlerManager<object, Stardew3D.IRenderHandler>;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using StardewValley.Menus;

namespace Stardew3D;

public class GenericHandlerManager<TBaseType, THandlerInterface>
    where TBaseType : class
    where THandlerInterface : class
{
    private Dictionary<Type, (Func<TBaseType, THandlerInterface> createHandlerFunc, bool allowsSubclasses)> handlers = new();
    private Dictionary<Type, List<(Func<TBaseType, THandlerInterface> createHandlerFunc, bool allowsSubclasses)>> handlerAddons = new();
    public ConditionalWeakTable<TBaseType, THandlerInterface[]> ActiveHandlers = new();

    public void SetHandler<MenuType>(Func<TBaseType, THandlerInterface> createHandlerFunc, bool includeMenuSubclasses = true)
        where MenuType : TBaseType
    {
        handlers[typeof(MenuType)] = new(createHandlerFunc, includeMenuSubclasses);
    }

    public void AddHandlerAddon<MenuType>(Func<TBaseType, THandlerInterface> createHandlerFunc, bool includeMenuSubclasses = true)
        where MenuType : TBaseType
    {
        handlerAddons.TryAdd(typeof(MenuType), new());
        handlerAddons[typeof(MenuType)].Add(new(createHandlerFunc, includeMenuSubclasses));
    }

    public THandlerInterface[] CreateApplicableHandlers(TBaseType menu)
    {
        List<THandlerInterface> ret = new();
        bool didPrimary = false;
        for (Type check = menu.GetType(); check != typeof(TBaseType).BaseType; check = check.BaseType)
        {
            if (!didPrimary)
            {
                if (handlers.TryGetValue(check, out var handlerData))
                {
                    if (handlerData.allowsSubclasses || check == menu.GetType())
                    {
                        didPrimary = true;
                        ret.Insert(0, handlerData.createHandlerFunc(menu)); // Insert, not add, so that even if some addons get added first, the main handler goes first
                    }
                }
            }

            if (handlerAddons.TryGetValue(check, out var addonDataList))
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

    public THandlerInterface[] GetHandlersFor(TBaseType obj)
    {
        return ActiveHandlers.GetValue(obj, _ => CreateApplicableHandlers(obj));
    }
}
