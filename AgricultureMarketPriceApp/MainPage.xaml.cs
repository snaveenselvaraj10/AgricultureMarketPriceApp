using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgricultureMarketPriceApp.Services;

namespace AgricultureMarketPriceApp
{
    public partial class MainPage : ContentPage
    {
        private readonly Services.ApiService _apiService;

        public MainPage(Services.ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;

            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object? sender, EventArgs e)
        {
            // Populate pickers with API-facing display strings (from enum mappings)
            StatePicker.ItemsSource = Enum.GetValues(typeof(Services.StateEnum))
                .Cast<Services.StateEnum>()
                .Select(s => s.ToApiState() ?? s.ToString())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            // start with empty district list; populate when state changes
            DistrictPicker.ItemsSource = new List<string>();

            StatePicker.SelectedIndexChanged += (s, ev) =>
            {
                if (StatePicker.SelectedIndex < 0)
                {
                    DistrictPicker.ItemsSource = new List<string>();
                    return;
                }

                var selected = StatePicker.Items[StatePicker.SelectedIndex];
                // Find corresponding StateEnum by matching ToApiState or enum name
                var matched = Enum.GetValues(typeof(Services.StateEnum))
                    .Cast<Services.StateEnum>()
                    .FirstOrDefault(se => string.Equals(se.ToApiState(), selected, StringComparison.OrdinalIgnoreCase)
                                          || string.Equals(se.ToString(), selected, StringComparison.OrdinalIgnoreCase));

                if (matched == null || matched.Equals(default(Services.StateEnum)))
                {
                    DistrictPicker.ItemsSource = new List<string>();
                    return;
                }

                // Get district list from enum helper
                var districts = matched.GetDistrictsForState();
                DistrictPicker.ItemsSource = districts;
            };

            CommodityPicker.ItemsSource = Enum.GetValues(typeof(Services.CommodityEnum))
                .Cast<Services.CommodityEnum>()
                .Select(c => c.ToApiCommodity() ?? c.ToString())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            await LoadSummariesAsync();
        }

        private async Task LoadSummariesAsync(string state = null, string district = null, string commodity = null)
        {
            var latest = await _apiService.GetDailyPricesAsync(apiKey: null, state: state, commodity: commodity, district: district, limit: 500);
            if (latest == null || !latest.Any())
            {
                PricesCollection.ItemsSource = new List<Models.CommoditySummary>();
                return;
            }

            var groups = latest.GroupBy(r => r.Commodity).Select(g => new Models.CommoditySummary
            {
                Commodity = g.Key,
                AverageModalPrice = g.Average(x => x.Modal_price ?? 0),
                Unit = string.Empty,
                Icon = "dotnet_bot.png"
            }).ToList();

            var dates = latest.Select(r => r.Arrival_Date).Distinct().ToList();
            if (dates.Count >= 2)
            {
                var parsed = dates.Select(d => DateTime.TryParseExact(d, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var dt) ? dt : (DateTime?)null)
                    .Where(x => x.HasValue).Select(x => x.Value).OrderByDescending(x => x).ToList();

                if (parsed.Count >= 2)
                {
                    var prevDate = parsed[1].ToString("dd/MM/yyyy");
                    var prevRecords = await _apiService.GetDailyPricesAsync(apiKey: null, state: state, commodity: commodity, district: district, limit: 500);
                    var prevGroups = prevRecords.Where(r => r.Arrival_Date == prevDate).GroupBy(r => r.Commodity)
                        .ToDictionary(g => g.Key, g => g.Average(x => x.Modal_price ?? 0));

                    foreach (var s in groups)
                    {
                        if (prevGroups.TryGetValue(s.Commodity, out var prevAvg) && prevAvg > 0)
                            s.PercentChange = (s.AverageModalPrice - prevAvg) / prevAvg;
                        else
                            s.PercentChange = 0;
                    }
                }
            }

            PricesCollection.ItemsSource = groups.OrderByDescending(g => g.AverageModalPrice).ToList();
        }

        private async void OnApplyFiltersClicked(object? sender, EventArgs e)
        {
            string state = null;
            string district = null;
            string commodity = null;
            if (StatePicker.SelectedIndex >= 0)
                state = StatePicker.Items[StatePicker.SelectedIndex];
            if (DistrictPicker.SelectedIndex >= 0)
                district = DistrictPicker.Items[DistrictPicker.SelectedIndex];
            if (CommodityPicker.SelectedIndex >= 0)
                commodity = CommodityPicker.Items[CommodityPicker.SelectedIndex];

            // call enum overload if all selected are enums
            if (!string.IsNullOrWhiteSpace(state) && !string.IsNullOrWhiteSpace(district) && !string.IsNullOrWhiteSpace(commodity) &&
                Enum.TryParse<Services.StateEnum>(state, out var se) && Enum.TryParse<Services.DistrictEnum>(district, out var de) && Enum.TryParse<Services.CommodityEnum>(commodity, out var ce))
            {
                var records = await _apiService.GetDailyPricesAsync(se, de, ce, limit: 500);
                // reuse logic from LoadSummariesAsync by temporarily setting data
                var groups = records.GroupBy(r => r.Commodity).Select(g => new Models.CommoditySummary
                {
                    Commodity = g.Key,
                    AverageModalPrice = g.Average(x => x.Modal_price ?? 0),
                    Unit = string.Empty,
                    Icon = "dotnet_bot.png"
                }).ToList();

                PricesCollection.ItemsSource = groups.OrderByDescending(g => g.AverageModalPrice).ToList();
                return;
            }

            await LoadSummariesAsync(state, commodity: commodity, district: district);
        }
    }
}
