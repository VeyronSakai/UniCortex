using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.Domains.Models;

namespace UniCortex.Core.Infrastructures;

/// <summary>
/// Reads and operates modal dialogs with the macOS Accessibility API.
/// The process that runs this code needs the Accessibility permission.
/// </summary>
[SupportedOSPlatform("macos")]
public sealed class MacEditorDialogAccessor : IEditorDialogAccessor
{
    // Subroles of the windows that macOS treats as dialogs, e.g. the NSAlert window shown by
    // EditorUtility.DisplayDialog.
    private static readonly string[] s_dialogSubroles = ["AXDialog", "AXSystemDialog"];

    // Title bar buttons of a window, which are not the buttons of the dialog itself.
    private static readonly string[] s_windowControlSubroles =
        ["AXCloseButton", "AXMinimizeButton", "AXZoomButton", "AXFullScreenButton"];

    private const int MaxDepth = 6;
    private const float MessagingTimeoutSeconds = 2f;

    private const int AXErrorSuccess = 0;
    private const int AXErrorNoValue = -25212;
    private const int AXErrorAttributeUnsupported = -25205;

    public IReadOnlyList<EditorDialog> GetDialogs(int processId)
    {
        using var scope = new CFScope();
        return FindDialogs(processId, scope).Select(found => found.Dialog).ToList();
    }

    public EditorDialog? ClickButton(int processId, string buttonLabel)
    {
        using var scope = new CFScope();
        foreach (var found in FindDialogs(processId, scope))
        {
            for (var i = 0; i < found.Dialog.buttons.Count; i++)
            {
                if (!EditorDialogButtonLabel.Matches(found.Dialog.buttons[i], buttonLabel))
                {
                    continue;
                }

                var error = AXUIElementPerformAction(found.ButtonElements[i], scope.CreateString("AXPress"));
                if (error != AXErrorSuccess)
                {
                    throw new InvalidOperationException(
                        $"Failed to press the button \"{found.Dialog.buttons[i]}\" (AXError {error}).");
                }

                return found.Dialog;
            }
        }

        return null;
    }

    private static List<FoundDialog> FindDialogs(int processId, CFScope scope)
    {
        if (!AXIsProcessTrusted())
        {
            throw new InvalidOperationException(
                "The Accessibility permission is required to read the Unity Editor's dialogs. " +
                "Allow the app that runs UniCortex (e.g. your terminal or AI agent app) in " +
                "System Settings > Privacy & Security > Accessibility, then restart it.");
        }

        var application = scope.Own(AXUIElementCreateApplication(processId));
        AXUIElementSetMessagingTimeout(application, MessagingTimeoutSeconds);

        var error = CopyAttribute(application, "AXWindows", scope, out var windows);
        if (error != AXErrorSuccess && error != AXErrorNoValue)
        {
            throw new InvalidOperationException(
                $"Failed to read the windows of the Unity Editor process {processId} (AXError {error}).");
        }

        // AXWindows lists only the windows in the current Space. While a modal dialog is open, it is the focused
        // window of the application, which is returned even when the Editor is in another Space.
        var candidates = ToElements(windows);
        CopyAttribute(application, "AXFocusedWindow", scope, out var focusedWindow);
        if (focusedWindow != IntPtr.Zero && !candidates.Any(window => CFEqual(window, focusedWindow)))
        {
            candidates.Add(focusedWindow);
        }

        var dialogs = new List<FoundDialog>();
        foreach (var window in candidates)
        {
            if (IsDialog(window, scope))
            {
                dialogs.Add(ReadDialog(window, scope));
            }

            // A sheet is not listed in AXWindows but attached to its parent window as a child.
            foreach (var child in GetElements(window, "AXChildren", scope))
            {
                if (GetString(child, "AXRole", scope) == "AXSheet")
                {
                    dialogs.Add(ReadDialog(child, scope));
                }
            }
        }

        // A progress window (e.g. EditorUtility.DisplayCancelableProgressBar) has a Cancel button but does not wait
        // for a choice, so it is not reported as a dialog that blocks the Editor.
        return dialogs.Where(found => !found.HasProgressIndicator).ToList();
    }

    private static bool IsDialog(IntPtr window, CFScope scope)
    {
        var subrole = GetString(window, "AXSubrole", scope);
        if (subrole != null && s_dialogSubroles.Contains(subrole))
        {
            return true;
        }

        CopyAttribute(window, "AXModal", scope, out var modal);
        return modal != IntPtr.Zero && CFGetTypeID(modal) == CFBooleanGetTypeID() && CFBooleanGetValue(modal);
    }

    private static FoundDialog ReadDialog(IntPtr window, CFScope scope)
    {
        var found = new FoundDialog();
        var texts = new List<string>();
        CollectContents(window, 0, texts, found, scope);

        // An NSAlert has no window title. EditorUtility.DisplayDialog shows its title as the first text
        // and its message as the second.
        var windowTitle = GetString(window, "AXTitle", scope) ?? "";
        if (windowTitle.Length == 0 && texts.Count > 0)
        {
            windowTitle = texts[0];
            texts.RemoveAt(0);
        }

        found.Dialog.title = windowTitle;
        found.Dialog.message = string.Join("\n", texts);
        return found;
    }

    private static void CollectContents(IntPtr element, int depth, List<string> texts, FoundDialog found,
        CFScope scope)
    {
        if (depth > MaxDepth)
        {
            return;
        }

        foreach (var child in GetElements(element, "AXChildren", scope))
        {
            var role = GetString(child, "AXRole", scope);
            if (role == "AXProgressIndicator")
            {
                found.HasProgressIndicator = true;
            }
            else if (role == "AXStaticText")
            {
                var text = GetString(child, "AXValue", scope);
                if (!string.IsNullOrEmpty(text))
                {
                    texts.Add(text);
                }
            }
            else if (role == "AXButton")
            {
                var subrole = GetString(child, "AXSubrole", scope);
                var label = GetString(child, "AXTitle", scope);
                if ((subrole == null || !s_windowControlSubroles.Contains(subrole)) && !string.IsNullOrEmpty(label))
                {
                    found.Dialog.buttons.Add(label);
                    found.ButtonElements.Add(child);
                }
            }
            else
            {
                CollectContents(child, depth + 1, texts, found, scope);
            }
        }
    }

    private static int CopyAttribute(IntPtr element, string attribute, CFScope scope, out IntPtr value)
    {
        var error = AXUIElementCopyAttributeValue(element, scope.CreateString(attribute), out value);
        if (error != AXErrorSuccess)
        {
            value = IntPtr.Zero;
            return error;
        }

        scope.Own(value);
        return error;
    }

    private static string? GetString(IntPtr element, string attribute, CFScope scope)
    {
        CopyAttribute(element, attribute, scope, out var value);
        if (value == IntPtr.Zero || CFGetTypeID(value) != CFStringGetTypeID())
        {
            return null;
        }

        var length = CFStringGetLength(value);
        var buffer = new char[length];
        CFStringGetCharacters(value, new CFRange(0, length), buffer);
        return new string(buffer);
    }

    private static List<IntPtr> GetElements(IntPtr element, string attribute, CFScope scope)
    {
        var error = CopyAttribute(element, attribute, scope, out var value);
        if (error != AXErrorSuccess && error != AXErrorNoValue && error != AXErrorAttributeUnsupported)
        {
            return [];
        }

        return ToElements(value);
    }

    // The elements are owned by the array, which the scope keeps alive.
    private static List<IntPtr> ToElements(IntPtr array)
    {
        var elements = new List<IntPtr>();
        if (array == IntPtr.Zero || CFGetTypeID(array) != CFArrayGetTypeID())
        {
            return elements;
        }

        var count = CFArrayGetCount(array);
        for (nint i = 0; i < count; i++)
        {
            elements.Add(CFArrayGetValueAtIndex(array, i));
        }

        return elements;
    }

    private sealed class FoundDialog
    {
        public readonly EditorDialog Dialog = new();
        public readonly List<IntPtr> ButtonElements = new();
        public bool HasProgressIndicator;
    }

    /// <summary>
    /// Releases the Core Foundation objects created in a scope when it is disposed.
    /// </summary>
    private sealed class CFScope : IDisposable
    {
        private readonly List<IntPtr> _objects = new();

        public IntPtr Own(IntPtr cf)
        {
            if (cf != IntPtr.Zero)
            {
                _objects.Add(cf);
            }

            return cf;
        }

        public IntPtr CreateString(string value)
        {
            return Own(CFStringCreateWithCharacters(IntPtr.Zero, value, value.Length));
        }

        public void Dispose()
        {
            for (var i = _objects.Count - 1; i >= 0; i--)
            {
                CFRelease(_objects[i]);
            }

            _objects.Clear();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct CFRange(nint location, nint length)
    {
        public readonly nint Location = location;
        public readonly nint Length = length;
    }

    private const string ApplicationServices =
        "/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices";

    private const string CoreFoundation = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

    [DllImport(ApplicationServices)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool AXIsProcessTrusted();

    [DllImport(ApplicationServices)]
    private static extern IntPtr AXUIElementCreateApplication(int pid);

    [DllImport(ApplicationServices)]
    private static extern int AXUIElementSetMessagingTimeout(IntPtr element, float timeoutInSeconds);

    [DllImport(ApplicationServices)]
    private static extern int AXUIElementCopyAttributeValue(IntPtr element, IntPtr attribute, out IntPtr value);

    [DllImport(ApplicationServices)]
    private static extern int AXUIElementPerformAction(IntPtr element, IntPtr action);

    [DllImport(CoreFoundation, CharSet = CharSet.Unicode)]
    private static extern IntPtr CFStringCreateWithCharacters(IntPtr allocator, string chars, nint numChars);

    [DllImport(CoreFoundation)]
    private static extern nint CFStringGetLength(IntPtr str);

    // UniChar is UTF-16, so the buffer must not be marshaled as ANSI characters.
    [DllImport(CoreFoundation, CharSet = CharSet.Unicode)]
    private static extern void CFStringGetCharacters(IntPtr str, CFRange range, [Out] char[] buffer);

    [DllImport(CoreFoundation)]
    private static extern nuint CFGetTypeID(IntPtr cf);

    [DllImport(CoreFoundation)]
    private static extern nuint CFStringGetTypeID();

    [DllImport(CoreFoundation)]
    private static extern nuint CFArrayGetTypeID();

    [DllImport(CoreFoundation)]
    private static extern nuint CFBooleanGetTypeID();

    [DllImport(CoreFoundation)]
    private static extern nint CFArrayGetCount(IntPtr array);

    [DllImport(CoreFoundation)]
    private static extern IntPtr CFArrayGetValueAtIndex(IntPtr array, nint index);

    [DllImport(CoreFoundation)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool CFBooleanGetValue(IntPtr boolean);

    [DllImport(CoreFoundation)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool CFEqual(IntPtr cf1, IntPtr cf2);

    [DllImport(CoreFoundation)]
    private static extern void CFRelease(IntPtr cf);
}
