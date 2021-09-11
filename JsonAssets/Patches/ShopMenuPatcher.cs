using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley.Menus;

namespace JsonAssets.Patches
{
    /// <summary>Applies Harmony patches to <see cref="ShopMenu"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class ShopMenuPatcher : BasePatcher
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The last owner for which a <see cref="ShopMenu"/> was constructed.</summary>
        public static string LastShopOwner { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<ShopMenu>(nameof(ShopMenu.setUpShopOwner)),
                prefix: this.GetHarmonyMethod(nameof(Before_SetUpShopOwner))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="ShopMenu.setUpShopOwner"/>.</summary>
        private static void Before_SetUpShopOwner(string who)
        {
            ShopMenuPatcher.LastShopOwner = who;
        }
    }
}
