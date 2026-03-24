using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class TimeScaleCommands(EditorUseCase editorUseCase)
{
    /// <summary>Set the Time.timeScale value in Unity.</summary>
    /// <param name="timeScale">The time scale value to set (e.g. 0.5 for half speed, 2 for double speed).</param>
    [Command("set")]
    public async Task Set([Argument] float timeScale, CancellationToken cancellationToken)
    {
        var message = await editorUseCase.SetTimeScaleAsync(timeScale, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Get the current Time.timeScale value in Unity.</summary>
    [Command("get")]
    public async Task Get(CancellationToken cancellationToken)
    {
        var message = await editorUseCase.GetTimeScaleAsync(cancellationToken);
        Console.WriteLine(message);
    }
}
