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
using Stardew3D.Rendering;
using Stardew3D.Rendering.Renderers;
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

    public ModelManager ModelManager { get; } = new();
    public GenericModelEffect GenericModelEffect { get; }

    private class GameHandlerSpecificData
    {
        public InputHandlerManager UpdateHandlerManager { get; } = new();
        public RenderHandlerManager RenderHandlerManager { get; } = new();
        public ConditionalWeakTable<object, object> JointHandlers { get; } = new();
    }
    private ConditionalWeakTable<IGameHandler, GameHandlerSpecificData> handlerData = new();

    public State()
    {
        GenericModelEffect = new(Game1.graphics.GraphicsDevice, File.ReadAllBytes(Path.Combine(Mod.Instance.Helper.DirectoryPath, "assets", "GenericModelEffect.mgfxo")));
    }

    private IEnumerable<IGameHandler> FindGameHandlersMatching(IReadOnlyCollection<string> requiredTags)
    {
        foreach (var handler in Handlers)
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

