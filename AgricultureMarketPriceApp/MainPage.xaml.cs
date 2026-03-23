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
            // Load available states/commodities for pickers - for now populate from enums
            StatePicker.ItemsSource = Enum.GetNames(typeof(Services.StateEnum));
            CommodityPicker.ItemsSource = Enum.GetNames(typeof(Services.CommodityEnum));

            await LoadSummariesAsync();
        }

        private async Task LoadSummariesAsync(string state = null, string commodity = null)
        {
            // Fetch latest day and previous day records (limit larger to ensure we get previous day)
            var latest = await _apiService.GetDailyPricesAsync(apiKey: null, state: state, commodity: commodity, limit: 500);
            // Attempt to determine previous day date from records
            if (latest == null || !latest.Any())
            {
                PricesCollection.ItemsSource = new List<Models.CommoditySummary>();
                return;
            }

            // Group by commodity and compute average modal price for latest day
            var groups = latest.GroupBy(r => r.Commodity).Select(g => new Models.CommoditySummary
            {
                Commodity = g.Key,
                AverageModalPrice = g.Average(x => x.Modal_price ?? 0),
                Unit = "", // unit not provided reliably
                Icon = "dotnet_bot.png"
            }).ToList();

            // For percent change, fetch previous day by filtering arrival_date
            // Determine most recent two dates in the data
            var dates = latest.Select(r => r.Arrival_Date).Distinct().ToList();
            if (dates.Count >= 2)
            {
                // Assume arrival_date formatted dd/MM/yyyy, pick top two
                var parsed = dates.Select(d => DateTime.TryParseExact(d, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var dt) ? dt : (DateTime?)null)
                    .Where(x => x.HasValue).Select(x => x.Value).OrderByDescending(x => x).ToList();

                if (parsed.Count >= 2)
                {
                    var latestDate = parsed[0].ToString("dd/MM/yyyy");
                    var prevDate = parsed[1].ToString("dd/MM/yyyy");

                    var prevRecords = await _apiService.GetDailyPricesAsync(apiKey: null, state: state, commodity: commodity, limit: 500);
                    var prevGroups = prevRecords.Where(r => r.Arrival_Date == prevDate).GroupBy(r => r.Commodity)
                        .ToDictionary(g => g.Key, g => g.Average(x => x.Modal_price ?? 0));

                    foreach (var s in groups)
                    {
                        if (prevGroups.TryGetValue(s.Commodity, out var prevAvg) && prevAvg > 0)
                        {
                            var change = (s.AverageModalPrice - prevAvg) / prevAvg;
                            s.PercentChange = change;
                        }
                        else
                        {
                            s.PercentChange = 0;
                        }
                    }
                }
            }

            PricesCollection.ItemsSource = groups.OrderByDescending(g => g.AverageModalPrice).ToList();
        }

        private async void OnApplyFiltersClicked(object? sender, EventArgs e)
        {
            string state = null;
            string commodity = null;
            if (StatePicker.SelectedIndex >= 0)
                state = StatePicker.Items[StatePicker.SelectedIndex];
            if (CommodityPicker.SelectedIndex >= 0)
                commodity = CommodityPicker.Items[CommodityPicker.SelectedIndex];

            await LoadSummariesAsync(state, commodity);
        }
    }
}
