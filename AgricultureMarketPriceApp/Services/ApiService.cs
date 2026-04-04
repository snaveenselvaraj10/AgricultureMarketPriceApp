using System.Net.Http.Json;
using AgricultureMarketPriceApp.Models;

namespace AgricultureMarketPriceApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _client;
        public string LastRequest { get; private set; }
        private const string BaseApiUrl = "https://api.data.gov.in/resource/35985678-0d79-46b4-9ed6-6f13308a1d24";
        // Default API key (provided by user). You can override by passing apiKey to the methods.
        private const string DefaultApiKey = "579b464db66ec23bdd0000015349a47eb21342e65d32b8c385aeb5e0";

        public ApiService()
        {
            _client = new HttpClient();
            // Ensure API returns JSON and some servers require a User-Agent
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            if (_client.DefaultRequestHeaders.UserAgent.Count == 0)
                _client.DefaultRequestHeaders.UserAgent.ParseAdd("AgricultureMarketPriceApp/1.0");
        }

        // Constructor that accepts an existing HttpClient for testing or advanced scenarios
        public ApiService(HttpClient client)
        {
            _client = client ?? new HttpClient();
            // Ensure sensible defaults when a test/client doesn't set headers
            try
            {
                if (_client.DefaultRequestHeaders.Accept.Count == 0)
                    _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                if (_client.DefaultRequestHeaders.UserAgent.Count == 0)
                    _client.DefaultRequestHeaders.UserAgent.ParseAdd("AgricultureMarketPriceApp/1.0");
            }
            catch
            {
                // ignore header setup failures in constrained environments
            }
        }

        // Backwards-compatible: fetch default set (no filters, no api-key)
        public Task<List<PriceRecord>> GetDailyPricesAsync()
            => GetDailyPricesAsync(apiKey: null, state: null, commodity: null, limit: 100);

        // New API: pass your apiKey and optional state/commodity filters.
        // filters[state.keyword] and filters[commodity] are used by the data.gov.in API.
        public async Task<List<PriceRecord>> GetDailyPricesAsync(string apiKey, string state = null, string commodity = null, string district = null, int limit = 100)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(apiKey))
                    apiKey = DefaultApiKey;
                var url = new System.Text.StringBuilder();
                url.Append(BaseApiUrl);
                // API returns JSON. Request JSON format and parse accordingly.
                url.Append("?format=json");
                url.Append("&limit=").Append(limit);

                if (!string.IsNullOrWhiteSpace(state))
                {
                    // API expects the field names with their exact casing (State, District, Commodity)
                    // Use filters[State] to match the data.gov.in examples.
                    url.Append("&filters[State]=").Append(Uri.EscapeDataString(state));
                }

                if (!string.IsNullOrWhiteSpace(commodity))
                {
                    url.Append("&filters[Commodity]=").Append(Uri.EscapeDataString(commodity));
                }

                if (!string.IsNullOrWhiteSpace(district))
                {
                    url.Append("&filters[District]=").Append(Uri.EscapeDataString(district));
                }
                // apiKey will always be set (falls back to DefaultApiKey if not provided)
                url.Append("&api-key=").Append(Uri.EscapeDataString(apiKey));
                var requestUrl = url.ToString();
                LastRequest = requestUrl;
#if DEBUG
                System.Diagnostics.Debug.WriteLine("ApiService GET: " + requestUrl);
#endif
                var resp = await _client.GetAsync(requestUrl);
                if (!resp.IsSuccessStatusCode)
                    return new List<PriceRecord>();

                var content = await resp.Content.ReadAsStringAsync();
#if DEBUG
                System.Diagnostics.Debug.WriteLine("ApiService Response (truncated): " + (content?.Length > 1000 ? content.Substring(0, 1000) + "..." : content));
#endif
                using var doc = System.Text.Json.JsonDocument.Parse(content);
                if (!doc.RootElement.TryGetProperty("records", out var records))
                    return new List<PriceRecord>();

                var list = new List<PriceRecord>();
                foreach (var el in records.EnumerateArray())
                {
                    try
                    {
                        string GetString(System.Text.Json.JsonElement e, params string[] names)
                        {
                            foreach (var n in names)
                            {
                                if (e.TryGetProperty(n, out var v) && v.ValueKind != System.Text.Json.JsonValueKind.Null)
                                    return v.GetString();
                            }
                            return null;
                        }

                        string sState = GetString(el, "State", "state");
                        string sDistrict = GetString(el, "District", "district");
                        string sMarket = GetString(el, "Market", "market");
                        string sCommodity = GetString(el, "Commodity", "commodity");
                        string sVariety = GetString(el, "Variety", "variety");
                        string sGrade = GetString(el, "Grade", "grade");
                        string sArrival = GetString(el, "Arrival_Date", "arrival_date");
                        string sMinp = GetString(el, "Min_Price", "min_price");
                        string sMaxp = GetString(el, "Max_Price", "max_price");
                        string sModp = GetString(el, "Modal_Price", "modal_price");
                        string sCommcode = GetString(el, "Commodity_Code", "commodity_code");

                        var rec = new PriceRecord
                        {
                            State = sState,
                            District = sDistrict,
                            Market = sMarket,
                            Commodity = sCommodity,
                            Variety = sVariety,
                            Grade = sGrade,
                            Arrival_Date = sArrival,
                            Commodity_Code = sCommcode
                        };

                        if (double.TryParse(sMinp, out var minv))
                            rec.Min_price = minv;
                        if (double.TryParse(sMaxp, out var maxv))
                            rec.Max_price = maxv;
                        if (double.TryParse(sModp, out var modv))
                            rec.Modal_price = modv;

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

        // Overloads that accept enums for convenience (include district)
        public Task<List<PriceRecord>> GetDailyPricesAsync(string apiKey, StateEnum state, DistrictEnum district, CommodityEnum commodity, int limit = 100)
            => GetDailyPricesAsync(apiKey: apiKey, state: state.ToApiState(), commodity: commodity.ToApiCommodity(), district: district.ToApiDistrict(), limit: limit);

        public Task<List<PriceRecord>> GetDailyPricesAsync(StateEnum state, DistrictEnum district, CommodityEnum commodity, int limit = 100)
            => GetDailyPricesAsync(apiKey: null, state: state.ToApiState(), commodity: commodity.ToApiCommodity(), district: district.ToApiDistrict(), limit: limit);

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
                System.Xml.Linq.XDocument xdoc;
                try
                {
                    xdoc = System.Xml.Linq.XDocument.Parse(content);
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
