using System;
using System.Runtime.CompilerServices;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewValley;
using Yarn;

namespace YarnEvents;

public class Mod : StardewModdingAPI.Mod
{
    public static Mod instance;

    internal static Library YarnLibrary;

    internal static IContentPatcherApi CP;

    public override void Entry(IModHelper helper)
    {
        instance = this;
        Log.Monitor = Monitor;

        Helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;

        Event.RegisterCommand("yarn", YarnCommand);
        Event.RegisterCommand("yarnContinue", YarnContinueCommand);
    }

    private void YarnCommand(Event @event, string[] args, EventContext context)
    {
        if (!ArgUtility.TryGet(args, 1, out string modId, out string error) || !ArgUtility.TryGet(args, 2, out string file, out error))
        {
            context.LogErrorAndSkip(error);
            return;
        }

        if (@event.currentCustomEventScript != null)
        {
            if (@event.currentCustomEventScript.update(context.Time, @event))
            {
                @event.currentCustomEventScript = null;
                @event.CurrentCommand++;
            }
        }
        else
        {
            @event.currentCustomEventScript = new YarnCustomEventScript(@event, context, modId, file);
        }
    }

    private void YarnContinueCommand(Event @event, string[] args, EventContext context)
    {
        (@event.currentCustomEventScript as YarnCustomEventScript)?.Continue();
        if (@event.currentCustomEventScript == null || @event.currentCustomEventScript.update(context.Time, @event))
        {
            @event.currentCustomEventScript = null;
            @event.CurrentCommand++;
        }
    }

    private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
    {
        YarnLibrary = new();
        CP = Helper.ModRegistry.GetApi<IContentPatcherApi>("Pathoschild.ContentPatcher");
    }
}
