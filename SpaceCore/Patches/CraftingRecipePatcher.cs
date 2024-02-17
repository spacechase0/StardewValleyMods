using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Objects;

namespace SpaceCore.Patches
{
    /// <summary>Applies Harmony patches to <see cref="CraftingRecipe"/>.</summary>
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
                prefix: this.GetHarmonyMethod(nameof(Before_CraftingRecipe_ConsumeIngredients))
            );
            if(Constants.TargetPlatform != GamePlatform.Android)
            {
                harmony.Patch(
                    original: this.RequireMethod<CraftingPage>("layoutRecipes"),
                    transpiler: this.GetHarmonyMethod(nameof(Transpile_CraftingPage_LayoutRecipes))
                );
            }
            else
            {
                harmony.Patch(
                    original: this.RequireMethod<CraftingPage>("setupRecipes"),
                    transpiler: this.GetHarmonyMethod(nameof(Transpile_CraftingPage_SetupRecipes))
                );
            }
            harmony.Patch(
                original: this.RequireMethod<CollectionsPage>(nameof(CollectionsPage.createDescription)),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_CollectionsPage_CreateDescription))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="CraftingRecipe.consumeIngredients"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_CraftingRecipe_ConsumeIngredients(CraftingRecipe __instance, List<IInventory> additionalMaterials)
        {
            if (__instance is Framework.CustomCraftingRecipe ccr)
            {
                foreach (var ingredient in ccr.recipe.Ingredients)
                {
                    ingredient.Consume(additionalMaterials);
                }
                return false;
            }

            return true;
        }

        /// <summary>The method which transpiles <see cref="CraftingPage.layoutRecipes"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static IEnumerable<CodeInstruction> Transpile_CraftingPage_LayoutRecipes(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            instructions = CraftingRecipePatcher.Transpile_CollectionsPage_CreateDescription(gen, original, instructions);

            LocalBuilder recipeLocal = null;
            bool didIt = false;
            var newInstructions = new List<CodeInstruction>();
            foreach (var instruction in instructions)
            {
                if (recipeLocal == null && instruction.opcode == OpCodes.Ldloc_S && (instruction.operand as LocalBuilder).LocalType == typeof(CraftingRecipe))
                {
                    recipeLocal = instruction.operand as LocalBuilder;
                }
                else if (!didIt && instruction.opcode == OpCodes.Ldloc_S && (instruction.operand as LocalBuilder).LocalType == typeof(ClickableTextureComponent))
                {
                    Log.Trace($"Found first ldloc.s for ClickableTextureComponent in {original}; storing potential override w/ recipeLocal={recipeLocal}");
                    newInstructions.Add(new CodeInstruction(OpCodes.Ldloc_S, instruction.operand));
                    newInstructions.Add(new CodeInstruction(OpCodes.Ldloc_S, recipeLocal));
                    newInstructions.Add(new CodeInstruction(OpCodes.Call, PatchHelper.RequireMethod<CraftingRecipePatcher>(nameof(RedirectedCTCCreation))));
                    newInstructions.Add(new CodeInstruction(OpCodes.Stloc_S, instruction.operand));
                    newInstructions.Add(instruction);

                    didIt = true;
                    continue;
                }

                newInstructions.Add(instruction);
            }

            return newInstructions;
        }

        /// <summary>The method which transpiles <see cref="CraftingPage.layoutRecipes"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static IEnumerable<CodeInstruction> Transpile_CraftingPage_SetupRecipes(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            instructions = CraftingRecipePatcher.Transpile_CollectionsPage_CreateDescription(gen, original, instructions);

            LocalBuilder recipeLocal = null;
            bool didIt = false;
            var newInstructions = new List<CodeInstruction>();
            foreach (var instruction in instructions)
            {
                if (recipeLocal == null && instruction.opcode == OpCodes.Ldloc_S && (instruction.operand as LocalBuilder).LocalType == typeof(CraftingRecipe))
                {
                    recipeLocal = instruction.operand as LocalBuilder;
                }
                else if (!didIt && instruction.opcode == OpCodes.Newobj && instruction.operand is ConstructorInfo constructor && constructor.DeclaringType == typeof(ClickableTextureComponent))
                {
                    Log.Trace($"Found first newobj for ClickableTextureComponent in {original}; storing potential override w/ recipeLocal={recipeLocal}");
                    newInstructions.Add(instruction);
                    newInstructions.Add(new CodeInstruction(OpCodes.Ldloc_S, recipeLocal));
                    newInstructions.Add(new CodeInstruction(OpCodes.Call, PatchHelper.RequireMethod<CraftingRecipePatcher>(nameof(RedirectedCTCCreation))));

                    didIt = true;
                    continue;
                }

                newInstructions.Add(instruction);
            }

            return newInstructions;
        }

        /// <summary>The method which transpiles <see cref="CollectionsPage.createDescription"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static IEnumerable<CodeInstruction> Transpile_CollectionsPage_CreateDescription(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            var newInstructions = new List<CodeInstruction>();
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Newobj && instruction.operand is ConstructorInfo constructor && constructor.DeclaringType == typeof(CraftingRecipe) && constructor.GetParameters().Length == 2)
                {
                    Log.Trace($"Found crafting recipe constructor in {original}!");
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = PatchHelper.RequireMethod<CraftingRecipePatcher>(nameof(RedirectedCreateRecipe));
                }

                newInstructions.Add(instruction);
            }

            return newInstructions;
        }

        private static CraftingRecipe RedirectedCreateRecipe(string name, bool isCooking)
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

        private static ClickableTextureComponent RedirectedCTCCreation(ClickableTextureComponent ctc, CraftingRecipe recipe)
        {
            if (recipe is Framework.CustomCraftingRecipe ccr)
            {
                ctc.texture = ccr.recipe.IconTexture;
                ctc.sourceRect = ccr.recipe.IconSubrect ?? new Microsoft.Xna.Framework.Rectangle(0, 0, ctc.texture.Width, ctc.texture.Height);
            }

            return ctc;
        }
    }
}
