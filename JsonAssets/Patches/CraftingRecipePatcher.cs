using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JsonAssets.Data;
using Netcode;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace JsonAssets.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Crop"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class CraftingRecipePatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            // Correctly assign display name field for CraftingRecipe instances in English locale
            harmony.Patch(
                original: AccessTools.Constructor(type: typeof(StardewValley.CraftingRecipe), parameters: new Type[] { typeof(string), typeof(bool) }),
                postfix: this.GetHarmonyMethod(nameof(CraftingRecipe_Constructor_Postfix)));
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call after <see cref="CraftingRecipe"/> constructor.</summary>
        private static void CraftingRecipe_Constructor_Postfix(StardewValley.CraftingRecipe __instance, bool isCookingRecipe)
        {
            // Determine if the item produced by the recipe is a JA item
            bool isJAContent;
            if (__instance.itemToProduce.Count <= 0)
            {
                isJAContent = false;
            }
            else
            {
                isJAContent = Mod.instance.ObjectIds.Values.Contains(__instance.itemToProduce[0]);
                if (isJAContent)
                {
                    Log.Debug($"Found JA Recipe with produced item ID {__instance.itemToProduce[0]}");
                    foreach (var kvp in Mod.instance.ObjectIds)
                    {
                        if (kvp.Value == __instance.itemToProduce[0])
                        {
                            Log.Debug($"Corresponding to item {kvp.Key}");
                        }
                    }
                }
            }

            // Location of display name depending on if it's a cooking or a crafting recipe
            int displayNameIndex = __instance.isCookingRecipe ? 4 : 5;

            // Check that we're only affecting JA content and only in English
            if (isJAContent && LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en) {
                // If it's a cooking recipe, grab the data and set display name
                if (isCookingRecipe && StardewValley.CraftingRecipe.cookingRecipes.TryGetValue(__instance.name, out string data)) {
                    if (data.Split('/') is string[] split && split.Length >= displayNameIndex)
                    {
                        __instance.DisplayName = split.Last();
                        Log.Debug($"Setting display name to {split.Last()}");
                    }
                }
                // If it's a cooking recipe, grab the data and set display name
                if (!isCookingRecipe && StardewValley.CraftingRecipe.craftingRecipes.TryGetValue(__instance.name, out data))
                {
                    if (data.Split('/') is string[] split && split.Length >= displayNameIndex)
                    {
                        __instance.DisplayName = split.Last();
                        Log.Debug($"Setting display name to {split.Last()}");
                    }
                }
            }
        }
    }
}