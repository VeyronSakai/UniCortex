using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.Domains.Models;

namespace UniCortex.Core.Infrastructures;

/// <summary>
/// Reads and operates modal dialogs with Win32 window messages.
/// Only dialogs of the standard dialog class (#32770), such as message boxes, are found.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsModalDialogAccessor : IModalDialogAccessor
{
    private const string DialogClassName = "#32770";
    private const uint WmCommand = 0x0111;
    private const int BnClicked = 0;

    public IReadOnlyList<ModalDialog> GetDialogs(int processId)
    {
        return FindDialogs(processId).Select(found => found.Dialog).ToList();
    }

    public ModalDialog? ClickButton(int processId, string buttonLabel)
    {
        foreach (var found in FindDialogs(processId))
        {
            for (var i = 0; i < found.Dialog.buttons.Count; i++)
            {
                if (!ModalDialogButtonLabel.Matches(found.Dialog.buttons[i], buttonLabel))
                {
                    continue;
                }

                // Notify the dialog as if the button were clicked. Unlike BM_CLICK, this works while the dialog is
                // not the active window. PostMessage does not wait for the dialog to handle it.
                var button = found.ButtonHandles[i];
                var wParam = (IntPtr)((BnClicked << 16) | (GetDlgCtrlID(button) & 0xFFFF));
                if (!PostMessage(found.Handle, WmCommand, wParam, button))
                {
                    throw new InvalidOperationException(
                        $"Failed to press the button \"{found.Dialog.buttons[i]}\" (Win32 error {Marshal.GetLastWin32Error()}).");
                }

                return found.Dialog;
            }
        }

        return null;
    }

    private static List<FoundDialog> FindDialogs(int processId)
    {
        var dialogs = new List<FoundDialog>();
        foreach (var window in EnumerateWindows(null))
        {
            GetWindowThreadProcessId(window, out var windowProcessId);
            if (windowProcessId != (uint)processId || !IsWindowVisible(window) ||
                GetClassName(window) != DialogClassName)
            {
                continue;
            }

            dialogs.Add(ReadDialog(window));
        }

        return dialogs;
    }

    private static FoundDialog ReadDialog(IntPtr window)
    {
        var found = new FoundDialog(window);
        found.Dialog.title = GetWindowText(window);

        var texts = new List<string>();
        foreach (var child in EnumerateWindows(window))
        {
            if (!IsWindowVisible(child))
            {
                continue;
            }

            var className = GetClassName(child);
            var text = GetWindowText(child);
            if (text.Length == 0)
            {
                continue;
            }

            if (className == "Static")
            {
                texts.Add(text);
            }
            else if (className == "Button")
            {
                found.Dialog.buttons.Add(RemoveMnemonic(text));
                found.ButtonHandles.Add(child);
            }
        }

        found.Dialog.message = string.Join("\n", texts);
        return found;
    }

    // "&Save" is shown as "Save" with an underlined S, and "&&" is shown as "&".
    private static string RemoveMnemonic(string text)
    {
        var builder = new StringBuilder(text.Length);
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '&' && i + 1 < text.Length)
            {
                i++;
            }

            builder.Append(text[i]);
        }

        return builder.ToString();
    }

    private static List<IntPtr> EnumerateWindows(IntPtr? parent)
    {
        var windows = new List<IntPtr>();
        var handle = GCHandle.Alloc(windows);
        try
        {
            if (parent is { } parentWindow)
            {
                EnumChildWindows(parentWindow, s_collectWindow, GCHandle.ToIntPtr(handle));
            }
            else
            {
                EnumWindows(s_collectWindow, GCHandle.ToIntPtr(handle));
            }
        }
        finally
        {
            handle.Free();
        }

        return windows;
    }

    private static readonly EnumWindowsProc s_collectWindow = CollectWindow;

    private static bool CollectWindow(IntPtr window, IntPtr state)
    {
        ((List<IntPtr>)GCHandle.FromIntPtr(state).Target!).Add(window);
        return true;
    }

    private static string GetClassName(IntPtr window)
    {
        var buffer = new StringBuilder(256);
        var length = GetClassName(window, buffer, buffer.Capacity);
        return length > 0 ? buffer.ToString() : "";
    }

    private static string GetWindowText(IntPtr window)
    {
        var length = GetWindowTextLength(window);
        if (length <= 0)
        {
            return "";
        }

        var buffer = new StringBuilder(length + 1);
        GetWindowText(window, buffer, buffer.Capacity);
        return buffer.ToString();
    }

    private sealed class FoundDialog(IntPtr handle)
    {
        public readonly IntPtr Handle = handle;
        public readonly ModalDialog Dialog = new();
        public readonly List<IntPtr> ButtonHandles = new();
    }

    [return: MarshalAs(UnmanagedType.Bool)]
    private delegate bool EnumWindowsProc(IntPtr window, IntPtr state);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr state);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumChildWindows(IntPtr parent, EnumWindowsProc callback, IntPtr state);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(IntPtr window);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetClassNameW")]
    private static extern int GetClassName(IntPtr window, StringBuilder className, int maxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetWindowTextLengthW")]
    private static extern int GetWindowTextLength(IntPtr window);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetWindowTextW")]
    private static extern int GetWindowText(IntPtr window, StringBuilder text, int maxCount);

    [DllImport("user32.dll")]
    private static extern int GetDlgCtrlID(IntPtr window);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "PostMessageW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PostMessage(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);
}
