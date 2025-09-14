using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using DynamicGameAssets.Framework;
using HarmonyLib;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley.Menus;

namespace DynamicGameAssets.Patches
{
    /// <summary>Applies Harmony patches to <see cref="CollectionsPage"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class CollectionsPagePatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireConstructor<CollectionsPage>(typeof(int), typeof(int), typeof(int), typeof(int)),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_Constructor))
            );

            harmony.Patch(
                original: this.RequireMethod<CollectionsPage>(nameof(CollectionsPage.createDescription)),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_CreateDescription))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method which transpiles the <see cref="CollectionsPage"/> constructor.</summary>
        private static IEnumerable<CodeInstruction> Transpile_Constructor(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            instructions = PatchCommon.RedirectForFakeObjectInformationCollectionTranspiler(gen, original, instructions);

            bool foundRedirect = false;
            var ret = new List<CodeInstruction>();
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Call && (instruction.operand as MethodBase)!.Name == nameof(PatchCommon.GetFakeObjectInformationCollection))
                    foundRedirect = true;

                else if (foundRedirect && instruction.opcode == OpCodes.Callvirt && (instruction.operand as MethodInfo)!.Name == "Sort")
                {
                    Log.Trace("Found object sorting, replacing with ours");
                    foundRedirect = false;
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = PatchHelper.RequireMethod<CollectionsPagePatcher>(nameof(RealSort));
                }

                ret.Add(instruction);
            }

            return ret;
        }

        /// <summary>The method which transpiles <see cref="CollectionsPage.createDescription"/>.</summary>
        private static IEnumerable<CodeInstruction> Transpile_CreateDescription(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            return PatchCommon.RedirectForFakeObjectInformationTranspiler(gen, original, insns);
        }

        private static void RealSort(List<KeyValuePair<int, string>> list, object oldSorter)
        {
            list.Sort((a, b) =>
            {
                var aja = Mod.itemLookup.ContainsKey(a.Key) ? Mod.Find(Mod.itemLookup[a.Key]) : null;
                var bja = Mod.itemLookup.ContainsKey(b.Key) ? Mod.Find(Mod.itemLookup[b.Key]) : null;

                if (aja == null && bja == null)
                    return a.Key.CompareTo(b.Key);
                if (aja == null && bja != null)
                    return -1;
                if (aja != null && bja == null)
                    return 1;

                return $"{aja.pack.smapiPack.Manifest.UniqueID}/{aja.ID}".CompareTo($"{bja.pack.smapiPack.Manifest.UniqueID}/{bja.ID}");
            });
        }
    }

}
