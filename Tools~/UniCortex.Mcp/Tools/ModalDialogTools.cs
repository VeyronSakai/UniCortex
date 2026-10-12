using System.ComponentModel;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using UniCortex.Core.UseCases;

namespace UniCortex.Mcp.Tools;

// These tools read the dialogs through the OS window APIs, not the Unity Editor's HTTP server, so they are not
// serialized with the other tools: they must run while another tool call waits for the blocked Editor.
[McpServerToolType, UsedImplicitly]
public class ModalDialogTools(ModalDialogUseCase modalDialogUseCase)
{
    [McpServerTool(Name = "get_modal_dialogs", ReadOnly = true),
     Description(
         "Get the modal dialogs open in the Unity Editor (e.g. \"Saving failed\", a scene reload confirmation) " +
         "with their title, message, and button labels. While a modal dialog is open, the Editor does not process " +
         "other tool calls. This tool does not need the Editor to respond, so use it when other tools do not return. " +
         "Requires the Accessibility permission on macOS. Not supported on Linux."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetModalDialogsAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextWithoutSequencingAsync(modalDialogUseCase.GetDialogsAsync, cancellationToken);

    [McpServerTool(Name = "click_modal_dialog_button", ReadOnly = false),
     Description(
         "Click a button of a modal dialog open in the Unity Editor by its label, to close the dialog without the " +
         "user's help. Use get_modal_dialogs first to read the dialog and its button labels. " +
         "Requires the Accessibility permission on macOS. Not supported on Linux."),
     UsedImplicitly]
    public ValueTask<CallToolResult> ClickModalDialogButtonAsync(
        [Description("The label of the button to click, as returned by get_modal_dialogs (e.g. \"OK\"). Case-insensitive.")]
        string button,
        CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextWithoutSequencingAsync(
            ct => modalDialogUseCase.ClickButtonAsync(button, ct), cancellationToken);
}
