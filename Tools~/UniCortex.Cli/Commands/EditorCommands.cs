using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class EditorCommands(EditorUseCase editorUseCase)
{
    /// <summary>Check connectivity with the Unity Editor.</summary>
    [Command("ping")]
    public async Task Ping(CancellationToken cancellationToken)
    {
        var message = await editorUseCase.PingAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Start Play Mode in the Unity Editor.</summary>
    [Command("play")]
    public async Task Play(CancellationToken cancellationToken)
    {
        var message = await editorUseCase.EnterPlayModeAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Stop Play Mode in the Unity Editor.</summary>
    [Command("stop")]
    public async Task Stop(CancellationToken cancellationToken)
    {
        var message = await editorUseCase.ExitPlayModeAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Get the current state of the Unity Editor (play mode, paused). Works even when the editor is paused.</summary>
    [Command("status")]
    public async Task Status(CancellationToken cancellationToken)
    {
        var message = await editorUseCase.GetEditorStatusAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Pause the Unity Editor. Use with step for frame-by-frame control.</summary>
    [Command("pause")]
    public async Task Pause(CancellationToken cancellationToken)
    {
        var message = await editorUseCase.PauseAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Unpause the Unity Editor. Works even when the editor is paused.</summary>
    [Command("unpause")]
    public async Task Unpause(CancellationToken cancellationToken)
    {
        var message = await editorUseCase.UnpauseAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Advance the Unity Editor by one frame while paused.</summary>
    [Command("step")]
    public async Task Step(CancellationToken cancellationToken)
    {
        var message = await editorUseCase.StepAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Perform Undo in the Unity Editor.</summary>
    [Command("undo")]
    public async Task Undo(CancellationToken cancellationToken)
    {
        var message = await editorUseCase.UndoAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Perform Redo in the Unity Editor.</summary>
    [Command("redo")]
    public async Task Redo(CancellationToken cancellationToken)
    {
        var message = await editorUseCase.RedoAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Set the Time.timeScale value in Unity.</summary>
    [Command("set-time-scale")]
    public async Task SetTimeScale([Argument] float timeScale, CancellationToken cancellationToken)
    {
        var message = await editorUseCase.SetTimeScaleAsync(timeScale, cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Get the current Time.timeScale value in Unity.</summary>
    [Command("get-time-scale")]
    public async Task GetTimeScale(CancellationToken cancellationToken)
    {
        var message = await editorUseCase.GetTimeScaleAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Request a domain reload (script recompilation) in the Unity Editor.</summary>
    [Command("reload-domain")]
    public async Task ReloadDomain(CancellationToken cancellationToken)
    {
        var message = await editorUseCase.ReloadDomainAsync(cancellationToken);
        Console.WriteLine(message);
    }
}
