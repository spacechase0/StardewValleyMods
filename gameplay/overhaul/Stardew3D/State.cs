using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SharpGLTF.Schema2;
using SpaceShared;
using Stardew3D.Handlers;
using Stardew3D.Handlers.Game;
using Stardew3D.Handlers.Game.FirstPerson;
using Stardew3D.Handlers.Game.ThirdPerson;
using Stardew3D.Models;
using Stardew3D.Rendering;
using Stardew3D.Rendering.Renderers;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace Stardew3D;
public class State
{
    private Dictionary<string, IGameHandler> Handlers { get; } = [];
    internal string ActiveHandlerId = null;

    public IGameHandler ActiveHandler
    {
        get => ActiveHandlerId == null ? null : Handlers[ActiveHandlerId];
        set
        {
            if (value != null && (!Handlers.ContainsKey(value.Id) || !Handlers.Values.Contains(value) || Handlers[value.Id] != value))
                throw new ArgumentException($"Given handler {value} wasn't registered", nameof(ActiveHandler));
            Log.Debug($"Switching from game handler \"{value?.Id ?? "null"}\" to \"{ActiveHandlerId ?? "null"}\"");

            if (value == null)
            {
                ActiveHandler?.SwitchOff(null);
                ActiveHandlerId = null;
                return;
            }

            var oldHandler = ActiveHandler;
            ActiveHandlerId = value?.Id;

            oldHandler?.SwitchOff(ActiveHandler);
            ActiveHandlerId = value?.Id;
            ActiveHandler?.SwitchOn(oldHandler);
        }
    }
    public void AddGameHandler(IGameHandler handler)
    {
        if (finishedAddingGameHandlers)
            throw new InvalidOperationException("Game handler registration has already finished");

        Handlers.Add(handler.Id, handler);
    }
    public IGameHandler GetGameHandler(string id) => Handlers.GetOrDefault(id, null);
    public IEnumerable<string> HandlerIds => Handlers.Keys;

    public static event EventHandler AddingGameHandlers;
    public static event EventHandler GameHandlersFinalized;
    private bool invokedEventsForThis = false;
    private bool finishedAddingGameHandlers = false;

    public ModelManager ModelManager { get; } = new();
    public GenericModelEffect GenericModelEffect { get; }

    private class GameHandlerSpecificData
    {
        public UpdateHandlerManager UpdateHandlerManager { get; } = new();
        public RenderHandlerManager RenderHandlerManager { get; } = new();
        public ConditionalWeakTable<object, object> JointHandlers { get; } = new();
    }
    private ConditionalWeakTable<IGameHandler, GameHandlerSpecificData> handlerData = new();

    internal State()
    {
        GenericModelEffect = new(Game1.graphics.GraphicsDevice, File.ReadAllBytes(Path.Combine(Mod.Instance.Helper.DirectoryPath, "assets", "GenericModelEffect.mgfxo")));

        if (Context.IsGameLaunched)
            InvokeAddingGameHandlers();
    }

    internal void InvokeAddingGameHandlers()
    {
        if (invokedEventsForThis)
            return;

        invokedEventsForThis = true;
        AddingGameHandlers?.Invoke(this, new());
        finishedAddingGameHandlers = true;
        GameHandlersFinalized?.Invoke(this, new());
    }

    public IEnumerable<IGameHandler> FindGameHandlersMatching(IReadOnlyCollection<string> requiredTags)
    {
        foreach (var handler in Handlers.Values)
        {
            if (handler == null)
                continue;

            if (!requiredTags.All(requiredTag => handler.Tags.Contains(requiredTag)))
                continue;

            yield return handler;
        }
        yield break;
    }

    public void SetJointHandlerForGameHandlerTags<ObjectType, THandlerType>(IReadOnlyCollection<string> requiredTags, Func<IGameHandler, Func<object, THandlerType>> createHandlerFunc, bool forSubclassesToo = true)
        where ObjectType : class
        where THandlerType : IUpdateHandler, IRenderHandler
    {
        foreach (var handler in FindGameHandlersMatching(requiredTags))
        {
            var createHandler = createHandlerFunc(handler);
            var data = handlerData.GetOrCreateValue(handler);
            data.UpdateHandlerManager.SetHandler<ObjectType>(obj => (IUpdateHandler) data.JointHandlers.GetValue(obj, _ => createHandler(obj)), forSubclassesToo);
            data.RenderHandlerManager.SetHandler<ObjectType>(obj => (IRenderHandler) data.JointHandlers.GetValue(obj, _ => createHandler(obj)), forSubclassesToo);
        }
    }
    public void AddJointHandlerAddonForGameHandlerTags<ObjectType, THandlerType>(IReadOnlyCollection<string> requiredTags, Func<IGameHandler, Func<object, THandlerType>> createHandlerFunc, bool forSubclassesToo = true)
        where ObjectType : class
        where THandlerType : IUpdateHandler, IRenderHandler
    {
        foreach (var handler in FindGameHandlersMatching(requiredTags))
        {
            var createHandler = createHandlerFunc(handler);
            var data = handlerData.GetOrCreateValue(handler);
            data.UpdateHandlerManager.AddHandlerAddon<ObjectType>(obj => (IUpdateHandler)data.JointHandlers.GetValue(obj, _ => createHandler(obj)), forSubclassesToo);
            data.RenderHandlerManager.AddHandlerAddon<ObjectType>(obj => (IRenderHandler)data.JointHandlers.GetValue(obj, _ => createHandler(obj)), forSubclassesToo);
        }
    }

    public void SetUpdateHandlerForGameHandlerTags<InputType>(IReadOnlyCollection<string> requiredTags, Func<IGameHandler, Func<object, IUpdateHandler>> createHandlerFunc, bool forSubclassesToo = true)
        where InputType : class
    {
        foreach (var handler in FindGameHandlersMatching(requiredTags))
        {
            handlerData.GetOrCreateValue(handler).UpdateHandlerManager.SetHandler<InputType>(createHandlerFunc(handler), forSubclassesToo);
        }
    }
    public void AddUpdateHandlerAddonForGameHandlerTags<InputType>(IReadOnlyCollection<string> requiredTags, Func<IGameHandler, Func<object, IUpdateHandler>> createHandlerFunc, bool forSubclassesToo = true)
         where InputType : class
    {
        foreach (var handler in FindGameHandlersMatching(requiredTags))
        {
            handlerData.GetOrCreateValue(handler).UpdateHandlerManager.AddHandlerAddon<InputType>(createHandlerFunc(handler), forSubclassesToo);
        }
    }
    public IUpdateHandler[] GetUpdateHandlersFor(object obj)
    {
        if (ActiveHandler == null || obj == null)
            return [];

        return handlerData.GetOrCreateValue(ActiveHandler).UpdateHandlerManager.GetHandlersFor(obj);
    }

    public void SetRenderHandlerForGameHandlerTags<RenderType>(IReadOnlyCollection<string> requiredTags, Func<IGameHandler, Func<object, Renderer>> createHandlerFunc, bool forSubclassesToo = true)
        where RenderType : class
    {
        foreach (var handler in FindGameHandlersMatching(requiredTags))
        {
            handlerData.GetOrCreateValue(handler).RenderHandlerManager.SetHandler<RenderType>(createHandlerFunc(handler), forSubclassesToo);
        }
    }
    public void AddRenderHandlerAddonForGameHandlerTags<RenderType>(IReadOnlyCollection<string> requiredTags, Func<IGameHandler, Func<object, Renderer>> createHandlerFunc, bool forSubclassesToo = true)
         where RenderType : class
    {
        foreach (var handler in FindGameHandlersMatching(requiredTags))
        {
            handlerData.GetOrCreateValue(handler).RenderHandlerManager.AddHandlerAddon<RenderType>(createHandlerFunc(handler), forSubclassesToo);
        }
    }
    public IRenderHandler[] GetRenderHandlersFor(object obj)
    {
        if (ActiveHandler == null || obj == null)
            return [];

        return handlerData.GetOrCreateValue(ActiveHandler).RenderHandlerManager.GetHandlersFor(obj);
    }
}

