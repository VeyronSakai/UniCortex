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
         "Get the modal dialogs the Unity Editor itself shows (e.g. \"Saving failed\"; not the game's UI) with " +
         "their title, message and button labels. While one is open the Editor processes no other tool calls; " +
         "this tool works without the Editor, so use it when other tools do not return. " +
         "Needs the Accessibility permission on macOS; not supported on Linux."),
     UsedImplicitly]
    public ValueTask<CallToolResult> GetEditorDialogsAsync(CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextWithoutSequencingAsync(editorDialogUseCase.GetDialogsAsync, cancellationToken);

    [McpServerTool(Name = "click_editor_dialog_button", ReadOnly = false),
     Description(
         "Click a button of a modal Unity Editor dialog (not the game's UI) by its label to close it. " +
         "Read the labels with get_editor_dialogs first. " +
         "Needs the Accessibility permission on macOS; not supported on Linux."),
     UsedImplicitly]
    public ValueTask<CallToolResult> ClickEditorDialogButtonAsync(
        [Description("Button label from get_editor_dialogs (e.g. \"OK\"), case-insensitive.")]
        string button,
        CancellationToken cancellationToken)
        => McpToolExecution.ExecuteTextWithoutSequencingAsync(
            ct => editorDialogUseCase.ClickButtonAsync(button, ct), cancellationToken);
}
