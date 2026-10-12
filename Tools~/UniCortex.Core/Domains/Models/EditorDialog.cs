namespace UniCortex.Core.Domains.Models;

/// <summary>
/// A modal dialog shown by the Unity Editor, read through the OS window APIs.
/// </summary>
public sealed class EditorDialog
{
    public string title = "";
    public string message = "";
    public List<string> buttons = new();
}
