using ConsoleAppFramework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UniCortex.Cli.Commands;
using UniCortex.Cli.Infrastructures;
using UniCortex.Core.Extensions;
using UniCortex.Core.UseCases;

static void ConfigureServices(IServiceCollection services)
{
    services.AddLogging(b =>
    {
        b.SetMinimumLevel(LogLevel.Warning);
    });
    services.AddUniCortexCore();
}

static void RegisterBuiltInCommands(ConsoleApp.ConsoleAppBuilder app)
{
    app.Add<EditorCommands>("editor");
    app.Add<SceneCommands>("scene");
    app.Add<GameObjectCommands>("gameobject");
    app.Add<ComponentCommands>("component");
    app.Add<PrefabCommands>("prefab");
    app.Add<TestCommands>("test");
    app.Add<ConsoleCommands>("console");
    app.Add<AssetCommands>("asset");
    app.Add<MenuItemCommands>("menu");
    app.Add<ScreenshotCommands>("screenshot");
    app.Add<CustomCommands>("custom");

    app.Add<SceneViewCommands>("scene-view");
    app.Add<GameViewCommands>("game-view");
    app.Add<GameViewSizeCommands>("game-view size");
    app.Add<RecorderAllCommands>("recorder all");
    app.Add<MovieRecorderCommands>("recorder movie");
    app.Add<InputCommands>("input");
    app.Add<TimelineCommands>("timeline");
    app.Add<TimelineTrackCommands>("timeline track");
    app.Add<TimelineClipCommands>("timeline clip");
}

static bool ShouldTryCustomCommand(IReadOnlyList<string> arguments)
{
    if (arguments.Count == 0)
    {
        return false;
    }

    var first = arguments[0];
    if (string.IsNullOrWhiteSpace(first) || first.StartsWith("-", StringComparison.Ordinal))
    {
        return false;
    }

    return first is not
        ("asset" or "component" or "console" or "custom" or "editor" or "game-view" or "gameobject" or "input" or
            "menu" or "prefab" or "recorder" or "scene" or "scene-view" or "screenshot" or "test" or "timeline");
}

if (ShouldTryCustomCommand(args))
{
    var bootstrapServices = new ServiceCollection();
    ConfigureServices(bootstrapServices);
    using var bootstrapProvider = bootstrapServices.BuildServiceProvider();
    var customCommandDispatcher =
        new CustomCommandDispatcher(bootstrapProvider.GetRequiredService<CustomToolUseCase>());

    if (await customCommandDispatcher.TryRunAsync(args, CancellationToken.None))
    {
        return;
    }
}

var app = ConsoleApp.Create()
    .ConfigureServices(ConfigureServices);
RegisterBuiltInCommands(app);
app.Run(args);
