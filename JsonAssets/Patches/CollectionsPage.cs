using Harmony;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace JsonAssets.Patches
{
    [HarmonyPatch]//( typeof( CollectionsPage ) )]
    //[HarmonyPatch( new[] { typeof( int ), typeof( int ), typeof( int ), typeof( int ) } )]
    public static class CollectionsPageConstructorPatch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            return new List<MethodBase>( new[] { typeof( CollectionsPage ).GetConstructor( new[] { typeof( int ), typeof( int ), typeof( int ), typeof( int ) } ) } );
        }

        public static void RealSort( List<KeyValuePair<int, string>> list, object oldSorter )
        {
            list.Sort( (a, b) =>
            {
                var aja = Mod.itemLookup.ContainsKey( a.Key ) ? Mod.Find( Mod.itemLookup[ a.Key ] ) : null;
                var bja = Mod.itemLookup.ContainsKey( b.Key ) ? Mod.Find( Mod.itemLookup[ b.Key ] ) : null;

                if ( aja == null && bja == null )
                    return a.Key.CompareTo( b.Key );
                if ( aja == null && bja != null )
                    return -1;
                if ( aja != null && bja == null )
                    return 1;

                return $"{aja.parent.smapiPack.Manifest.UniqueID}/{aja.ID}".CompareTo( $"{bja.parent.smapiPack.Manifest.UniqueID}/{bja.ID}" );
            } );
        }

        public static IEnumerable<CodeInstruction> Transpiler( ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns )
        {
            insns = Common.RedirectForFakeObjectInformationCollectionTranspiler( gen, original, insns );

            bool foundRedirect = false;
            var ret = new List<CodeInstruction>();
            foreach ( var insn in insns )
            {
                if ( insn.opcode == OpCodes.Call && ( insn.operand as MethodBase ).Name == nameof( Common.GetFakeObjectInformationCollection ) )
                {
                    foundRedirect = true;
                }
                else if ( foundRedirect && insn.opcode == OpCodes.Callvirt && ( insn.operand as MethodInfo ).Name == "Sort" )
                {
                    foundRedirect = false;
                    insn.opcode = OpCodes.Call;
                    insn.operand = typeof( CollectionsPageConstructorPatch ).GetMethod( nameof( CollectionsPageConstructorPatch.RealSort ) );
                }

                ret.Add( insn );
            }

            return ret;
        }
    }

    [HarmonyPatch( typeof( CollectionsPage ), nameof( CollectionsPage.createDescription ) )]
    public static class CollectionsPageDescriptionPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler( ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns )
        {
            return Common.RedirectForFakeObjectInformationTranspiler( gen, original, insns );
        }
    }
}