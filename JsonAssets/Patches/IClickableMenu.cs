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
            return PatchCommon.RedirectForFakeObjectInformationTranspiler( gen, original, insns );
        }
    }
}
