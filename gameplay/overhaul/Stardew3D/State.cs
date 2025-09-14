using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SharpGLTF.Schema2;
using SpaceShared;
using Stardew3D.FirstPerson;
using Stardew3D.Models;
using Stardew3D.ThirdPerson;
using StardewValley;
using StardewValley.Menus;

namespace Stardew3D;
public class State
{
    public List<IGameHandler> Handlers { get; } = [null, new FirstPersonGameHandler(), new ThirdPersonGameHandler()];
    private int _activeHandlerIndex = 0;
    public int ActiveHandlerIndex
    {
        get => _activeHandlerIndex;
        set => _activeHandlerIndex = value % Handlers.Count;
    }
    public IGameHandler ActiveHandler => Handlers[ActiveHandlerIndex];

    public ModelManager ModelManager { get; set; } = new();

    public void SetMenuHandlerForGameHandlerTags<MenuType>(IReadOnlyCollection<string> requiredTags, Func<IGameHandler, Func<IClickableMenu, IMenuHandler>> createHandlerFunc, bool forSubclassesToo = true)
    {
        foreach (var handler in Handlers)
        {
            if (handler == null)
                continue;

            if (!requiredTags.All(requiredTag => handler.Tags.Contains(requiredTag)))
                continue;

            handler.SetMenuHandler< MenuType >(createHandlerFunc(handler), forSubclassesToo);
        }
    }
    public void AddMenuHandlerAddonForGameHandlerTags<MenuType>(IReadOnlyCollection<string> requiredTags, Func<IGameHandler, Func<IClickableMenu, IMenuHandler>> createHandlerFunc, bool forSubclassesToo = true)
    {
        foreach (var handler in Handlers)
        {
            if (handler == null)
                continue;

            if (!requiredTags.All(requiredTag => handler.Tags.Contains(requiredTag)))
                continue;

            handler.AddMenuHandlerAddon<MenuType>(createHandlerFunc(handler), forSubclassesToo);
        }
    }

    private ConditionalWeakTable<IClickableMenu, Dictionary<IGameHandler, IMenuHandler[]>> activeMenuHandlers = new();
    public IMenuHandler[] GetMenuHandlersFor(IClickableMenu menu)
    {
        if (ActiveHandler == null || menu == null)
            return [];

        var forGameHandlers = activeMenuHandlers.GetOrCreateValue(menu);
        if (!forGameHandlers.TryGetValue(ActiveHandler, out var menuHandlers))
        {
            forGameHandlers.Add(ActiveHandler, menuHandlers = ActiveHandler.CreateApplicableMenuHandlers(menu));
        }
        return menuHandlers;
    }
}

