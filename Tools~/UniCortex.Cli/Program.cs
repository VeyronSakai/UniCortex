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
            // Warnings go to stderr, so that they never mix with a command's output on stdout. They tell early why
            // a command may wait long, e.g. that the Editor's dialogs cannot be read without a permission.
            b.AddConsole(options => { options.LogToStandardErrorThreshold = LogLevel.Trace; });
            b.SetMinimumLevel(LogLevel.Warning);
        });
        services.AddUniCortexCore();
    });

app.Add<EditorCommands>("editor");
app.Add<EditorPlatformCommands>("editor platform");
app.Add<EditorDialogCommands>("editor dialog");
app.Add<SceneCommands>("scene");
app.Add<GameObjectCommands>("game-object");
app.Add<ComponentCommands>("component");
app.Add<ComponentPropertyCommands>("component property");
app.Add<PrefabCommands>("prefab");
app.Add<ScriptableObjectCommands>("scriptable-object");
app.Add<ScriptableObjectPropertyCommands>("scriptable-object property");
app.Add<AnimationClipCommands>("animation-clip");
app.Add<AnimationCurveCommands>("animation-clip curve");
app.Add<TestCommands>("test");
app.Add<ConsoleCommands>("console");
app.Add<AssetCommands>("asset");
app.Add<ProjectWindowCommands>("project-window");
app.Add<MenuItemCommands>("menu");

app.Add<SceneViewCommands>("scene-view");
app.Add<GameViewCommands>("game-view");
app.Add<GameViewSizeCommands>("game-view size");
app.Add<GameViewScaleCommands>("game-view scale");
app.Add<GameViewViewTypeCommands>("game-view view-type");
app.Add<SimulatorDeviceCommands>("game-view simulator device");
app.Add<RecorderAllCommands>("recorder all");
app.Add<MovieRecorderCommands>("recorder movie");
app.Add<InputKeyCommands>("input key");
app.Add<InputTextCommands>("input text");
app.Add<InputMouseCommands>("input mouse");
app.Add<InputUIPointerCommands>("input ui-pointer");
app.Add<TimelineCommands>("timeline");
app.Add<TimelineTrackCommands>("timeline track");
app.Add<TimelineTrackPropertyCommands>("timeline track property");
app.Add<TimelineClipCommands>("timeline clip");
app.Add<TimelineClipPropertyCommands>("timeline clip property");
app.Add<ExtensionCommands>("extension");
app.Run(args);
