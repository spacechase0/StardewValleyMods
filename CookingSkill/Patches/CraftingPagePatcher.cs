using System.Diagnostics.CodeAnalysis;
using Harmony;
using Spacechase.Shared.Harmony;
using SpaceCore;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using SObject = StardewValley.Object;

namespace CookingSkill.Patches
{
    /// <summary>Applies Harmony patches to <see cref="CraftingPage"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class CraftingPagePatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<CraftingPage>("clickCraftingRecipe"),
                prefix: this.GetHarmonyMethod(nameof(Before_ClickCraftingRecipe))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="CraftingPage.clickCraftingRecipe"/>.</summary>
        /// <returns>Returns whether to skip the original method.</returns>
        /// <remarks>This is copied verbatim from the original method with some changes (marked with comments).</remarks>
        public static bool Before_ClickCraftingRecipe(CraftingPage __instance, ClickableTextureComponent c, bool playSound, ref int ___currentCraftingPage, ref Item ___heldItem, ref bool ___cooking)
        {
            // TODO:
            // - handle Qi seasoning
            // - compare with latest game code to see if anything else changed

            CraftingPage menu = __instance;

            Item crafted = menu.pagesOfCraftingRecipes[___currentCraftingPage][c].createItem();

            // custom code begins
            bool consume = Mod.OnCook(menu.pagesOfCraftingRecipes[___currentCraftingPage][c], crafted, menu._materialContainers);
            SObject itemObj = crafted as SObject;
            bool didCraft = false;
            // custom code ends

            Game1.player.checkForQuestComplete(null, -1, -1, crafted, null, 2);
            if (___heldItem == null)
            {
                // custom code begins
                if (consume)
                    menu.pagesOfCraftingRecipes[___currentCraftingPage][c].consumeIngredients(menu._materialContainers);
                didCraft = true;
                // custom code ends

                ___heldItem = crafted;
                if (playSound)
                    Game1.playSound("coin");
            }
            else if (___heldItem.Name.Equals(crafted.Name) && ___heldItem.Stack + menu.pagesOfCraftingRecipes[___currentCraftingPage][c].numberProducedPerCraft - 1 < ___heldItem.maximumStackSize())
            {
                ___heldItem.Stack += menu.pagesOfCraftingRecipes[___currentCraftingPage][c].numberProducedPerCraft;

                // custom code begins
                if (consume)
                    menu.pagesOfCraftingRecipes[___currentCraftingPage][c].consumeIngredients(menu._materialContainers);
                didCraft = true;
                // custom code ends

                if (playSound)
                    Game1.playSound("coin");
            }

            // custom code begins
            if (!didCraft)
                return true;
            // custom code ends

            if (!___cooking && Game1.player.craftingRecipes.ContainsKey(menu.pagesOfCraftingRecipes[___currentCraftingPage][c].name))
                Game1.player.craftingRecipes[menu.pagesOfCraftingRecipes[___currentCraftingPage][c].name] += menu.pagesOfCraftingRecipes[___currentCraftingPage][c].numberProducedPerCraft;
            if (___cooking)
            {
                Game1.player.cookedRecipe(___heldItem.ParentSheetIndex);

                // custom code begins
                Game1.player.AddCustomSkillExperience(Mod.Skill, itemObj.Edibility);
                // custom code ends
            }
            if (!___cooking)
                Game1.stats.checkForCraftingAchievements();
            else
                Game1.stats.checkForCookingAchievements();
            if (!Game1.options.gamepadControls || ___heldItem == null || !Game1.player.couldInventoryAcceptThisItem(___heldItem))
                return true;
            Game1.player.addItemToInventoryBool(___heldItem);
            ___heldItem = null;

            return true;
        }
    }
}
