namespace AgricultureMarketPriceApp.Models
{
    public class PriceRecord
    {
        public string State { get; set; }

        public string District { get; set; }

        public string Market { get; set; }

        public string Commodity { get; set; }

        public string Variety { get; set; }

        public string Grade { get; set; }

        public string Arrival_Date { get; set; }

        public double? Min_price { get; set; }

        public double? Max_price { get; set; }

        public double? Modal_price { get; set; }

        public string Commodity_Code { get; set; }
    }
}
