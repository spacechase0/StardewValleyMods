using HarmonyLib;
using StardewValley;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using StardewValley.Tools;
using System.Reflection.Emit;

namespace MoreRings
{
    public partial class ModEntry
    {

        /// <summary>
        /// Replacement for <see cref="ModEntry.OnItemEaten"/> to avoid limit prequisite APIs
        /// </summary>
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.doneEating))]
        public class Farmer_doneEating_Patch
        {
            public static void Postfix()
            {
                if (HasRingEquipped("Ring_of_Diamond_Booze"))
                {
                    if (Game1.player.hasBuff(Buff.tipsy))
                    {
                        Game1.player.buffs.Remove(Buff.tipsy);
                    }
                }
            }
        }

        /// <summary>
        /// Replacement for AxePatcher.Before_DoFunction because I am just not familiar with that Harmony Patch formatting lol
        /// </summary>
        [HarmonyPatch(typeof(Axe), nameof(Axe.DoFunction))]
        public class Axe_DoFunction_Patch
        {
            public static bool Prefix(ref int x, ref int y, Farmer who)
            {
                if (HasRingEquipped("Ring_of_Far_Reaching"))
                {
                    x = (int)who.lastClick.X;
                    y = (int)who.lastClick.Y;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Hoe), nameof(Hoe.DoFunction))]
        public class Hoe_DoFunction_Patch
        {
            public static bool Prefix(ref int x, ref int y, Farmer who)
            {
                if (HasRingEquipped("Ring_of_Far_Reaching"))
                {
                    x = (int)who.lastClick.X;
                    y = (int)who.lastClick.Y;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Pickaxe), nameof(Pickaxe.DoFunction))]
        public class Pickaxe_DoFunction_Patch
        {
            public static bool Prefix(ref int x, ref int y, Farmer who)
            {
                if (HasRingEquipped("Ring_of_Far_Reaching"))
                {
                    x = (int)who.lastClick.X;
                    y = (int)who.lastClick.Y;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(WateringCan), nameof(WateringCan.DoFunction))]
        public class WateringCan_DoFunction_Patch
        {
            public static bool Prefix(ref int x, ref int y, Farmer who)
            {
                if (HasRingEquipped("Ring_of_Far_Reaching"))
                {
                    x = (int)who.lastClick.X;
                    y = (int)who.lastClick.Y;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Crop), nameof(Crop.harvest))]
        public class Crop_harvest_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                Log("Transpiling Crop.harvest", debugOnly: true);
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 3 && codes[i].opcode == OpCodes.Ldc_I4_0 && codes[i + 1].opcode == OpCodes.Call && codes[i + 2].opcode == OpCodes.Pop)
                    {
                        LogOnce("Replacing Game1.createItemDebris with method", debugOnly: true);
                        codes.RemoveAt(i + 1);
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(Game1_createItemDebris))));
                        continue;
                    }
                    if (i < codes.Count - 3 && codes[i].opcode == OpCodes.Ldc_I4_0 && codes[i + 1].opcode == OpCodes.Callvirt && codes[i + 2].opcode == OpCodes.Brfalse)
                    {
                        LogOnce("Replacing Game1.player.addItemToInventoryBool with method", debugOnly: true);
                        codes.RemoveAt(i + 1);
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(Farmer_addItemToInventoryBool))));
                        continue;
                    }
                    if (i >= codes.Count) break;
                }
                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(Game1), nameof(Game1.pressUseToolButton))]
        public class Game1_pressToolUseButton_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                Log("Transpiling Game1.pressToolUseButton", debugOnly: true);
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count + 4 && codes[i].opcode == OpCodes.Ldc_I4_1 && codes[i + 1].opcode == OpCodes.Call && codes[i + 2].opcode == OpCodes.Call && codes[i + 3].opcode == OpCodes.Brfalse_S)
                    {
                        LogOnce("Replacing Utility.withinRadiusOfPlayer parameter with custom value", debugOnly: true);
                        codes.RemoveAt(i);
                        codes.Insert(i, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.Utility_withinRadiusOfPlayer))));
                    }
                }
                return codes.AsEnumerable();
            }
        }
    }
}
