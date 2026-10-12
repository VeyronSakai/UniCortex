using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class EditorDialogCommands(ModalDialogUseCase modalDialogUseCase)
{
    /// <summary>List the modal dialogs open in the Unity Editor with their title, message, and button labels. Works while the Editor is blocked by a dialog.</summary>
    [Command("list")]
    public async Task List(CancellationToken cancellationToken)
    {
        var json = await modalDialogUseCase.GetDialogsAsync(cancellationToken);
        Console.WriteLine(json);
    }

    /// <summary>Click a button of a modal dialog open in the Unity Editor by its label.</summary>
    /// <param name="button">The label of the button to click (e.g. OK). Case-insensitive.</param>
    [Command("click")]
    public async Task Click([Argument] string button, CancellationToken cancellationToken)
    {
        var message = await modalDialogUseCase.ClickButtonAsync(button, cancellationToken);
        Console.WriteLine(message);
    }
}
