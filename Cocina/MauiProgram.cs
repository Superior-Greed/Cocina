using Cocina.Data;
using Cocina.Interfas;
using Cocina.Service;
using CommunityToolkit.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Cocina
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"dev.db");
            if (!File.Exists(path))
            {
                var assambly = IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;
                using (Stream stream = assambly.GetManifestResourceStream("Cocina.dev.db"))
                {
                    if (stream is not null)
                    {
                        using (MemoryStream memoryStream = new())
                        {
                            stream.CopyTo(memoryStream);
                            File.WriteAllBytes(path, memoryStream.ToArray());
                        }
                    }
                }
            }
#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
		    builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<WeatherForecastService>();
            builder.Services.AddSingleton<RecipeInterface,RecipeService>(p => ActivatorUtilities.CreateInstance<RecipeService>(p, path));

            builder.Services.AddSingleton<AlertsInterface, AlertsService>();

            return builder.Build();
        }
    }
}