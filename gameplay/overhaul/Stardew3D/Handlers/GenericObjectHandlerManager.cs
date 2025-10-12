global using UpdateHandlerManager = Stardew3D.Handlers.GenericObjectHandlerManager<object, Stardew3D.Handlers.IUpdateHandler>;
global using RenderHandlerManager = Stardew3D.Handlers.GenericObjectHandlerManager<object, Stardew3D.Handlers.IRenderHandler>;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using StardewValley.Menus;

namespace Stardew3D.Handlers;

public class GenericObjectHandlerManager<TBaseType, THandlerInterface>
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
        List<THandlerInterface> ret = [null];
        for (Type check = menu.GetType(); check != typeof(TBaseType).BaseType; check = check.BaseType)
        {
            if (ret[0] == null && handlers.TryGetValue(check, out var handlerData))
            {
                if (handlerData.allowsSubclasses || check == menu.GetType())
                {
                    ret[0] = handlerData.createHandlerFunc(menu);
                }
            }

            if (handlerAddons.TryGetValue(check, out var addonDataList))
            {
                foreach (var addonData in addonDataList)
                {
                    if (addonData.allowsSubclasses || check == menu.GetType())
                    {
                        ret.Insert(1, addonData.createHandlerFunc(menu)); // Insert, not add, so that parent class addons come first
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
