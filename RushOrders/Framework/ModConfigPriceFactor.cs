namespace RushOrders.Framework
{
    internal class ModConfigPriceFactor
    {
        public ModConfigPriceFactorForTools Tool { get; set; } = new();
        public ModConfigPriceFactorForBuildings Building { get; set; } = new();
    }
}
