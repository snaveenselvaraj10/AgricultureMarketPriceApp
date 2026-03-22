namespace AgricultureMarketPriceApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("monthlygraph", typeof(Pages.MonthlyGraphPage));
        }
    }
}
