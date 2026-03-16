using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class EditorCommands(EditorUseCase editorService)
{
    /// <summary>Check connectivity with the Unity Editor.</summary>
    [Command("ping")]
    public async Task Ping(CancellationToken cancellationToken)
    {
        var message = await editorService.PingAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Start Play Mode in the Unity Editor.</summary>
    [Command("play")]
    public async Task Play(CancellationToken cancellationToken)
    {
        var message = await editorService.EnterPlayModeAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Stop Play Mode in the Unity Editor.</summary>
    [Command("stop")]
    public async Task Stop(CancellationToken cancellationToken)
    {
        var message = await editorService.ExitPlayModeAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Get the current state of the Unity Editor (play mode, paused). Works even when the editor is paused.</summary>
    [Command("status")]
    public async Task Status(CancellationToken cancellationToken)
    {
        var message = await editorService.GetEditorStatusAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Unpause the Unity Editor. Works even when the editor is paused.</summary>
    [Command("unpause")]
    public async Task Unpause(CancellationToken cancellationToken)
    {
        var message = await editorService.UnpauseAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Perform Undo in the Unity Editor.</summary>
    [Command("undo")]
    public async Task Undo(CancellationToken cancellationToken)
    {
        var message = await editorService.UndoAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Perform Redo in the Unity Editor.</summary>
    [Command("redo")]
    public async Task Redo(CancellationToken cancellationToken)
    {
        var message = await editorService.RedoAsync(cancellationToken);
        Console.WriteLine(message);
    }

    /// <summary>Request a domain reload (script recompilation) in the Unity Editor.</summary>
    [Command("reload-domain")]
    public async Task ReloadDomain(CancellationToken cancellationToken)
    {
        var message = await editorService.ReloadDomainAsync(cancellationToken);
        Console.WriteLine(message);
    }
}
