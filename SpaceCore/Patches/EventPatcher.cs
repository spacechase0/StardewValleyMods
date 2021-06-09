using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Harmony;
using Microsoft.Xna.Framework;
using Spacechase.Shared.Harmony;
using SpaceCore.Events;
using StardewModdingAPI;
using StardewValley;
using xTile.Dimensions;

namespace SpaceCore.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Event"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming is determined by Harmony.")]
    internal class EventPatcher : BasePatcher
    {
        /*********
        ** Accessors
        *********/
        internal static Dictionary<string, MethodInfo> customCommands = new();


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Event>(nameof(Event.tryEventCommand)),
                prefix: this.GetHarmonyMethod(nameof(Before_TryEventCommand))
            );

            harmony.Patch(
                original: this.RequireMethod<Event>(nameof(Event.checkAction)),
                prefix: this.GetHarmonyMethod(nameof(Before_CheckAction))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="Event.tryEventCommand"/>.</summary>
        private static bool Before_TryEventCommand(Event __instance, GameLocation location, GameTime time, string[] split)
        {
            var _eventCommandArgs = SpaceCore.instance.Helper.Reflection.GetField<object[]>(typeof(Event), "_eventCommandArgs").GetValue();
            var _commandLookup = SpaceCore.instance.Helper.Reflection.GetField<Dictionary<string, MethodInfo>>(typeof(Event), "_commandLookup").GetValue();

            _eventCommandArgs[0] = (object)location;
            _eventCommandArgs[1] = (object)time;
            _eventCommandArgs[2] = (object)split;
            if (split.Length == 0)
                return false;
            if (EventPatcher.customCommands.ContainsKey(split[0]))
                EventPatcher.customCommands[split[0]].Invoke(null, new object[] { __instance, _eventCommandArgs[0], _eventCommandArgs[1], _eventCommandArgs[2] });
            else if (_commandLookup.ContainsKey(split[0]))
                _commandLookup[split[0]].Invoke((object)__instance, _eventCommandArgs);
            else
                SpaceShared.Log.warn("ERROR: Invalid command: " + split[0]);

            return false;
        }

        /// <summary>The method to call before <see cref="Event.checkAction"/>.</summary>
        private static bool Before_CheckAction(Event __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
        {
            var actionStr = Game1.currentLocation.doesTileHaveProperty(tileLocation.X, tileLocation.Y, "Action", "Buildings");
            if (actionStr != null)
                return !SpaceEvents.InvokeActionActivated(who, actionStr, tileLocation);
            return true;
        }
    }
}
