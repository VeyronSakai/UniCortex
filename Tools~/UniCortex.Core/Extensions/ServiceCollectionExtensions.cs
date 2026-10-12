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
        services.AddTransient<HttpRequestHandler>();
        services.AddTransient<IUnityServerUrlProvider, UnityServerUrlProvider>();
        services.AddHttpClient(HttpClientNames.UniCortex, client =>
            {
                // Test runs and domain reloads can take several minutes.
                // HttpRequestHandler retries during reloads, so this must exceed the max retry window.
                client.Timeout = TimeSpan.FromMinutes(10);
            })
            .AddHttpMessageHandler<HttpRequestHandler>();

        services.AddTransient<IUnityEditorClient, UnityEditorClient>();
        services.AddTransient<IUnityEditorProcessProvider, UnityEditorProcessProvider>();
        services.AddTransient<IModalDialogAccessor>(_ => CreateModalDialogAccessor());

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
        services.AddTransient<ModalDialogUseCase>();

        return services;
    }

    private static IModalDialogAccessor CreateModalDialogAccessor()
    {
        if (OperatingSystem.IsMacOS())
        {
            return new MacModalDialogAccessor();
        }

        if (OperatingSystem.IsWindows())
        {
            return new WindowsModalDialogAccessor();
        }

        return new UnsupportedModalDialogAccessor();
    }
}
