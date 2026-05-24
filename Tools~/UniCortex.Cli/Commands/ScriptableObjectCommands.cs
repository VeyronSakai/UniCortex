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

    /// <summary>Get serialized properties of a ScriptableObject .asset file.</summary>
    /// <param name="assetPath">Asset path of the ScriptableObject (e.g. "Assets/Data/MyData.asset").</param>
    [Command("properties")]
    public async Task Properties([Argument] string assetPath,
        CancellationToken cancellationToken = default)
    {
        var json = await scriptableObjectUseCase.GetPropertiesAsync(assetPath, cancellationToken);
        Console.WriteLine(json);
    }

    /// <summary>Set a serialized property on a ScriptableObject .asset file.</summary>
    /// <param name="assetPath">Asset path of the ScriptableObject.</param>
    /// <param name="propertyPath">Serialized property path (e.g. "m_Speed").</param>
    /// <param name="value">Value to set as a string. Type is auto-detected from the property.</param>
    [Command("set-property")]
    public async Task SetProperty([Argument] string assetPath, [Argument] string propertyPath,
        [Argument] string value, CancellationToken cancellationToken = default)
    {
        var message = await scriptableObjectUseCase.SetPropertyAsync(assetPath, propertyPath, value,
            cancellationToken);
        Console.WriteLine(message);
    }
}
