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
    // Title bar buttons of a window, which are not the buttons of the dialog itself.
    private static readonly string[] s_windowControlSubroles =
        ["AXCloseButton", "AXMinimizeButton", "AXZoomButton", "AXFullScreenButton"];

    private const int MaxDepth = 6;
    private const float MessagingTimeoutSeconds = 2f;

    // Element ids are given in order, about one for each new window: the Editor used about 40 while it started,
    // and none for a domain reload. The ids of closed windows have no element, so the scan goes on past them and
    // stops only after this many ids in a row without an element. Each id takes less than 0.1 milliseconds, so the
    // scan of a newly opened Editor took about 60 milliseconds.
    private const long MaxIdsWithoutElement = 1000;

    private const int AXErrorSuccess = 0;
    private const int AXErrorInvalidUIElement = -25202;
    private const int AXErrorCannotComplete = -25204;
    private const int AXErrorNoValue = -25212;
    private const int AXErrorAttributeUnsupported = -25205;

    private static bool s_canCreateElementFromRemoteToken = true;

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

        // AXWindows lists only the windows in the current Space. A modal dialog is usually the focused window of the
        // application, which is returned even when the Editor is in another Space.
        var candidates = ToElements(windows);
        CopyAttribute(application, "AXFocusedWindow", scope, out var focusedWindow);
        var isInOtherSpace = focusedWindow == IntPtr.Zero || !candidates.Any(window => CFEqual(window, focusedWindow));
        if (focusedWindow != IntPtr.Zero && isInOtherSpace)
        {
            candidates.Add(focusedWindow);
        }

        var dialogs = ReadDialogs(candidates, scope);

        // A dialog opened while the Editor is not the active application (e.g. the save panel of an untitled scene)
        // may not become the focused window, so it can be in neither attribute while the Editor is in another Space.
        if (dialogs.Count == 0 && isInOtherSpace)
        {
            dialogs = ReadDialogs(FindWindowsByElementId(processId, scope), scope);
        }

        // While the screen is locked, an open dialog may not be found, so "no dialog" cannot be reported.
        if (dialogs.Count == 0 && IsScreenLocked(scope))
        {
            throw new InvalidOperationException(
                "The screen is locked, so the Unity Editor's dialogs cannot be read. " +
                "Ask the user to unlock the screen, then try again.");
        }

        return dialogs;
    }

    private static bool IsScreenLocked(CFScope scope)
    {
        // Null when the process is not in a GUI login session (e.g. logged in with SSH).
        var session = scope.Own(CGSessionCopyCurrentDictionary());
        if (session == IntPtr.Zero)
        {
            return false;
        }

        // kCGSSessionScreenIsLocked is in the dictionary only while the screen is locked.
        var locked = CFDictionaryGetValue(session, scope.CreateString("CGSSessionScreenIsLocked"));
        return locked != IntPtr.Zero && CFGetTypeID(locked) == CFBooleanGetTypeID() && CFBooleanGetValue(locked);
    }

    private static List<FoundDialog> ReadDialogs(List<IntPtr> windows, CFScope scope)
    {
        var dialogs = new List<FoundDialog>();
        foreach (var window in windows)
        {
            if (IsModal(window, scope))
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

    // Finds the windows of the process in every Space by creating its AX elements from their ids with the private
    // _AXUIElementCreateWithRemoteToken, as window switchers do. No public attribute of the application leads to a
    // window that is in another Space and is not the focused window.
    private static List<IntPtr> FindWindowsByElementId(int processId, CFScope scope)
    {
        var windows = new List<IntPtr>();
        if (!s_canCreateElementFromRemoteToken)
        {
            return windows;
        }

        var roleAttribute = scope.CreateString("AXRole");
        var windowRole = scope.CreateString("AXWindow");
        var token = new RemoteToken(processId);
        var lastElementId = 0L;
        for (var id = 0L; id - lastElementId <= MaxIdsWithoutElement; id++)
        {
            token.ElementId = id;
            var data = CFDataCreate(IntPtr.Zero, ref token, Marshal.SizeOf<RemoteToken>());
            IntPtr element;
            try
            {
                element = _AXUIElementCreateWithRemoteToken(data);
            }
            catch (EntryPointNotFoundException)
            {
                // Removed in this version of macOS: the windows in other Spaces cannot be found.
                s_canCreateElementFromRemoteToken = false;
                return windows;
            }
            finally
            {
                CFRelease(data);
            }

            if (element == IntPtr.Zero)
            {
                continue;
            }

            // An id without an element returns kAXErrorInvalidUIElement at once.
            AXUIElementSetMessagingTimeout(element, MessagingTimeoutSeconds);
            var error = AXUIElementCopyAttributeValue(element, roleAttribute, out var role);
            if (error != AXErrorInvalidUIElement)
            {
                lastElementId = id;
            }

            var isWindow = false;
            if (error == AXErrorSuccess && role != IntPtr.Zero)
            {
                isWindow = CFEqual(role, windowRole);
                CFRelease(role);
            }

            if (isWindow)
            {
                windows.Add(scope.Own(element));
                continue;
            }

            CFRelease(element);

            // The Editor does not answer (its main thread is busy), so the other ids would time out as well.
            if (error == AXErrorCannotComplete)
            {
                break;
            }
        }

        return windows;
    }

    // The dialogs that block the Editor (EditorUtility.DisplayDialog, the save panel, EditorWindow.ShowModal, etc.) have
    // AXModal true. The subrole is not used: some windows that do not block the Editor (e.g. the "New Scene" window)
    // have the AXDialog subrole, and the Editor keeps answering requests while they are open.
    private static bool IsModal(IntPtr window, CFScope scope)
    {
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

    /// <summary>
    /// The 20 bytes that _AXUIElementCreateWithRemoteToken takes to identify an AX element of a process.
    /// Pack = 4 puts the element id right after the 12 bytes before it, with no padding.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct RemoteToken(int processId)
    {
        public readonly int ProcessId = processId;
        public readonly int Reserved = 0;
        public readonly int Magic = 0x636f636f; // "coco"
        public long ElementId;
    }

    private const string ApplicationServices =
        "/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices";

    private const string CoreFoundation = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

    private const string CoreGraphics = "/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";

    [DllImport(CoreGraphics)]
    private static extern IntPtr CGSessionCopyCurrentDictionary();

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

    // Private API: not in the SDK headers, and may be removed in a future version of macOS.
    [DllImport(ApplicationServices)]
    private static extern IntPtr _AXUIElementCreateWithRemoteToken(IntPtr data);

    [DllImport(CoreFoundation)]
    private static extern IntPtr CFDataCreate(IntPtr allocator, ref RemoteToken bytes, nint length);

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
    private static extern IntPtr CFDictionaryGetValue(IntPtr dictionary, IntPtr key);

    [DllImport(CoreFoundation)]
    private static extern void CFRelease(IntPtr cf);
}
