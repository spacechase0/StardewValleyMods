using Harmony;
using JsonAssets.Game;
using JsonAssets.PackData;
using SpaceShared;
using StardewValley;
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
    public static class PatchCommon
    {
        public static void DoShop( string key, ShopMenu shop )
        {
            if ( !Mod.State.TodaysShopEntries.ContainsKey( key ) )
                return;

            foreach ( var entry in Mod.State.TodaysShopEntries[ key ] )
            {
                entry.AddToShop( shop );
            }
        }

        public static void DoShopStock( string key, Dictionary<ISalable, int[]> data )
        {
            if ( !Mod.State.TodaysShopEntries.ContainsKey( key ) )
                return;

            foreach ( var entry in Mod.State.TodaysShopEntries[ key ] )
            {
                entry.AddToShopStock( data );
            }
        }

        public static string GetFakeObjectInformation( IDictionary<int, string> data, int index )
        {
            if ( Mod.itemLookup.ContainsKey( index ) )
            {
                return ( ( ObjectPackData ) Mod.Find( Mod.itemLookup[ index ] ) ).GetFakeData();
            }
            return data[ index ];
        }

        public static string GetFakeObjectInformation2( IDictionary<int, string> objInfo, StardewValley.Object obj )
        {
            if ( obj is CustomObject jobj )
            {
                return jobj.Data.GetFakeData();
            }
            else
            {
                return objInfo[ obj.ParentSheetIndex ];
            }
        }

        public static int GetFakeObjectId( StardewValley.Object obj )
        {
            if ( obj is CustomObject cobj )
            {
                return cobj.FullId.GetHashCode();
            }
            return obj.ParentSheetIndex;
        }

        public static IDictionary< int,string> GetFakeObjectInformationCollection()
        {
            var ret = new Dictionary<int , string >( Game1.objectInformation );
            foreach ( var cp in Mod.contentPacks )
            {
                foreach ( var data in cp.Value.items )
                {
                    if ( data.Value is ObjectPackData objData )
                    {
                        ret.Add( $"{cp.Key}/{data.Key}".GetHashCode(), objData.GetFakeData() );
                    }
                }
            }

            return ret;
        }
        public static IEnumerable<CodeInstruction> RedirectForFakeObjectInformationTranspiler( ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns )
        {
            var ret = new List< CodeInstruction >();

            bool nextGetItem = true;
            foreach ( var insn in insns )
            {
                if ( insn.opcode == OpCodes.Ldsfld && ( insn.operand as FieldInfo ).Name == "objectInformation" )
                {
                    nextGetItem = true;
                }
                else if ( nextGetItem && insn.opcode == OpCodes.Callvirt && ( insn.operand as MethodInfo ).Name == "get_Item" )
                {
                    nextGetItem = false;
                    Log.trace( "Found the object information get call, redirecting..." );
                    insn.opcode = OpCodes.Call;
                    insn.operand = typeof( PatchCommon ).GetMethod( nameof( PatchCommon.GetFakeObjectInformation ) );
                }

                ret.Add( insn );
            }

            return ret;
        }

        public static IEnumerable<CodeInstruction> RedirectForFakeObjectInformationTranspiler2( ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns )
        {
            var ret = new List< CodeInstruction >();

            bool foundObjInfo = false;
            foreach ( var insn in insns )
            {
                if ( insn.opcode == OpCodes.Ldsfld && ( insn.operand as FieldInfo ).Name == "objectInformation" )
                {
                    foundObjInfo = true;
                }
                else if ( foundObjInfo &&
                          ( ret.Last().opcode == OpCodes.Callvirt && ( ret.Last().operand as MethodInfo ).Name == "get_ParentSheetIndex" ) &&
                          ( insn.opcode == OpCodes.Callvirt && ( insn.operand as MethodInfo ).Name == "get_Item" ) )
                {
                    foundObjInfo = false;
                    Log.trace( "Found object information reference in " + original + ", editing IL now" );

                    insn.labels.AddRange( ret.Last().labels );
                    ret.Remove( ret.Last() );

                    insn.opcode = OpCodes.Call;
                    insn.operand = typeof( PatchCommon ).GetMethod( nameof( PatchCommon.GetFakeObjectInformation2 ) );
                }

                ret.Add( insn );
            }

            return ret;
        }

        public static IEnumerable<CodeInstruction> RedirectForFakeObjectInformationCollectionTranspiler( ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns )
        {
            var ret = new List< CodeInstruction >();

            foreach ( var insn in insns )
            {
                if ( insn.opcode == OpCodes.Ldsfld && ( insn.operand as FieldInfo ).Name == "objectInformation" )
                {
                    Log.trace( "Found object information reference in " + original + ", editing IL now" );

                    insn.opcode = OpCodes.Call;
                    insn.operand = typeof( PatchCommon ).GetMethod( nameof( PatchCommon.GetFakeObjectInformationCollection ) );
                }

                ret.Add( insn );
            }

            return ret;
        }

        public static IEnumerable<CodeInstruction> RedirectForFakeObjectIdTranspiler( ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns )
        {
            var ret = new List< CodeInstruction >();

            int actIn = -1;
            foreach ( var insn in insns )
            {
                if ( insn.opcode == OpCodes.Ldfld && ( insn.operand as FieldInfo ).Name == "parentSheetIndex" )
                {
                    actIn = 1;
                }
                if ( actIn >= 0 )
                {
                    if ( actIn-- == 0 )
                    {
                        Log.trace( "Found parentSheetIndex reference in " + original + ", editing IL now" );

                        insn.labels.AddRange( ret.Last().labels );
                        ret.Remove( ret.Last() );

                        insn.opcode = OpCodes.Call;
                        insn.operand = typeof( PatchCommon ).GetMethod( nameof( PatchCommon.GetFakeObjectId ) );
                    }
                }

                ret.Add( insn );
            }

            return ret;
        }
    }
}
