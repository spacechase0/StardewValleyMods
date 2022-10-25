using System;
using System.Diagnostics.CodeAnalysis;

using HarmonyLib;

using Spacechase.Shared.Patching;

using SpaceCore.Framework.Extensions;

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
            // this is fixed in 1.6.
            if (new Version(1, 6) > new Version(Game1.version))
            {
                harmony.Patch(
                original: this.RequireConstructor<CraftingRecipe>(new Type[] { typeof(string), typeof(bool) }),
                postfix: this.GetHarmonyMethod(nameof(CraftingRecipe_Constructor_Postfix)));
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call after <see cref="CraftingRecipe"/> constructor.</summary>
        private static void CraftingRecipe_Constructor_Postfix(CraftingRecipe __instance, bool isCookingRecipe)
        {
            if (__instance.itemToProduce.Count <= 0 || LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en)
            {
                return;
            }

            // Location of display name depending on if it's a cooking or a crafting recipe
            int displayNameIndex = __instance.isCookingRecipe ? 4 : 5;


            if ((isCookingRecipe && CraftingRecipe.cookingRecipes.TryGetValue(__instance.name, out string data))
                || (!isCookingRecipe && CraftingRecipe.craftingRecipes.TryGetValue(__instance.name, out data)))
            {
                if (data.GetNthChunk('/', displayNameIndex).Length != 0)
                {
                    int index = data.LastIndexOf('/');
                    if (index > 0)
                    {
                        string possibleName = data[(index + 1)..];
                        if (possibleName.Length != 0)
                            __instance.DisplayName = possibleName;
                    }
                }
            }
        }
    }
}
