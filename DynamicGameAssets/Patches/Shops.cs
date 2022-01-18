using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DynamicGameAssets.Framework;
using HarmonyLib;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace DynamicGameAssets.Patches
{
    /// <summary>Applies Harmony patches to various shop stock methods.</summary>
    /// <remarks>This handles all shop stocks except <c>ResortBar</c> and <c>VolcanoShop</c> (which are in the menu changed event handler).</remarks>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class ShopPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            // night market
            harmony.Patch(
                original: this.RequireMethod<BeachNightMarket>(nameof(BeachNightMarket.getBlueBoatStock)),
                postfix: this.GetHarmonyMethod(nameof(After_BeachNightMarker_GetBlueBoatStock))
            );
            harmony.Patch(
                original: this.RequireMethod<BeachNightMarket>(nameof(BeachNightMarket.geMagicShopStock)),
                postfix: this.GetHarmonyMethod(nameof(After_BeachNightMarket_GeMagicShopStock))
            );

            // desert
            harmony.Patch(
                original: this.RequireMethod<Desert>(nameof(Desert.getDesertMerchantTradeStock)),
                postfix: this.GetHarmonyMethod(nameof(After_Desert_GetDesertMerchantTradeStock))
            );

            // event
            harmony.Patch(
                original: this.RequireMethod<Event>(nameof(Event.checkAction)),
                postfix: this.GetHarmonyMethod(nameof(After_Event_CheckAction))
            );

            // game location
            harmony.Patch(
                original: this.RequireMethod<GameLocation>("sandyShopStock"),
                postfix: this.GetHarmonyMethod(nameof(After_GameLocation_SandyShopStock))
            );
            harmony.Patch(
                original: this.RequireMethod<GameLocation>(nameof(GameLocation.performAction)),
                postfix: this.GetHarmonyMethod(nameof(After_GameLocation_PerformAction))
            );

            // island north
            harmony.Patch(
                original: this.RequireMethod<IslandNorth>(nameof(IslandNorth.getIslandMerchantTradeStock)),
                postfix: this.GetHarmonyMethod(nameof(After_IslandNorth_GetIslandMerchantTradeStock))
            );

            // seed shop
            harmony.Patch(
                original: this.RequireMethod<SeedShop>(nameof(SeedShop.shopStock)),
                postfix: this.GetHarmonyMethod(nameof(After_SeedShop_ShopStock))
            );

            // sewer
            harmony.Patch(
                original: this.RequireMethod<Sewer>(nameof(Sewer.getShadowShopStock)),
                postfix: this.GetHarmonyMethod(nameof(After_Sewer_GenerateKrobusStock))
            );

            // utility
            harmony.Patch(
                original: this.RequireMethod<Utility>("generateLocalTravelingMerchantStock"),
                postfix: this.GetHarmonyMethod(nameof(After_Utility_GenerateLocalTravelingMerchantStock))
            );
            harmony.Patch(
                original: this.RequireMethod<Utility>(nameof(Utility.getHatStock)),
                postfix: this.GetHarmonyMethod(nameof(After_Utility_GetHatStock))
            );
            harmony.Patch(
                original: this.RequireMethod<Utility>(nameof(Utility.GetQiChallengeRewardStock)),
                postfix: this.GetHarmonyMethod(nameof(After_Utility_GetQiChallengeRewardStock))
            );
            harmony.Patch(
                original: this.RequireMethod<Utility>(nameof(Utility.getJojaStock)),
                postfix: this.GetHarmonyMethod(nameof(After_Utility_GetJojaStock))
            );
            harmony.Patch(
                original: this.RequireMethod<Utility>(nameof(Utility.getHospitalStock)),
                postfix: this.GetHarmonyMethod(nameof(After_Utility_GetHospitalStock))
            );
            harmony.Patch(
                original: this.RequireMethod<Utility>(nameof(Utility.getQiShopStock)),
                postfix: this.GetHarmonyMethod(nameof(After_Utility_GetQiShopStock))
            );
            harmony.Patch(
                original: this.RequireMethod<Utility>(nameof(Utility.getFishShopStock)),
                postfix: this.GetHarmonyMethod(nameof(After_Utility_GetFishShopStock))
            );
            harmony.Patch(
                original: this.RequireMethod<Utility>(nameof(Utility.getSaloonStock)),
                postfix: this.GetHarmonyMethod(nameof(After_Utility_GetSaloonStock))
            );
            harmony.Patch(
                original: this.RequireMethod<Utility>(nameof(Utility.getAdventureShopStock)),
                postfix: this.GetHarmonyMethod(nameof(After_Utility_GetAdventureShopStock))
            );
            harmony.Patch(
                original: this.RequireMethod<Utility>(nameof(Utility.getCarpenterStock)),
                postfix: this.GetHarmonyMethod(nameof(After_Utility_GetCarpenterStock))
            );
            harmony.Patch(
                original: this.RequireMethod<Utility>(nameof(Utility.getAnimalShopStock)),
                postfix: this.GetHarmonyMethod(nameof(After_Utility_GetAnimalShopStock))
            );
            harmony.Patch(
                original: this.RequireMethod<Utility>(nameof(Utility.getBlacksmithStock)),
                postfix: this.GetHarmonyMethod(nameof(After_Utility_GetBlacksmithStock))
            );
            harmony.Patch(
                original: this.RequireMethod<Utility>(nameof(Utility.getDwarfShopStock)),
                postfix: this.GetHarmonyMethod(nameof(After_Utility_GetDwarfShopStock))
            );
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Night market
        ****/
        /// <summary>The method to call after <see cref="BeachNightMarket.getBlueBoatStock"/>.</summary>
        private static void After_BeachNightMarker_GetBlueBoatStock(Dictionary<ISalable, int[]> __result)
        {
            PatchCommon.DoShopStock("BlueBoat", __result);
        }

        /// <summary>The method to call after <see cref="BeachNightMarket.geMagicShopStock"/>.</summary>
        private static void After_BeachNightMarket_GeMagicShopStock(Dictionary<ISalable, int[]> __result)
        {
            PatchCommon.DoShopStock("GeMagic", __result);
        }

        /****
        ** Desert
        ****/
        /// <summary>The method to call after <see cref="Desert.getDesertMerchantTradeStock"/>.</summary>
        private static void After_Desert_GetDesertMerchantTradeStock(Dictionary<ISalable, int[]> __result)
        {
            PatchCommon.DoShopStock("DesertMerchant", __result);
        }

        /****
        ** Event
        ****/
        /// <summary>The method to call after <see cref="Event.checkAction"/>.</summary>
        private static void After_Event_CheckAction()
        {
            if (Game1.activeClickableMenu is ShopMenu shop)
                PatchCommon.DoShop($"Festival.{Game1.currentSeason}{Game1.dayOfMonth}", shop);
        }

        /****
        ** Game location
        ****/
        /// <summary>The method to call after <see cref="GameLocation.sandyShopStock"/>.</summary>
        private static void After_GameLocation_SandyShopStock(Dictionary<ISalable, int[]> __result)
        {
            PatchCommon.DoShopStock("Sandy", __result);
        }

        /// <summary>The method to call after <see cref="GameLocation.performAction"/>.</summary>
        private static void After_GameLocation_PerformAction(string action)
        {
            if ((action is "IceCreamStand" or "Theater_BoxOffice") && Game1.activeClickableMenu is ShopMenu shop)
                PatchCommon.DoShop(action, shop);
        }

        /****
        ** Island north
        ****/
        /// <summary>The method to call after <see cref="IslandNorth.getIslandMerchantTradeStock"/>.</summary>
        private static void After_IslandNorth_GetIslandMerchantTradeStock(Dictionary<ISalable, int[]> __result)
        {
            PatchCommon.DoShopStock("IslandMerchant", __result);
        }

        /****
        ** Seed shop
        ****/
        /// <summary>The method to call after <see cref="SeedShop.shopStock"/>.</summary>
        private static void After_SeedShop_ShopStock(Dictionary<ISalable, int[]> __result)
        {
            PatchCommon.DoShopStock("SeedShop", __result);
        }

        /****
        ** Sewer
        ****/
        /// <summary>The method to call after <see cref="Sewer.generateKrobusStock"/>.</summary>
        private static void After_Sewer_GenerateKrobusStock(Dictionary<ISalable, int[]> __result)
        {
            PatchCommon.DoShopStock("Krobus", __result);
        }

        /****
        ** Utility
        ****/
        /// <summary>The method to call after <see cref="Utility.generateLocalTravelingMerchantStock"/>.</summary>
        private static void After_Utility_GenerateLocalTravelingMerchantStock(Dictionary<ISalable, int[]> __result)
        {
            PatchCommon.DoShopStock("TravelingMerchant", __result);
        }

        /// <summary>The method to call after <see cref="Utility.getHatStock"/>.</summary>
        private static void After_Utility_GetHatStock(Dictionary<ISalable, int[]> __result)
        {
            PatchCommon.DoShopStock("HatMouse", __result);
        }

        /// <summary>The method to call after <see cref="Utility.GetQiChallengeRewardStock"/>.</summary>
        private static void After_Utility_GetQiChallengeRewardStock(Dictionary<ISalable, int[]> __result)
        {
            PatchCommon.DoShopStock("QiGemShop", __result);
        }

        /// <summary>The method to call after <see cref="Utility.getJojaStock"/>.</summary>
        private static void After_Utility_GetJojaStock(Dictionary<ISalable, int[]> __result)
        {
            PatchCommon.DoShopStock("Joja", __result);
        }

        /// <summary>The method to call after <see cref="Utility.getHospitalStock"/>.</summary>
        private static void After_Utility_GetHospitalStock(Dictionary<ISalable, int[]> __result)
        {
            PatchCommon.DoShopStock("Hospital", __result);
        }

        /// <summary>The method to call after <see cref="Utility.getQiShopStock"/>.</summary>
        private static void After_Utility_GetQiShopStock(Dictionary<ISalable, int[]> __result)
        {
            PatchCommon.DoShopStock("Club", __result);
        }

        /// <summary>The method to call after <see cref="Utility.getFishShopStock"/>.</summary>
        private static void After_Utility_GetFishShopStock(Dictionary<ISalable, int[]> __result)
        {
            PatchCommon.DoShopStock("FishShop", __result);
        }

        /// <summary>The method to call after <see cref="Utility.getSaloonStock"/>.</summary>
        private static void After_Utility_GetSaloonStock(Dictionary<ISalable, int[]> __result)
        {
            PatchCommon.DoShopStock("Saloon", __result);
        }

        /// <summary>The method to call after <see cref="Utility.getAdventureShopStock"/>.</summary>
        private static void After_Utility_GetAdventureShopStock(Dictionary<ISalable, int[]> __result)
        {
            PatchCommon.DoShopStock("AdventurerGuild", __result);
        }

        /// <summary>The method to call after <see cref="Utility.getCarpenterStock"/>.</summary>
        private static void After_Utility_GetCarpenterStock(Dictionary<ISalable, int[]> __result)
        {
            PatchCommon.DoShopStock("Carpenter", __result);
        }

        /// <summary>The method to call after <see cref="Utility.getAnimalShopStock"/>.</summary>
        private static void After_Utility_GetAnimalShopStock(Dictionary<ISalable, int[]> __result)
        {
            PatchCommon.DoShopStock("AnimalSupplies", __result);
        }

        /// <summary>The method to call after <see cref="Utility.getBlacksmithStock"/>.</summary>
        private static void After_Utility_GetBlacksmithStock(Dictionary<ISalable, int[]> __result)
        {
            PatchCommon.DoShopStock("Blacksmith", __result);
        }

        /// <summary>The method to call after <see cref="Utility.getDwarfShopStock"/>.</summary>
        private static void After_Utility_GetDwarfShopStock(Dictionary<ISalable, int[]> __result)
        {
            PatchCommon.DoShopStock("Dwarf", __result);
        }
    }
}
