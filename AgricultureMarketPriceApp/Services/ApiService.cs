using System.Net.Http.Json;
using System.Xml.Linq;
using AgricultureMarketPriceApp.Models;

namespace AgricultureMarketPriceApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _client;
        private const string BaseApiUrl = "https://api.data.gov.in/resource/35985678-0d79-46b4-9ed6-6f13308a1d24";
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
                // API now returns XML. Request XML format and parse accordingly.
                url.Append("?format=xml");
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
                // Parse XML response and map <records><item> elements to PriceRecord
                var xdoc = XDocument.Load(stream);
                var recordsEl = xdoc.Root?.Element("records");
                if (recordsEl == null)
                    return new List<PriceRecord>();

                var list = new List<PriceRecord>();
                foreach (var item in recordsEl.Elements("item"))
                {
                    try
                    {
                        var rec = new PriceRecord
                        {
                            State = (string?)item.Element("State") ?? (string?)item.Element("state"),
                            District = (string?)item.Element("District") ?? (string?)item.Element("district"),
                            Market = (string?)item.Element("Market") ?? (string?)item.Element("market"),
                            Commodity = (string?)item.Element("Commodity") ?? (string?)item.Element("commodity"),
                            Variety = (string?)item.Element("Variety") ?? (string?)item.Element("variety"),
                            Grade = (string?)item.Element("Grade") ?? (string?)item.Element("grade"),
                            Arrival_Date = (string?)item.Element("Arrival_Date") ?? (string?)item.Element("arrival_date"),
                        };

                        // Parse numeric price fields (may be missing or non-numeric)
                        if (double.TryParse((string?)item.Element("Min_Price") ?? (string?)item.Element("min_price"), out var minv))
                            rec.Min_price = minv;

                        if (double.TryParse((string?)item.Element("Max_Price") ?? (string?)item.Element("max_price"), out var maxv))
                            rec.Max_price = maxv;

                        if (double.TryParse((string?)item.Element("Modal_Price") ?? (string?)item.Element("modal_price"), out var modv))
                            rec.Modal_price = modv;

                        list.Add(rec);
                    }
                    catch
                    {
                        // ignore malformed item and continue
                    }
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
                // Request XML format for the updated API response
                url.Append("?format=xml");
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

                // Parse XML content and extract <records><item> entries
                XDocument xdoc;
                try
                {
                    xdoc = XDocument.Parse(content);
                }
                catch (Exception ex)
                {
                    return ApiResult<List<PriceRecord>>.FromError((int)resp.StatusCode, "InvalidXml", content, ex.Message);
                }

                var recordsEl = xdoc.Root?.Element("records");
                if (recordsEl == null)
                    return ApiResult<List<PriceRecord>>.FromError((int)resp.StatusCode, "NoRecords", content, "Response did not contain records element");

                var list = new List<PriceRecord>();
                foreach (var item in recordsEl.Elements("item"))
                {
                    try
                    {
                        var rec = new PriceRecord
                        {
                            State = (string?)item.Element("State") ?? (string?)item.Element("state"),
                            District = (string?)item.Element("District") ?? (string?)item.Element("district"),
                            Market = (string?)item.Element("Market") ?? (string?)item.Element("market"),
                            Commodity = (string?)item.Element("Commodity") ?? (string?)item.Element("commodity"),
                            Variety = (string?)item.Element("Variety") ?? (string?)item.Element("variety"),
                            Grade = (string?)item.Element("Grade") ?? (string?)item.Element("grade"),
                            Arrival_Date = (string?)item.Element("Arrival_Date") ?? (string?)item.Element("arrival_date"),
                            Commodity_Code = (string?)item.Element("Commodity_Code") ?? (string?)item.Element("commodity_code"),
                        };

                        if (double.TryParse((string?)item.Element("Min_Price") ?? (string?)item.Element("min_price"), out var minv))
                            rec.Min_price = minv;

                        if (double.TryParse((string?)item.Element("Max_Price") ?? (string?)item.Element("max_price"), out var maxv))
                            rec.Max_price = maxv;

                        if (double.TryParse((string?)item.Element("Modal_Price") ?? (string?)item.Element("modal_price"), out var modv))
                            rec.Modal_price = modv;

                        list.Add(rec);
                    }
                    catch
                    {
                        // ignore malformed item
                    }
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
