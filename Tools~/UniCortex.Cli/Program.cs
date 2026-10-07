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
app.Add<RecorderAllCommands>("recorder all");
app.Add<MovieRecorderCommands>("recorder movie");
app.Add<InputKeyCommands>("input key");
app.Add<InputMouseCommands>("input mouse");
app.Add<InputUIPointerCommands>("input ui-pointer");
app.Add<TimelineCommands>("timeline");
app.Add<TimelineTrackCommands>("timeline track");
app.Add<TimelineTrackPropertyCommands>("timeline track property");
app.Add<TimelineClipCommands>("timeline clip");
app.Add<TimelineClipPropertyCommands>("timeline clip property");
app.Add<ExtensionCommands>("extension");
app.Run(args);
