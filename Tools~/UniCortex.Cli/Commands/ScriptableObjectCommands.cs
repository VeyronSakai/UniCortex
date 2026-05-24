using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class ScriptableObjectCommands(ScriptableObjectUseCase scriptableObjectUseCase)
{
    /// <summary>Create a new ScriptableObject .asset file.</summary>
    /// <param name="typeName">Fully-qualified ScriptableObject subclass name (e.g. "MyNamespace.MyScriptableObject").</param>
    /// <param name="assemblyName">Name of the assembly that defines the type (e.g. "Assembly-CSharp").</param>
    /// <param name="assetPath">Asset path where the .asset should be saved (e.g. "Assets/Data/MyData.asset").</param>
    [Command("create")]
    public async Task Create([Argument] string typeName, [Argument] string assemblyName,
        [Argument] string assetPath, CancellationToken cancellationToken = default)
    {
        var json = await scriptableObjectUseCase.CreateAsync(typeName, assemblyName, assetPath, cancellationToken);
        Console.WriteLine(json);
    }
}
