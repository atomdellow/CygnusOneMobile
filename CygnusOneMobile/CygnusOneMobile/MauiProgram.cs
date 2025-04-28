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
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<SessionManager>();
            builder.Services.AddSingleton<DebugLogger>();

            // Register app shell
            builder.Services.AddSingleton<AppShell>();

            // Register view models
            builder.Services.AddTransient<ArticlesViewModel>();
            builder.Services.AddTransient<AuthViewModel>();
            builder.Services.AddTransient<DebugViewModel>();

            // Register pages
            builder.Services.AddTransient<ArticlesPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<DebugPage>();
            builder.Services.AddTransient<MainPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
