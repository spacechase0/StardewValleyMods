using Harmony;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonAssets.Patches
{
    // The following are for shop stock - all cases but ResortBar and VolcanoShop
    // Those two are in the menu changed event handler

    [HarmonyPatch(typeof(BeachNightMarket), nameof(BeachNightMarket.getBlueBoatStock))]
    public static class BlueBoatStockPatch
    {
        public static void Postfix( Dictionary<ISalable, int[]> __result )
        {
            PatchCommon.DoShopStock( "BlueBoat", __result );
        }
    }

    [HarmonyPatch(typeof(Utility), "generateLocalTravelingMerchantStock" )]
    public static class TravelingMerchantStockPatch
    {
        public static void Postfix( Dictionary<ISalable, int[]> __result )
        {
            PatchCommon.DoShopStock( "TravelingMerchant", __result );
        }
    }

    [HarmonyPatch( typeof( BeachNightMarket ), nameof( BeachNightMarket.geMagicShopStock ) )]
    public static class GeMagicStockPatch
    {
        public static void Postfix( Dictionary<ISalable, int[]> __result )
        {
            PatchCommon.DoShopStock( "GeMagic", __result );
        }
    }

    [HarmonyPatch( typeof( Desert ), nameof( Desert.getDesertMerchantTradeStock ) )]
    public static class DesertMerchantStockPatch
    {
        public static void Postfix( Dictionary<ISalable, int[]> __result )
        {
            PatchCommon.DoShopStock( "DesertMerchant", __result );
        }
    }

    [HarmonyPatch( typeof( Utility ), nameof( Utility.getHatStock ) )]
    public static class HatMouseStockPatch
    {
        public static void Postfix( Dictionary<ISalable, int[]> __result )
        {
            PatchCommon.DoShopStock( "HatMouse", __result );
        }
    }

    [HarmonyPatch( typeof( IslandNorth ), nameof( IslandNorth.getIslandMerchantTradeStock ) )]
    public static class IslandMerchantStockPatch
    {
        public static void Postfix( Dictionary<ISalable, int[]> __result )
        {
            PatchCommon.DoShopStock( "IslandMerchant", __result );
        }
    }

    [HarmonyPatch( typeof( Sewer ), "generateKrobusStock" )]
    public static class KrobusStockPatch
    {
        public static void Postfix( Dictionary<ISalable, int[]> __result )
        {
            PatchCommon.DoShopStock( "IslandMerchant", __result );
        }
    }

    [HarmonyPatch( typeof( Utility ), nameof( Utility.GetQiChallengeRewardStock ) )]
    public static class QiGemShopStockPatch
    {
        public static void Postfix( Dictionary<ISalable, int[]> __result )
        {
            PatchCommon.DoShopStock( "QiGemShop", __result );
        }
    }

    [HarmonyPatch( typeof( Utility ), nameof( Utility.getJojaStock ) )]
    public static class JojaStockPatch
    {
        public static void Postfix( Dictionary<ISalable, int[]> __result )
        {
            PatchCommon.DoShopStock( "Joja", __result );
        }
    }

    [HarmonyPatch( typeof( Utility ), nameof( Utility.getHospitalStock ) )]
    public static class HospitalStockPatch
    {
        public static void Postfix( Dictionary<ISalable, int[]> __result )
        {
            PatchCommon.DoShopStock( "Hospital", __result );
        }
    }

    [HarmonyPatch( typeof( Utility ), nameof( Utility.getQiShopStock ) )]
    public static class ClubStockPatch
    {
        public static void Postfix( Dictionary<ISalable, int[]> __result )
        {
            PatchCommon.DoShopStock( "Club", __result );
        }
    }

    [HarmonyPatch( typeof( Utility ), nameof( Utility.getFishShopStock ) )]
    public static class FishShopStockPatch
    {
        public static void Postfix( Dictionary<ISalable, int[]> __result )
        {
            PatchCommon.DoShopStock( "FishShop", __result );
        }
    }

    [HarmonyPatch( typeof( SeedShop ), nameof( SeedShop.shopStock ) )]
    public static class SeedShopStockPatch
    {
        public static void Postfix( Dictionary<ISalable, int[]> __result )
        {
            PatchCommon.DoShopStock( "SeedShop", __result );
        }
    }

    [HarmonyPatch( typeof( GameLocation ), "sandyShopStock" )]
    public static class SandyStock
    {
        public static void Postfix( Dictionary<ISalable, int[]> __result )
        {
            PatchCommon.DoShopStock( "Sandy", __result );
        }
    }

    [HarmonyPatch( typeof( Utility ), nameof( Utility.getSaloonStock ) )]
    public static class SaloonStockPatch
    {
        public static void Postfix( Dictionary<ISalable, int[]> __result )
        {
            PatchCommon.DoShopStock( "Saloon", __result );
        }
    }

    [HarmonyPatch( typeof( Utility ), nameof( Utility.getAdventureShopStock ) )]
    public static class AdventurerGuildStockPatch
    {
        public static void Postfix( Dictionary<ISalable, int[]> __result )
        {
            PatchCommon.DoShopStock( "AdventurerGuild", __result );
        }
    }

    [HarmonyPatch( typeof( Utility ), nameof( Utility.getAdventureShopStock ) )]
    public static class CarpenterStockPatch
    {
        public static void Postfix( Dictionary<ISalable, int[]> __result )
        {
            PatchCommon.DoShopStock( "Carpenter", __result );
        }
    }

    [HarmonyPatch( typeof( Utility ), nameof( Utility.getAdventureShopStock ) )]
    public static class AnimalSuppliesStockPatch
    {
        public static void Postfix( Dictionary<ISalable, int[]> __result )
        {
            PatchCommon.DoShopStock( "AnimalSupplies", __result );
        }
    }

    [HarmonyPatch( typeof( Utility ), nameof( Utility.getAdventureShopStock ) )]
    public static class BlacksmithStockPatch
    {
        public static void Postfix( Dictionary<ISalable, int[]> __result )
        {
            PatchCommon.DoShopStock( "Blacksmith", __result );
        }
    }

    [HarmonyPatch( typeof( Utility ), nameof( Utility.getAdventureShopStock ) )]
    public static class DwarfStockPatch
    {
        public static void Postfix( Dictionary<ISalable, int[]> __result )
        {
            PatchCommon.DoShopStock( "Dwarf", __result );
        }
    }

    [HarmonyPatch( typeof( GameLocation ), nameof( GameLocation.performAction ) )]
    public static class GLPAStockPatch
    {
        public static void Postfix(string action)
        {
            if ( ( action == "IceCreamStand" || action == "Theater_BoxOffice" ) &&Game1.activeClickableMenu is ShopMenu shop )
            {
                PatchCommon.DoShop( action, shop );
            }
        }
    }

    [HarmonyPatch( typeof( Event ), nameof( Event.checkAction ) )]
    public static class FestivalStockPatch
    {
        public static void Postfix()
        {
            if ( Game1.activeClickableMenu is ShopMenu shop )
            {
                PatchCommon.DoShop( $"Festival.{Game1.currentSeason}{Game1.dayOfMonth}", shop );
            }
        }
    }
}
