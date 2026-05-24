using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class ScriptableObjectCommands(ScriptableObjectUseCase scriptableObjectUseCase)
{
    /// <summary>Create a new ScriptableObject .asset file.</summary>
    /// <param name="typeName">Fully qualified ScriptableObject subclass name (e.g. "MyNamespace.MyScriptableObject").</param>
    /// <param name="assetPath">Asset path where the .asset should be saved (e.g. "Assets/Data/MyData.asset").</param>
    [Command("create")]
    public async Task Create([Argument] string typeName, [Argument] string assetPath,
        CancellationToken cancellationToken = default)
    {
        var json = await scriptableObjectUseCase.CreateAsync(typeName, assetPath, cancellationToken);
        Console.WriteLine(json);
    }
}
