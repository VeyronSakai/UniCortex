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
app.Add<MenuItemCommands>("menu");
app.Add<ScreenshotCommands>("screenshot");
app.Add<InputCommands>("input");

app.Run(args);
