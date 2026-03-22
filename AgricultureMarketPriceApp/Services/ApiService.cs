using System.Net.Http.Json;
using AgricultureMarketPriceApp.Models;

namespace AgricultureMarketPriceApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _client;
        private const string BaseApiUrl = "https://api.data.gov.in/resource/9ef84268-d588-465a-a308-a864a43d0070";
        // Default API key (provided by user). You can override by passing apiKey to the methods.
        private const string DefaultApiKey = "579b464db66ec23bdd0000015349a47eb21342e65d32b8c385aeb5e0";

        public ApiService()
        {
            _client = new HttpClient();
        }

        // Backwards-compatible: fetch default set (no filters, no api-key)
        public Task<List<PriceRecord>> GetDailyPricesAsync()
            => GetDailyPricesAsync(apiKey: null, state: null, commodity: null, limit: 100);

        // New API: pass your apiKey and optional state/commodity filters.
        // filters[state.keyword] and filters[commodity] are used by the data.gov.in API.
        public async Task<List<PriceRecord>> GetDailyPricesAsync(string apiKey, string state = null, string commodity = null, int limit = 100)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(apiKey))
                    apiKey = DefaultApiKey;
                var url = new System.Text.StringBuilder();
                url.Append(BaseApiUrl);
                url.Append("?format=json");
                url.Append("&limit=").Append(limit);

                if (!string.IsNullOrWhiteSpace(state))
                {
                    url.Append("&filters[state.keyword]=").Append(Uri.EscapeDataString(state));
                }

                if (!string.IsNullOrWhiteSpace(commodity))
                {
                    url.Append("&filters[commodity]=").Append(Uri.EscapeDataString(commodity));
                }

                // apiKey will always be set (falls back to DefaultApiKey if not provided)
                url.Append("&api-key=").Append(Uri.EscapeDataString(apiKey));

                var resp = await _client.GetAsync(url.ToString());
                if (!resp.IsSuccessStatusCode)
                    return new List<PriceRecord>();

                await using var stream = await resp.Content.ReadAsStreamAsync();
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

        // Overloads that accept enums for convenience
        public Task<List<PriceRecord>> GetDailyPricesAsync(string apiKey, StateEnum state, CommodityEnum commodity, int limit = 100)
            => GetDailyPricesAsync(apiKey, state.ToApiState(), commodity.ToApiCommodity(), limit);

        public Task<List<PriceRecord>> GetDailyPricesAsync(StateEnum state, CommodityEnum commodity, int limit = 100)
            => GetDailyPricesAsync(apiKey: null, state: state.ToApiState(), commodity: commodity.ToApiCommodity(), limit: limit);

        // Return diagnostics with HTTP details
        public async Task<ApiResult<List<PriceRecord>>> GetDailyPricesWithDiagnosticsAsync(string apiKey, string state = null, string commodity = null, int limit = 100)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(apiKey))
                    apiKey = DefaultApiKey;
                var url = new System.Text.StringBuilder();
                url.Append(BaseApiUrl);
                url.Append("?format=json");
                url.Append("&limit=").Append(limit);

                if (!string.IsNullOrWhiteSpace(state))
                    url.Append("&filters[state.keyword]=").Append(Uri.EscapeDataString(state));

                if (!string.IsNullOrWhiteSpace(commodity))
                    url.Append("&filters[commodity]=").Append(Uri.EscapeDataString(commodity));

                // apiKey will always be set (falls back to DefaultApiKey if not provided)
                url.Append("&api-key=").Append(Uri.EscapeDataString(apiKey));

                var resp = await _client.GetAsync(url.ToString());

                var content = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                    return ApiResult<List<PriceRecord>>.FromError((int)resp.StatusCode, resp.ReasonPhrase, content);

                using var doc = System.Text.Json.JsonDocument.Parse(content);
                if (!doc.RootElement.TryGetProperty("records", out var records))
                    return ApiResult<List<PriceRecord>>.FromError((int)resp.StatusCode, "NoRecords", content, "Response did not contain records");

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

                return ApiResult<List<PriceRecord>>.FromSuccess(list);
            }
            catch (Exception ex)
            {
                return ApiResult<List<PriceRecord>>.FromError(null, "Exception", ex.ToString(), ex.Message);
            }
        }
    }
}
