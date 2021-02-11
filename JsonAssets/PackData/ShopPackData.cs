using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonAssets.PackData
{
    public class ShopPackData : BasePackData
    {
        /*
        BlueBoat
        TravelingMerchant
        GeMagic
        DesertMerchant
        HatMouse
        IslandMerchant
        ResortBar
        Krobus
        VolcanoShop
        Festival.<date> ie. Festival.summer5
        QiGemShop
        Joja
        IceCreamStand
        Hospital
        Club
        Theater_BoxOffice
        FishShop
        SeedShop
        Sandy
        Saloon
        AdventurerGuild
        Carpenter
        AnimalSupplies
        Blacksmith
        Dwarf
        */
        public string ShopId { get; set; }

        public string Item { get; set; }
        public int MaxSold { get; set; } = int.MaxValue;

        public int Cost { get; set; }
        public string Currency { get; set; }
    }
}
