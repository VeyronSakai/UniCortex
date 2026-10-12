using UniCortex.Core.Domains.Models;

namespace UniCortex.Core.Domains.Interfaces;

/// <summary>
/// Reads and operates the modal dialogs of a process through the OS window APIs.
/// It does not go through the Unity Editor's HTTP server, so it works while the Editor's main thread is blocked
/// by a modal dialog.
/// </summary>
public interface IModalDialogAccessor
{
    IReadOnlyList<ModalDialog> GetDialogs(int processId);

    /// <summary>
    /// Clicks the first button labeled <paramref name="buttonLabel"/> in the open modal dialogs.
    /// Returns the dialog that had the button, or null if no open dialog has it.
    /// </summary>
    ModalDialog? ClickButton(int processId, string buttonLabel);
}
