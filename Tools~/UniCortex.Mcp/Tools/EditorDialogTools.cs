using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

// These tools read the dialogs through the OS window APIs, not the Unity Editor's HTTP server, so they are not
// serialized with the other tools: they must run while another tool call waits for the blocked Editor.
[McpServerToolType, UsedImplicitly]
public class EditorDialogTools(EditorDialogUseCase editorDialogUseCase)
{
    [McpServerTool(Name = "get_editor_dialogs", ReadOnly = true),
     Description(
         "Get the modal dialogs that the Unity Editor itself shows (e.g. \"Saving failed\", a scene reload " +
         "confirmation), not dialogs in the game's UI, with their title, message, and button labels. While a modal dialog is open, the Editor does not process " +
         "other tool calls. This tool does not need the Editor to respond, so use it when other tools do not return. " +
         "Requires the Accessibility permission on macOS. Not supported on Linux."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetEditorDialogsAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextWithoutSequencingAsync(editorDialogUseCase.GetDialogsAsync, cancellationToken);

    [McpServerTool(Name = "click_editor_dialog_button", ReadOnly = false),
     Description(
         "Click a button of a modal dialog that the Unity Editor itself shows (not the game's UI) by its label, to close the dialog without the " +
         "user's help. Use get_editor_dialogs first to read the dialog and its button labels. " +
         "Requires the Accessibility permission on macOS. Not supported on Linux."),
     UsedImplicitly]
    public ValueTask<CallToolResult> ClickEditorDialogButtonAsync(
        [Description("The label of the button to click, as returned by get_editor_dialogs (e.g. \"OK\"). Case-insensitive.")]
        string button,
        CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextWithoutSequencingAsync(
            ct => editorDialogUseCase.ClickButtonAsync(button, ct), cancellationToken);
}
