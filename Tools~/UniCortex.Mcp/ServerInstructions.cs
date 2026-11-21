using UniCortex.Core.Infrastructures;

namespace UniCortex.Mcp;

internal static class ServerInstructions
{
    /// <summary>
    /// Tells the agent at startup that the Unity Editor's dialogs cannot be read, so that it can tell the user before
    /// a dialog blocks the Editor. Returns null when there is nothing to tell.
    /// </summary>
    public static string? Create()
    {
        if (!OperatingSystem.IsMacOS() || MacEditorDialogAccessor.HasAccessibilityPermission())
        {
            return null;
        }

        return
            "The app that runs this MCP server does not have the macOS Accessibility permission, so UniCortex cannot " +
            "read or close the Unity Editor's modal dialogs (get_editor_dialogs, click_editor_dialog_button). While " +
            "a dialog is open, the Editor does not respond, and a tool call waits until it times out (10 minutes) " +
            "instead of failing with the dialog's contents. Let the user know, so that they can grant it: " +
            MacEditorDialogAccessor.HowToGrantAccessibilityPermission;
    }
}
