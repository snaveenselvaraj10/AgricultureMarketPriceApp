using Microsoft.Extensions.Logging;
using System.Globalization;
using AgricultureMarketPriceApp.Services;

namespace AgricultureMarketPriceApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // Register services
            builder.Services.AddSingleton<ApiService>();

            // Initialize localization based on device culture
            try
            {
                var culture = CultureInfo.CurrentCulture;
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
            }
            catch
            {
                // ignore
            }

            return builder.Build();
        }
    }
}
