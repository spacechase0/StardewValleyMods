using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
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
                prefix: this.GetHarmonyMethod(nameof(Before_LoadDisplayFields))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="Ring.loadDisplayFields"/>.</summary>
        public static bool Before_LoadDisplayFields(Ring __instance, ref bool __result)
        {
            // So I just realized this is exactly the same as the vanilla method.
            // The key here though is we try/catch around it when something fails, but don't let the vanilla method run still
            // This way if it fails (like for some reason equipped custom rings do for farmhands when first connecting),
            //  the game will still run.
            try
            {
                if (Game1.objectInformation == null || __instance.indexInTileSheet.Value == null)
                {
                    __result = false;
                    return false;
                }
                string[] strArray = Game1.objectInformation[__instance.indexInTileSheet.Value].Split('/');
                __instance.displayName = strArray[4];
                __instance.description = strArray[5];
                __result = true;
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed in {nameof(Before_LoadDisplayFields)} for #{__instance?.indexInTileSheet}:\n{ex}");
                return false;
            }
        }
    }
}
