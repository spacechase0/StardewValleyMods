using System.Runtime.CompilerServices;
using SpaceShared;
using Stardew3D.GameModes;
using Stardew3D.Handlers;
using Stardew3D.Models;
using Stardew3D.Rendering;
using StardewModdingAPI;
using StardewValley;

namespace Stardew3D;
public class State
{
    private Dictionary<string, IGameMode> Modes { get; } = [];
    internal string ActiveModeId = null;

    public IGameMode ActiveMode
    {
        get => ActiveModeId == null ? null : Modes[ActiveModeId];
        set
        {
            if (value != null && (!Modes.ContainsKey(value.Id) || !Modes.Values.Contains(value) || Modes[value.Id] != value))
                throw new ArgumentException($"Given handler {value} wasn't registered", nameof(ActiveMode));
            Log.Debug($"Switching to game handler \"{value?.Id ?? "null"}\" (from \"{ActiveModeId ?? "null"}\")");

            if (value == null)
            {
                ActiveMode?.SwitchOff(null);
                ActiveModeId = null;
                return;
            }

            var oldHandler = ActiveMode;
            ActiveModeId = value?.Id;

            oldHandler?.SwitchOff(ActiveMode);
            ActiveModeId = value?.Id;
            ActiveMode?.SwitchOn(oldHandler);
        }
    }
    public void AddGameMode(IGameMode mode)
    {
        if (finishedAddingGameModes)
            throw new InvalidOperationException("Game handler registration has already finished");

        Modes.Add(mode.Id, mode);
    }
    public IGameMode GetGameMode(string id) => Modes.GetOrDefault(id, null);
    public IEnumerable<string> HandlerIds => Modes.Keys;

    public static event EventHandler AddingGameModes;
    public static event EventHandler GameModesFinalized;
    private bool invokedEventsForThis = false;
    private bool finishedAddingGameModes = false;

    public ModelManager ModelManager { get; } = new();
    public GenericModelEffect GenericModelEffect { get; }

    public bool RenderDebugDraw { get; set; } = false;
    public bool RenderDebugGrid { get; set; } = false;
    public bool RenderDebugInteractions { get; set; } = false;

    private class GameModeSpecificData
    {
        public UpdateHandlerManager UpdateHandlerManager { get; } = new();
        public RenderHandlerManager RenderHandlerManager { get; } = new();
        public ConditionalWeakTable<object, object> JointHandlers { get; } = new();
    }
    private ConditionalWeakTable<IGameMode, GameModeSpecificData> modeData = new();

    internal State()
    {
        GenericModelEffect = new(Game1.graphics.GraphicsDevice, File.ReadAllBytes(Path.Combine(Mod.Instance.Helper.DirectoryPath, "assets", "GenericModelEffect.mgfxo")));

        if (Context.IsGameLaunched)
            InvokeAddingGameModes();
    }

    internal void InvokeAddingGameModes()
    {
        if (invokedEventsForThis)
            return;

        invokedEventsForThis = true;
        AddingGameModes?.Invoke(this, new());
        finishedAddingGameModes = true;
        GameModesFinalized?.Invoke(this, new());
    }

    public void ClearHandlerState()
    {
        foreach (var entry in modeData)
        {
            entry.Value.UpdateHandlerManager.ActiveHandlers.Clear();
            entry.Value.RenderHandlerManager.ActiveHandlers.Clear();
            entry.Value.JointHandlers.Clear();
        }
    }

    public void ClearHandlerStateFor(object obj)
    {
        foreach (var entry in modeData)
        {
            entry.Value.UpdateHandlerManager.ActiveHandlers.Remove(obj);
            entry.Value.RenderHandlerManager.ActiveHandlers.Remove(obj);
            entry.Value.JointHandlers.Remove(obj);
        }
    }

    public IEnumerable<IGameMode> FindGameModesMatching(IReadOnlyCollection<string> requiredTags)
    {
        foreach (var mode in Modes.Values)
        {
            if (mode == null)
                continue;

            if (!requiredTags.All(requiredTag => mode.Tags.Contains(requiredTag)))
                continue;

            yield return mode;
        }
        yield break;
    }

    public void SetJointHandlerForGameModeTags<ObjectType, THandlerType>(IReadOnlyCollection<string> requiredTags, Func<IGameMode, Func<object, THandlerType>> createHandlerFunc, bool forSubclassesToo = true)
        where ObjectType : class
        where THandlerType : IUpdateHandler, IRenderHandler
    {
        foreach (var handler in FindGameModesMatching(requiredTags))
        {
            var createHandler = createHandlerFunc(handler);
            var data = modeData.GetOrCreateValue(handler);
            data.UpdateHandlerManager.SetHandler<ObjectType>(obj => (IUpdateHandler) data.JointHandlers.GetValue(obj, _ => createHandler(obj)), forSubclassesToo);
            data.RenderHandlerManager.SetHandler<ObjectType>(obj => (IRenderHandler) data.JointHandlers.GetValue(obj, _ => createHandler(obj)), forSubclassesToo);
        }
    }
    public void AddJointHandlerAddonForGameModeTags<ObjectType, THandlerType>(IReadOnlyCollection<string> requiredTags, Func<IGameMode, Func<object, THandlerType>> createHandlerFunc, bool forSubclassesToo = true)
        where ObjectType : class
        where THandlerType : IUpdateHandler, IRenderHandler
    {
        foreach (var handler in FindGameModesMatching(requiredTags))
        {
            var createHandler = createHandlerFunc(handler);
            var data = modeData.GetOrCreateValue(handler);
            data.UpdateHandlerManager.AddHandlerAddon<ObjectType>(obj => (IUpdateHandler)data.JointHandlers.GetValue(obj, _ => createHandler(obj)), forSubclassesToo);
            data.RenderHandlerManager.AddHandlerAddon<ObjectType>(obj => (IRenderHandler)data.JointHandlers.GetValue(obj, _ => createHandler(obj)), forSubclassesToo);
        }
    }

    public void SetUpdateHandlerForGameModeTags<InputType>(IReadOnlyCollection<string> requiredTags, Func<IGameMode, Func<object, IUpdateHandler>> createHandlerFunc, bool forSubclassesToo = true)
        where InputType : class
    {
        foreach (var handler in FindGameModesMatching(requiredTags))
        {
            modeData.GetOrCreateValue(handler).UpdateHandlerManager.SetHandler<InputType>(createHandlerFunc(handler), forSubclassesToo);
        }
    }
    public void AddUpdateHandlerAddonForGameModeTags<InputType>(IReadOnlyCollection<string> requiredTags, Func<IGameMode, Func<object, IUpdateHandler>> createHandlerFunc, bool forSubclassesToo = true)
         where InputType : class
    {
        foreach (var handler in FindGameModesMatching(requiredTags))
        {
            modeData.GetOrCreateValue(handler).UpdateHandlerManager.AddHandlerAddon<InputType>(createHandlerFunc(handler), forSubclassesToo);
        }
    }
    public IUpdateHandler[] GetUpdateHandlersFor(object obj)
    {
        if (ActiveMode == null || obj == null)
            return [null];

        return modeData.GetOrCreateValue(ActiveMode).UpdateHandlerManager.GetHandlersFor(obj);
    }

    public void SetRenderHandlerForGameModeTags<RenderType>(IReadOnlyCollection<string> requiredTags, Func<IGameMode, Func<object, Renderer>> createHandlerFunc, bool forSubclassesToo = true)
        where RenderType : class
    {
        foreach (var handler in FindGameModesMatching(requiredTags))
        {
            modeData.GetOrCreateValue(handler).RenderHandlerManager.SetHandler<RenderType>(createHandlerFunc(handler), forSubclassesToo);
        }
    }
    public void AddRenderHandlerAddonForGameModeTags<RenderType>(IReadOnlyCollection<string> requiredTags, Func<IGameMode, Func<object, Renderer>> createHandlerFunc, bool forSubclassesToo = true)
         where RenderType : class
    {
        foreach (var handler in FindGameModesMatching(requiredTags))
        {
            modeData.GetOrCreateValue(handler).RenderHandlerManager.AddHandlerAddon<RenderType>(createHandlerFunc(handler), forSubclassesToo);
        }
    }
    public IRenderHandler[] GetRenderHandlersFor(object obj)
    {
        if (ActiveMode == null || obj == null)
            return [null];

        return modeData.GetOrCreateValue(ActiveMode).RenderHandlerManager.GetHandlersFor(obj);
    }
}

