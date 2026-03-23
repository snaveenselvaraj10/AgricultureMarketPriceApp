namespace AgricultureMarketPriceApp.Models
{
    public class CommoditySummary
    {
        public string Commodity { get; set; }
        public double? AverageModalPrice { get; set; }
        public string Unit { get; set; }
        public double? PercentChange { get; set; }
        public string Icon { get; set; }
    }
}
