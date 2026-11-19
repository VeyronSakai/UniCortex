namespace UniCortex.Core.Domains;

internal static class EditorDialogButtonLabel
{
    /// <summary>
    /// Tells the caller how to close a dialog whose buttons could not be read.
    /// </summary>
    internal const string UnreadButtonsHint =
        "On macOS, the buttons of a dialog that is not shown (e.g. the save panel while the Editor is in another " +
        "Space or hidden) are read only after the Editor is brought to the front, which " +
        "click_editor_dialog_button does (switching the user's Space to the Editor): call it with the button you " +
        "expect (e.g. \"Cancel\"), and if that is not found, the error lists the buttons read then.";

    /// <summary>
    /// Whether the label of a dialog button matches the requested label, ignoring case and surrounding spaces.
    /// </summary>
    internal static bool Matches(string label, string requested)
    {
        return string.Equals(label.Trim(), requested.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
