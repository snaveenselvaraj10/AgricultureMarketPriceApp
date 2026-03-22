using System.Text.Json.Serialization;

namespace AgricultureMarketPriceApp.Models
{
    public class PriceRecord
    {
        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("district")]
        public string District { get; set; }

        [JsonPropertyName("market")]
        public string Market { get; set; }

        [JsonPropertyName("commodity")]
        public string Commodity { get; set; }

        [JsonPropertyName("variety")]
        public string Variety { get; set; }

        [JsonPropertyName("grade")]
        public string Grade { get; set; }

        [JsonPropertyName("arrival_date")]
        public string Arrival_Date { get; set; }

        [JsonPropertyName("min_price")]
        public double? Min_price { get; set; }

        [JsonPropertyName("max_price")]
        public double? Max_price { get; set; }

        [JsonPropertyName("modal_price")]
        public double? Modal_price { get; set; }
    }
}
