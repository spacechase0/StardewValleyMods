using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TerrainFeatures;

namespace FruitTreeCapacityGrowth
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }
    }

    [HarmonyPatch( typeof( FruitTree ), nameof( FruitTree.draw ) )]
    public static class FruitTreeDrawPatch
    {
        public static Vector2 GetFruitOffset(Vector2 tilePos, int index)
        {
            var offset = FruitTreeShakePatch.GetFruitOffset(index);
            return Game1.GlobalToLocal(Game1.viewport, tilePos * Game1.tileSize + offset + new Vector2(0, -192));
        }

        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> ret = new();

            int drawCounts = 0;
            int lightningCounter = 0;
            foreach (var insn in instructions)
            {
                if (drawCounts > 0 && insn.opcode == OpCodes.Callvirt && insn.operand is MethodInfo mi && mi.DeclaringType == typeof( SpriteBatch ) && mi.Name == "Draw" )
                {
                    // TODO: Don't hardcode these local variable indices
                    int sourceRectIndex = 8;
                    int fruitIndexIndex = 6;

                    int sr = ret.Count - 1;
                    for (; sr >= 0; --sr)
                    {
                        if (ret[sr].opcode == OpCodes.Ldloc_S && (ret[sr].operand as LocalBuilder).LocalIndex == sourceRectIndex)
                        {
                            List<CodeInstruction> insert = new();
                            insert.Add(new CodeInstruction(OpCodes.Pop));
                            insert.Add(new CodeInstruction(OpCodes.Ldarg_2));
                            insert.Add(new CodeInstruction(OpCodes.Ldloc, fruitIndexIndex));
                            insert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FruitTreeDrawPatch), nameof(FruitTreeDrawPatch.GetFruitOffset))));
                            ret.InsertRange(sr, insert);
                            break;
                        }
                    }

                    --drawCounts;
                }
                else if (insn.opcode == OpCodes.Ldfld && (insn.operand as FieldInfo).Name == nameof(FruitTree.struckByLightningCountdown))
                {
                    if ( ++lightningCounter == 4 )
                        drawCounts = 3;
                }
                ret.Add(insn);
            }

            return ret;
        }

        public static void Postfix(FruitTree __instance, SpriteBatch spriteBatch, Vector2 tileLocation)
        {
            for (int i = 3; i < __instance.fruit.Count; i++)
            {
                ParsedItemData obj = (((int)__instance.struckByLightningCountdown > 0) ? ItemRegistry.GetDataOrErrorItem("(O)382") : ItemRegistry.GetDataOrErrorItem(__instance.fruit[i].QualifiedItemId));
                Texture2D texture = obj.GetTexture();
                Rectangle sourceRect = obj.GetSourceRect();
                spriteBatch.Draw(texture, GetFruitOffset( tileLocation, i ), sourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)__instance.getBoundingBox(tileLocation).Bottom / 10000f + 0.002f - tileLocation.X / 1000000f);
            }
        }
    }

    [HarmonyPatch(typeof(FruitTree), nameof(FruitTree.shake))]
    public static class FruitTreeShakePatch
    {
        public static Vector2 GetFruitOffset(int index)
        {
            Vector2[] fruits = new[]
            {
                new Vector2( -32,  48 ),
                new Vector2(  32,  48 ),
                new Vector2( -64,   0 ),
                new Vector2(   0,   0 ),
                new Vector2(  64,   0 ),
                new Vector2( -32, -48 ),
                new Vector2(  32, -48 ),
            };
            return index < fruits.Length ? fruits[index] : fruits[ 0 ];
        }

        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> ret = new();

            foreach (var insn in instructions)
            {
                if (insn.opcode == OpCodes.Ldfld && (insn.operand as FieldInfo).Name == nameof( FruitTree.struckByLightningCountdown ) )
                {
                    List<CodeInstruction> insert = new();
                    insert.Add(new CodeInstruction(OpCodes.Ldloc, (ret[ret.Count - 4].operand as LocalBuilder).LocalIndex - 1)); // Kinda shaky assuming the local right before the vector2 is j like we want but whatever
                    insert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FruitTreeShakePatch), nameof(FruitTreeShakePatch.GetFruitOffset))));
                    insert.Add(new CodeInstruction(OpCodes.Stloc, ret[ret.Count - 4].operand));
                    ret.InsertRange(ret.Count - 1, insert);
                }
                ret.Add(insn);
            }

            return ret;
        }
    }

    [HarmonyPatch(typeof(FruitTree), nameof(FruitTree.performToolAction))]
    public static class FruitTreeToolActionPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            return FruitTreeShakePatch.Transpiler(gen, original, instructions);
        }
    }

    [HarmonyPatch(typeof(FruitTree), nameof(FruitTree.dayUpdate))]
    public static class FruitTreeDayUpdatePatch
    {
        public static void Postfix(FruitTree __instance)
        {
            if (!__instance.stump && __instance.daysUntilMature < -112 * 3 )
            {
                int additionalTries = Math.Min( __instance.daysUntilMature / -112 + 3, 6);
                for (int i = 0; i < additionalTries; ++i)
                {
                    __instance.TryAddFruit();
                }
            }
        }
    }

    [HarmonyPatch(typeof(FruitTree), nameof(FruitTree.TryAddFruit))]
    public static class FruitTreeTryAddFruitPatch
    {
        public static int GetCapacityOfFruitTree(FruitTree ftree)
        {
            if (ftree.daysUntilMature.Value >= 0)
                return 0;

            return Math.Min( 3 + ftree.daysUntilMature.Value / -28, 7 );
        }

        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> ret = new();

            bool next = false;
            foreach (var insn in instructions)
            {
                if (next)
                {
                    ret.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    insn.opcode = OpCodes.Call;
                    insn.operand = AccessTools.Method(typeof(FruitTreeTryAddFruitPatch), nameof(GetCapacityOfFruitTree));
                    next = false;
                }
                else if (insn.opcode == OpCodes.Callvirt && (insn.operand as MethodInfo).Name == "get_Count")
                {
                    next = true;
                }
                ret.Add(insn);
            }

            return ret;
        }
    }
}
