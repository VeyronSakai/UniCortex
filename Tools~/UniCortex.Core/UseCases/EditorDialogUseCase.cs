using System.Text.Encodings.Web;
using System.Text.Json;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.Domains.Models;

namespace UniCortex.Core.UseCases;

/// <summary>
/// Reads and closes the Unity Editor's modal dialogs through the OS window APIs instead of the Editor's HTTP server,
/// which does not respond while a modal dialog blocks the Editor's main thread.
/// </summary>
public class EditorDialogUseCase(IUnityEditorProcessProvider processProvider, IEditorDialogAccessor dialogAccessor)
{
    // Dialog texts are shown as they are, e.g. in Japanese, instead of escaping non-ASCII characters.
    private static readonly JsonSerializerOptions s_jsonOptions = new(JsonOptions.Default)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public ValueTask<string> GetDialogsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var response = new GetEditorDialogsResponse
        {
            dialogs = dialogAccessor.GetDialogs(processProvider.GetProcessId()).ToList()
        };
        return ValueTask.FromResult(JsonSerializer.Serialize(response, s_jsonOptions));
    }

    public ValueTask<string> ClickButtonAsync(string buttonLabel, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var processId = processProvider.GetProcessId();
        var dialog = dialogAccessor.ClickButton(processId, buttonLabel);
        if (dialog != null)
        {
            var clicked = dialog.buttons.FirstOrDefault(b => EditorDialogButtonLabel.Matches(b, buttonLabel)) ?? buttonLabel;
            return ValueTask.FromResult($"Clicked \"{clicked}\" in the dialog \"{dialog.title}\".");
        }

        var dialogs = dialogAccessor.GetDialogs(processId);
        if (dialogs.Count == 0)
        {
            throw new InvalidOperationException("No modal dialog is open in the Unity Editor.");
        }

        var buttons = dialogs.SelectMany(d => d.buttons).ToList();
        if (buttons.Count == 0)
        {
            // The accessor has already tried to make the buttons readable (on macOS, by bringing the Editor to the
            // front), so they cannot be read and the dialog has to be closed by the user.
            var titles = string.Join(", ", dialogs.Select(d => $"\"{d.title}\""));
            throw new InvalidOperationException(
                $"No button labeled \"{buttonLabel}\" is found in the open dialogs ({titles}), and none of their " +
                "buttons could be read. Ask the user to close them.");
        }

        var available = string.Join(", ", buttons.Select(b => $"\"{b}\""));
        throw new InvalidOperationException(
            $"No button labeled \"{buttonLabel}\" is found in the open dialogs. Available buttons: {available}");
    }
}
