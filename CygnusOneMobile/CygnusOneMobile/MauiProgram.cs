using Microsoft.Extensions.Logging;
using CygnusOneMobile.Services;
using CygnusOneMobile.ViewModels;
using CygnusOneMobile.Views;

namespace CygnusOneMobile
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

            // Register services
            builder.Services.AddSingleton<ApiService>();

            // Register view models
            builder.Services.AddTransient<ArticlesViewModel>();

            // Register pages
            builder.Services.AddTransient<ArticlesPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
