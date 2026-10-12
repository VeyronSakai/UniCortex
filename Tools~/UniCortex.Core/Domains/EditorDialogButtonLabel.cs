namespace UniCortex.Core.Domains;

internal static class EditorDialogButtonLabel
{
    /// <summary>
    /// Whether the label of a dialog button matches the requested label, ignoring case and surrounding spaces.
    /// </summary>
    internal static bool Matches(string label, string requested)
    {
        return string.Equals(label.Trim(), requested.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
