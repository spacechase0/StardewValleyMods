using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Force.DeepCloner;
using HarmonyLib;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Delegates;
using StardewValley.Extensions;
using StardewValley.GameData.Crops;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace SpaceCore.VanillaAssetExpansion
{
    public class FruitTreeExtensionData
    {
        public List<Vector2> FruitLocations { get; set; } = new();
    }

    [HarmonyPatch(typeof(FruitTree), nameof(FruitTree.draw))]
    public static class FruitTreeDrawFruitPatch
    {
        public static Vector2 GetFruitOffset(Vector2 tilePos, FruitTree __instance, int index)
        {
            var dict = Game1.content.Load<Dictionary<string, FruitTreeExtensionData>>("spacechase0.SpaceCore/FruitTreeExtensionData");

            Vector2 offset = GetFruitOffset(index);

            if (dict.TryGetValue(__instance.treeId.Value, out var ftData) && ftData.FruitLocations != null)
            {
                offset = ftData.FruitLocations[index];
            }

            return Game1.GlobalToLocal(Game1.viewport, tilePos * Game1.tileSize + offset + new Vector2(0, -192));
        }

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
            return index < fruits.Length ? fruits[index] : fruits[0];
        }

        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> ret = new();

            int drawCounts = 0;
            int lightningCounter = 0;
            foreach (var insn in instructions)
            {
                if (drawCounts > 0 && insn.opcode == OpCodes.Callvirt && insn.operand is MethodInfo mi && mi.DeclaringType == typeof(SpriteBatch) && mi.Name == "Draw")
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
                            insert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FruitTreeDrawFruitPatch), nameof(FruitTreeDrawFruitPatch.GetFruitOffset))));
                            ret.InsertRange(sr, insert);
                            break;
                        }
                    }

                    --drawCounts;
                }
                else if (insn.opcode == OpCodes.Ldfld && (insn.operand as FieldInfo).Name == nameof(FruitTree.struckByLightningCountdown))
                {
                    if (++lightningCounter == 4)
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
                ParsedItemData obj = (((int)__instance.struckByLightningCountdown.Value > 0) ? ItemRegistry.GetDataOrErrorItem("(O)382") : ItemRegistry.GetDataOrErrorItem(__instance.fruit[i].QualifiedItemId));
                Texture2D texture = obj.GetTexture();
                Rectangle sourceRect = obj.GetSourceRect();
                spriteBatch.Draw(texture, GetFruitOffset(tileLocation, __instance, i), sourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)__instance.getBoundingBox().Bottom / 10000f + 0.002f - tileLocation.X / 1000000f);
            }
        }
    }
}