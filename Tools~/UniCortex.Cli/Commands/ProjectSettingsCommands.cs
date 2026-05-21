using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class ProjectSettingsCommands(ProjectSettingsUseCase projectSettingsUseCase)
{
    /// <summary>Get serialized properties of a ProjectSettings category.</summary>
    /// <param name="category">Category name (e.g. "Player", "Quality", "Time"). Run `categories` for the full list.</param>
    [Command("get")]
    public async Task Get([Argument] string category, CancellationToken cancellationToken = default)
    {
        var json = await projectSettingsUseCase.GetAsync(category, cancellationToken);
        Console.WriteLine(json);
    }

    /// <summary>Set a serialized property on a ProjectSettings category.</summary>
    /// <param name="category">Category name (e.g. "Player", "Time").</param>
    /// <param name="propertyPath">Serialized property path (e.g. "m_TimeScale").</param>
    /// <param name="value">Value to set as a string. Type is auto-detected from the property.</param>
    [Command("set")]
    public async Task Set([Argument] string category, [Argument] string propertyPath, [Argument] string value,
        CancellationToken cancellationToken = default)
    {
        var message = await projectSettingsUseCase.SetAsync(category, propertyPath, value, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>List the available ProjectSettings category names and their backing asset paths.</summary>
    [Command("categories")]
    public async Task Categories(CancellationToken cancellationToken = default)
    {
        var json = await projectSettingsUseCase.ListCategoriesAsync(cancellationToken);
        Console.WriteLine(json);
    }
}
