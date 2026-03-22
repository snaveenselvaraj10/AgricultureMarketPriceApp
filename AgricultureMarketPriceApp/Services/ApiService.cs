using System.Net.Http.Json;
using AgricultureMarketPriceApp.Models;

namespace AgricultureMarketPriceApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _client;
        // Example API url - user will replace with real api endpoint
        private const string ApiUrl = "https://api.data.gov.in/resource/9ef84268-d588-465a-a308-a864a43d0070?format=json&limit=100";

        public ApiService()
        {
            _client = new HttpClient();
        }

        public async Task<List<PriceRecord>> GetDailyPricesAsync()
        {
            try
            {
                var resp = await _client.GetAsync(ApiUrl);
                if (!resp.IsSuccessStatusCode)
                    return new List<PriceRecord>();

                using var stream = await resp.Content.ReadAsStreamAsync();
                using var doc = await System.Text.Json.JsonDocument.ParseAsync(stream);
                if (!doc.RootElement.TryGetProperty("records", out var records))
                    return new List<PriceRecord>();

                var list = new List<PriceRecord>();
                foreach (var el in records.EnumerateArray())
                {
                    try
                    {
                        var rec = System.Text.Json.JsonSerializer.Deserialize<PriceRecord>(el.GetRawText());
                        if (rec != null)
                            list.Add(rec);
                    }
                    catch { }
                }

                return list;
            }
            catch
            {
                return new List<PriceRecord>();
            }
        }
    }
}
