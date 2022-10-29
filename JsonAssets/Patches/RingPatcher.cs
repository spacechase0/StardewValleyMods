using System;
using System.Diagnostics.CodeAnalysis;

using HarmonyLib;

using Spacechase.Shared.Patching;

using SpaceShared;

using StardewModdingAPI;

using StardewValley.Objects;

namespace JsonAssets.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Ring"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class RingPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Ring>("loadDisplayFields"),
                finalizer: this.GetHarmonyMethod(nameof(Finalize_LoadDisplayFields))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>A finalizer for <see cref="Ring.loadDisplayFields"/>.</summary>
        public static Exception Finalize_LoadDisplayFields(Ring __instance, Exception __exception)
        {
            if (__exception is not null)
                Log.Error($"Failed in loading display fields for rings for #{__instance?.indexInTileSheet}:\n{__exception}");
            return null;
        }
    }
}
