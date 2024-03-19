using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace SpaceCore.Patches
{
    /// <summary>Applies Harmony patches to <see cref="HoeDirt"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class HoeDirtPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<HoeDirt>(nameof(HoeDirt.dayUpdate)),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_DayUpdate))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method which transpiles <see cref="HoeDirt.dayUpdate"/> to remove the <see cref="HoeDirt.destroyCrop"/> call.</summary>
        /// <remarks>Withering isn't needed here since crops will wither separately if needed, so this simplifies Json Assets' winter crop logic elsewhere.</remarks>
        private static IEnumerable<CodeInstruction> Transpile_DayUpdate(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.MethodReplacer(
                from: PatchHelper.RequireMethod<HoeDirt>(nameof(HoeDirt.destroyCrop)),
                to: PatchHelper.RequireMethod<HoeDirtPatcher>(nameof(DestroyCrop))
            );
        }

        /// <summary>An implementation of <see cref="HoeDirt.destroyCrop"/> that does nothing.</summary>
        public void DestroyCrop(bool showAnimation) { }
    }
}
