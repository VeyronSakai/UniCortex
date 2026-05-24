using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class ScriptableObjectPropertyCommands(ScriptableObjectUseCase scriptableObjectUseCase)
{
    /// <summary>List serialized properties of a ScriptableObject .asset file.</summary>
    /// <param name="assetPath">Asset path of the ScriptableObject (e.g. "Assets/Data/MyData.asset").</param>
    [Command("list")]
    public async Task List([Argument] string assetPath,
        CancellationToken cancellationToken = default)
    {
        var json = await scriptableObjectUseCase.GetPropertiesAsync(assetPath, cancellationToken);
        Console.WriteLine(json);
    }

    /// <summary>Set a serialized property on a ScriptableObject .asset file.</summary>
    /// <param name="assetPath">Asset path of the ScriptableObject.</param>
    /// <param name="propertyPath">Serialized property path (e.g. "m_Speed").</param>
    /// <param name="value">Value to set as a string. Type is auto-detected from the property.</param>
    [Command("set")]
    public async Task Set([Argument] string assetPath, [Argument] string propertyPath,
        [Argument] string value, CancellationToken cancellationToken = default)
    {
        var message = await scriptableObjectUseCase.SetPropertyAsync(assetPath, propertyPath, value,
            cancellationToken);
        Console.WriteLine(message);
    }
}
