namespace AgricultureMarketPriceApp
{
    public partial class MainPage : ContentPage
    {
        private readonly Services.ApiService _apiService;
        private const string ApiKeyStorageKey = "ApiKey";

        public MainPage(Services.ApiService apiService)
        {
            InitializeComponent();
            _apiService = apiService;

            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object? sender, EventArgs e)
        {
            try
            {
                var stored = await SecureStorage.Default.GetAsync(ApiKeyStorageKey);
                if (!string.IsNullOrWhiteSpace(stored))
                {
                    ApiKeyEntry.Text = stored;
                    ApiKeyStatusLabel.Text = "Using saved API key";
                }

                var result = await _apiService.GetDailyPricesWithDiagnosticsAsync(stored, null, null);
                if (result.Success)
                {
                    PricesCollection.ItemsSource = result.Data;
                    ApiKeyStatusLabel.Text = "Loaded prices";
                }
                else
                {
                    ApiKeyStatusLabel.Text = $"API error: {result.StatusCode} {result.ReasonPhrase}";
                }
            }
            catch (Exception ex)
            {
                ApiKeyStatusLabel.Text = ex.Message;
            }
        }

        private async void OnMonthlyGraphClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("monthlygraph");
        }

        private async void OnSaveApiKeyClicked(object? sender, EventArgs e)
        {
            var key = ApiKeyEntry.Text?.Trim();
            if (string.IsNullOrWhiteSpace(key))
            {
                await DisplayAlert("API Key", "Please enter a valid API key.", "OK");
                return;
            }

            try
            {
                await SecureStorage.Default.SetAsync(ApiKeyStorageKey, key);
                ApiKeyStatusLabel.Text = "API key saved";

                // Try a fetch with the new key
                var result = await _apiService.GetDailyPricesWithDiagnosticsAsync(key, null, null);
                if (result.Success)
                {
                    PricesCollection.ItemsSource = result.Data;
                    ApiKeyStatusLabel.Text = "Loaded prices with saved key";
                }
                else
                {
                    ApiKeyStatusLabel.Text = $"API error: {result.StatusCode} {result.ReasonPhrase}";
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
