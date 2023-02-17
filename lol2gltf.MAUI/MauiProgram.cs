using lol2gltf.MAUI.Data;
using Microsoft.AspNetCore.Components.WebView.Maui;
using MudBlazor.Services;
using CommunityToolkit.Maui;

namespace lol2gltf.MAUI;

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
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        builder.Services.AddMudServices();

        builder.Services.AddSingleton<WeatherForecastService>();

        return builder.Build();
    }
}
