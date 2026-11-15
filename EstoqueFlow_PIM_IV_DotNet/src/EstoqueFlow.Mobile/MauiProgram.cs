using System.Net;
using EstoqueFlow.Mobile.Services;

namespace EstoqueFlow.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        // Android Emulator: 10.0.2.2 aponta para o computador hospedeiro.
        // O Docker Compose publica o servidor Web/API na porta 8080.
        var cookies = new CookieContainer();
        var handler = new HttpClientHandler
        {
            CookieContainer = cookies,
            UseCookies = true,
            AllowAutoRedirect = false
        };
        var http = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://10.0.2.2:8080/"),
            Timeout = TimeSpan.FromSeconds(20)
        };
        builder.Services.AddSingleton(http);
        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddSingleton<MainPage>();
        return builder.Build();
    }
}
