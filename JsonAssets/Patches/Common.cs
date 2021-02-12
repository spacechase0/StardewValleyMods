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
    public static class Common
    {
        public static void DoShop( string key, ShopMenu shop )
        {
            if ( !Mod.todaysShopEntries.ContainsKey( key ) )
                return;

            foreach ( var entry in Mod.todaysShopEntries[ key ] )
            {
                entry.AddToShop( shop );
            }
        }

        public static void DoShopStock( string key, Dictionary<ISalable, int[]> data )
        {
            if ( !Mod.todaysShopEntries.ContainsKey( key ) )
                return;

            foreach ( var entry in Mod.todaysShopEntries[ key ] )
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
        public static IEnumerable<CodeInstruction> RedirectForFakeObjectInformationTranspiler( ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns )
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
                    insn.operand = typeof( Common ).GetMethod( nameof( Common.GetFakeObjectInformation2 ) );
                }

                ret.Add( insn );
            }

            foreach ( var insn in ret )
                Log.trace( "I:"+insn );

            return ret;
        }
    }
}
