using ConsoleAppFramework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UniCortex.Cli.Commands;
using UniCortex.Core.Extensions;

var app = ConsoleApp.Create()
    .ConfigureServices(services =>
    {
        services.AddLogging(b =>
        {
            b.SetMinimumLevel(LogLevel.Warning);
        });
        services.AddUniCortexCore();
    });

app.Add<EditorCommands>("editor");
app.Add<SceneCommands>("scene");
app.Add<GameObjectCommands>("gameobject");
app.Add<ComponentCommands>("component");
app.Add<PrefabCommands>("prefab");
app.Add<TestCommands>("test");
app.Add<ConsoleCommands>("console");
app.Add<AssetCommands>("asset");
app.Add<ProjectViewCommands>("project-view");
app.Add<MenuItemCommands>("menu");
app.Add<ScreenshotCommands>("screenshot");

app.Add<SceneViewCommands>("scene-view");
app.Add<GameViewCommands>("game-view");
app.Add<GameViewSizeCommands>("game-view size");
app.Add<RecorderAllCommands>("recorder all");
app.Add<MovieRecorderCommands>("recorder movie");
app.Add<InputCommands>("input");
app.Add<TimelineCommands>("timeline");
app.Add<TimelineTrackCommands>("timeline track");
app.Add<TimelineClipCommands>("timeline clip");
app.Add<ExtensionCommands>("extension");
app.Run(args);
