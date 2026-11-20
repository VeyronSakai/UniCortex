namespace UniCortex.Core.Domains;

/// <summary>
/// Thrown on macOS when the Unity Editor's dialogs cannot be read because the process that runs UniCortex does not
/// have the Accessibility permission.
/// </summary>
public sealed class AccessibilityPermissionException() : InvalidOperationException(
    "The Accessibility permission is required to read the Unity Editor's dialogs. " + HowToGrant)
{
    public const string SettingsUrl = "x-apple.systempreferences:com.apple.preference.security?Privacy_Accessibility";

    public const string HowToGrant =
        "Allow the app that runs UniCortex (e.g. your terminal or AI agent app) in " +
        "System Settings > Privacy & Security > Accessibility (" + SettingsUrl + "), then restart it.";
}
