using Harmony;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace LuckSkill.Overrides
{
    public static class ExperienceGainFix
    {
        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            // TODO: Learn how to use ILGenerator
            
            int skipCounter = 3; // Skip the first three instructions, which just skip things if it is the luck skill
            var newInsns = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                if ( skipCounter > 0 )
                {
                    --skipCounter;
                    continue;
                }

                newInsns.Add(insn);
            }

            return newInsns;
        }
    }
}
