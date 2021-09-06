using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Spacechase.Shared.Patching;
using SpaceCore.Events;
using SpaceCore.Framework;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using xTile.Dimensions;

namespace SpaceCore.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Event"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class CraftingRecipePatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<CraftingRecipe>(nameof(CraftingRecipe.consumeIngredients)),
                prefix: this.GetHarmonyMethod(nameof(Before_ConsumeIngredients))
            );
            harmony.Patch(
                original: this.RequireMethod<CraftingPage>("layoutRecipes"),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_LayoutRecipes))
            );
            harmony.Patch(
                original: this.RequireMethod<CollectionsPage>(nameof(CollectionsPage.createDescription)),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_MakeCraftingRecipe))
            );
        }

        public static CraftingRecipe RedirectedCreateRecipe(string name, bool isCooking)
        {
            var container = CustomCraftingRecipe.CraftingRecipes;
            if (isCooking)
                container = CustomCraftingRecipe.CookingRecipes;

            if (container.ContainsKey(name))
            {
                return new Framework.CustomCraftingRecipe(name, isCooking, container[name]);
            }

            return new CraftingRecipe(name, isCooking);
        }

        public static ClickableTextureComponent RedirectedCTCCreation(ClickableTextureComponent ctc, CraftingRecipe recipe )
        {
            if ( recipe is Framework.CustomCraftingRecipe ccr )
            {
                ctc.texture = ccr.recipe.IconTexture;
                ctc.sourceRect = ccr.recipe.IconSubrect ?? new Microsoft.Xna.Framework.Rectangle(0, 0, ctc.texture.Width, ctc.texture.Height);
            }

            return ctc;
        }

        /*********
        ** Private methods
        *********/
        private static bool Before_ConsumeIngredients(CraftingRecipe __instance, List<Chest> additional_materials)
        {
            if ( __instance is Framework.CustomCraftingRecipe ccr )
            {
                foreach ( var ingred in ccr.recipe.Ingredients )
                {
                    ingred.Consume(additional_materials);
                }
                return false;
            }

            return true;
        }

        private static IEnumerable<CodeInstruction> Transpile_LayoutRecipes(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            insns = Transpile_MakeCraftingRecipe(gen, original, insns);

            LocalBuilder recipeLocal = null;
            bool didIt = false;
            var newInsns = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                if (recipeLocal == null && insn.opcode == OpCodes.Ldloc_S && ( insn.operand as LocalBuilder ).LocalType == typeof( CraftingRecipe ) )
                {
                    recipeLocal = insn.operand as LocalBuilder;
                }
                else if (!didIt && insn.opcode == OpCodes.Ldloc_S && ( insn.operand as LocalBuilder ).LocalType == typeof( ClickableTextureComponent ) )
                {
                    Log.Trace($"Found first ldloc.s for ClickableTextureComponent in {original}; storing potential override w/ recipeLocal={recipeLocal}" );
                    newInsns.Add(new CodeInstruction(OpCodes.Ldloc_S, insn.operand));
                    newInsns.Add(new CodeInstruction(OpCodes.Ldloc_S, recipeLocal));
                    newInsns.Add(new CodeInstruction(OpCodes.Call, typeof(CraftingRecipePatcher).GetMethod(nameof(RedirectedCTCCreation))));
                    newInsns.Add(new CodeInstruction(OpCodes.Stloc_S, insn.operand));
                    newInsns.Add(insn);

                    didIt = true;
                    continue;
                }

                newInsns.Add(insn);
            }

            return newInsns;
        }

        private static IEnumerable<CodeInstruction> Transpile_MakeCraftingRecipe(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            var newInsns = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                if (insn.opcode == OpCodes.Newobj && insn.operand is ConstructorInfo cinfo && cinfo.DeclaringType == typeof( CraftingRecipe ) && cinfo.GetParameters().Length == 2 )
                {
                    Log.Trace($"Found crafting recipe constructor in {original}!");
                    insn.opcode = OpCodes.Call;
                    insn.operand = typeof(CraftingRecipePatcher).GetMethod(nameof(RedirectedCreateRecipe));
                }

                newInsns.Add(insn);
            }

            return newInsns;
        }
    }
}
