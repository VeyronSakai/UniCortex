using Microsoft.Extensions.DependencyInjection;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.Infrastructures;
using UniCortex.Core.Services;

namespace UniCortex.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUniCortexCore(this IServiceCollection services)
    {
        services.AddTransient<HttpRequestHandler>();
        services.AddTransient<IUnityServerUrlProvider, UnityServerUrlProvider>();
        services.AddHttpClient(HttpClientNames.UniCortex, client =>
            {
                // Test runs can take several minutes, so increase the default timeout.
                client.Timeout = TimeSpan.FromMinutes(10);
            })
            .AddHttpMessageHandler<HttpRequestHandler>();

        services.AddTransient<EditorService>();
        services.AddTransient<GameObjectService>();
        services.AddTransient<ComponentService>();
        services.AddTransient<SceneService>();
        services.AddTransient<PrefabService>();
        services.AddTransient<TestService>();
        services.AddTransient<ConsoleService>();
        services.AddTransient<AssetService>();
        services.AddTransient<MenuItemService>();
        services.AddTransient<ScreenshotService>();

        return services;
    }
}
