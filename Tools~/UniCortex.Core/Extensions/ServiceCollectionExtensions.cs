using Microsoft.Extensions.DependencyInjection;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.Infrastructures;
using UniCortex.Core.UseCases;

namespace UniCortex.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUniCortexCore(this IServiceCollection services)
    {
        services.AddSingleton<IAsyncOperationSequencer, AsyncOperationSequencer>();
        services.AddTransient<EditorDialogWatchHandler>();
        services.AddTransient<HttpRequestHandler>();
        services.AddTransient<IUnityServerUrlProvider, UnityServerUrlProvider>();
        var httpClientBuilder = services.AddHttpClient(HttpClientNames.UniCortex, client =>
        {
            // How long a request takes cannot be known in advance (e.g. a platform switch or a menu item in a large
            // project), so there is no time limit: the caller decides when to stop waiting by cancelling the request.
            client.Timeout = Timeout.InfiniteTimeSpan;
        });
        // The dialog watch is outside the retries, so that it also covers the time spent resending a request. The
        // dialogs can be read on macOS and Windows only, so elsewhere it would only warn on every long request.
        if (OperatingSystem.IsMacOS() || OperatingSystem.IsWindows())
        {
            httpClientBuilder.AddHttpMessageHandler<EditorDialogWatchHandler>();
        }

        httpClientBuilder.AddHttpMessageHandler<HttpRequestHandler>();

        services.AddTransient<IUnityEditorClient, UnityEditorClient>();
        services.AddTransient<IUnityEditorProcessProvider, UnityEditorProcessProvider>();
        services.AddTransient<IEditorDialogAccessor>(_ => CreateEditorDialogAccessor());

        services.AddTransient<EditorUseCase>();
        services.AddTransient<GameObjectUseCase>();
        services.AddTransient<ComponentUseCase>();
        services.AddTransient<SceneUseCase>();
        services.AddTransient<PrefabUseCase>();
        services.AddTransient<ScriptableObjectUseCase>();
        services.AddTransient<AnimationClipUseCase>();
        services.AddTransient<TestUseCase>();
        services.AddTransient<ConsoleUseCase>();
        services.AddTransient<AssetUseCase>();
        services.AddTransient<ProjectWindowUseCase>();
        services.AddTransient<MenuItemUseCase>();
        services.AddTransient<MovieRecordingUseCase>();

        services.AddTransient<SceneViewUseCase>();
        services.AddTransient<GameViewUseCase>();
        services.AddTransient<InputUseCase>();
        services.AddTransient<TimelineUseCase>();
        services.AddTransient<ExtensionUseCase>();
        services.AddTransient<EditorDialogUseCase>();

        return services;
    }

    private static IEditorDialogAccessor CreateEditorDialogAccessor()
    {
        if (OperatingSystem.IsMacOS())
        {
            return new MacEditorDialogAccessor();
        }

        if (OperatingSystem.IsWindows())
        {
            return new WindowsEditorDialogAccessor();
        }

        return new UnsupportedEditorDialogAccessor();
    }
}
