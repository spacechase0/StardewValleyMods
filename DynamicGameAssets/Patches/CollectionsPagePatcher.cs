using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using DynamicGameAssets.Framework;
using HarmonyLib;
using SpaceShared;
using StardewValley.Menus;

namespace DynamicGameAssets.Patches
{
    [HarmonyPatch]//( typeof( CollectionsPage ) )]
    //[HarmonyPatch( new[] { typeof( int ), typeof( int ), typeof( int ), typeof( int ) } )]
    public static class CollectionsPageConstructorPatch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            return new List<MethodBase>(new[] { typeof(CollectionsPage).GetConstructor(new[] { typeof(int), typeof(int), typeof(int), typeof(int) }) });
        }

        public static void RealSort(List<KeyValuePair<int, string>> list, object oldSorter)
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

        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            insns = PatchCommon.RedirectForFakeObjectInformationCollectionTranspiler(gen, original, insns);

            bool foundRedirect = false;
            var ret = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                if (insn.opcode == OpCodes.Call && (insn.operand as MethodBase).Name == nameof(PatchCommon.GetFakeObjectInformationCollection))
                {
                    foundRedirect = true;
                }
                else if (foundRedirect && insn.opcode == OpCodes.Callvirt && (insn.operand as MethodInfo).Name == "Sort")
                {
                    Log.Trace("Found object sorting, replacing with ours");
                    foundRedirect = false;
                    insn.opcode = OpCodes.Call;
                    insn.operand = typeof(CollectionsPageConstructorPatch).GetMethod(nameof(CollectionsPageConstructorPatch.RealSort));
                }

                ret.Add(insn);
            }

            return ret;
        }
    }

    [HarmonyPatch(typeof(CollectionsPage), nameof(CollectionsPage.createDescription))]
    public static class CollectionsPageDescriptionPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            return PatchCommon.RedirectForFakeObjectInformationTranspiler(gen, original, insns);
        }
    }
}
