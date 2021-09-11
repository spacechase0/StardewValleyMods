using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace ExtendedReach.Patches
{
    /// <summary>Applies player radius checks throughout the game code to increase the tile radius.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class TileRadiusPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            var methods = new[]
            {
                this.RequireMethod<Game1>(nameof(Game1.pressActionButton)),
                this.RequireMethod<Game1>(nameof(Game1.pressUseToolButton)),
                this.RequireMethod<Game1>(nameof(Game1.tryToCheckAt)),
                this.RequireMethod<GameLocation>(nameof(GameLocation.isActionableTile)),
                this.RequireMethod<Utility>(nameof(Utility.canGrabSomethingFromHere)),
                this.RequireMethod<Utility>(nameof(Utility.checkForCharacterInteractionAtTile))
            };

            foreach (var method in methods)
            {
                harmony.Patch(
                    original: method,
                    transpiler: this.GetHarmonyMethod(nameof(TranspileRadiusChecks))
                );
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method which transpiles methods to increase the player's reach radius.</summary>
        private static IEnumerable<CodeInstruction> TranspileRadiusChecks(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            // TODO: Learn how to use ILGenerator

            var newInsns = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                if (insn.opcode == OpCodes.Call && insn.operand is MethodInfo meth)
                {
                    if (meth.Name is "withinRadiusOfPlayer" or "tileWithinRadiusOfPlayer")
                    {
                        var newInsn = new CodeInstruction(OpCodes.Ldc_I4, 100);
                        Log.Trace($"Found {meth.Name}, replacing {newInsns[newInsns.Count - 2]} with {newInsn}");
                        newInsns[newInsns.Count - 2] = newInsn;
                    }
                }
                newInsns.Add(insn);
            }

            return newInsns;
        }
    }
}
