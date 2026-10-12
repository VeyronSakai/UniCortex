using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.Domains.Models;

namespace UniCortex.Core.Infrastructures;

public sealed class UnsupportedEditorDialogAccessor : IEditorDialogAccessor
{
    public IReadOnlyList<EditorDialog> GetDialogs(int processId)
    {
        throw CreateException();
    }

    public EditorDialog? ClickButton(int processId, string buttonLabel)
    {
        throw CreateException();
    }

    private static PlatformNotSupportedException CreateException()
    {
        return new PlatformNotSupportedException("Reading the Unity Editor's dialogs is supported on macOS and Windows only.");
    }
}
