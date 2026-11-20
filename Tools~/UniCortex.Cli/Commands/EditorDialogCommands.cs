using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class EditorDialogCommands(EditorDialogUseCase editorDialogUseCase)
{
    /// <summary>List the modal dialogs open in the Unity Editor with their title, message, and button labels. Works while the Editor is blocked by a dialog.</summary>
    [Command("list")]
    public async Task List(CancellationToken cancellationToken)
    {
        var json = await editorDialogUseCase.GetDialogsAsync(cancellationToken);
        Console.WriteLine(json);
    }

    /// <summary>Click a button of a modal dialog open in the Unity Editor by its label. On macOS, if the buttons of a dialog cannot be read because the Editor is in another Space or hidden, this brings the Editor to the front to read them.</summary>
    /// <param name="button">The label of the button to click (e.g. OK). Case-insensitive.</param>
    [Command("click")]
    public async Task Click([Argument] string button, CancellationToken cancellationToken)
    {
        var message = await editorDialogUseCase.ClickButtonAsync(button, cancellationToken);
        Console.WriteLine(message);
    }
}
