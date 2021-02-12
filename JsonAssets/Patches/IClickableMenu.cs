using Harmony;
using JsonAssets.PackData;
using SpaceShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace JsonAssets.Patches
{
    public static class DrawHoverTextPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler( ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns )
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
                    insn.operand = typeof( Common ).GetMethod( nameof( Common.GetFakeObjectInformation ) );
                }

                ret.Add( insn );
            }

            return ret;
        }
    }
}
