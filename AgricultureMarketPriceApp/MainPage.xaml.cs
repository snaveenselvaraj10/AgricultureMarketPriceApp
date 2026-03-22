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
            var data = await _apiService.GetDailyPricesAsync();
            PricesCollection.ItemsSource = data;
        }

        private async void OnMonthlyGraphClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("monthlygraph");
        }
    }
}
