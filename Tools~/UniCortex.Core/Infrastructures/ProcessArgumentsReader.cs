using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace UniCortex.Core.Infrastructures;

/// <summary>
/// Reads the command-line arguments of another process, which .NET does not provide.
/// </summary>
internal static class ProcessArgumentsReader
{
    private const int CtlKern = 1;
    private const int KernArgMax = 8;
    private const int KernProcArgs2 = 49;

    private const uint ProcessQueryLimitedInformation = 0x1000;
    private const int ProcessCommandLineInformation = 60;

    /// <summary>
    /// Returns the arguments of the process, starting with the executable (argv[0]), or null if they cannot be read
    /// (e.g. the process has exited, it belongs to another user, or the OS is not supported).
    /// </summary>
    public static IReadOnlyList<string>? Read(int processId)
    {
        if (OperatingSystem.IsMacOS())
        {
            return ReadOnMac(processId);
        }

        if (OperatingSystem.IsWindows())
        {
            return ReadOnWindows(processId);
        }

        return null;
    }

    [SupportedOSPlatform("macos")]
    private static List<string>? ReadOnMac(int processId)
    {
        // KERN_ARGMAX is the largest size of the arguments and the environment variables of a process.
        var argMax = 0;
        var size = (nint)sizeof(int);
        if (sysctl([CtlKern, KernArgMax], 2, ref argMax, ref size, IntPtr.Zero, 0) != 0 || argMax <= 0)
        {
            return null;
        }

        var buffer = new byte[argMax];
        size = buffer.Length;
        if (sysctl([CtlKern, KernProcArgs2, processId], 3, buffer, ref size, IntPtr.Zero, 0) != 0)
        {
            return null;
        }

        return ParseProcArgs2(buffer.AsSpan(0, (int)size));
    }

    // KERN_PROCARGS2 returns argc, the path of the executable, null bytes that align the arguments, the arguments,
    // and then the environment variables. Each string ends with a null byte.
    private static List<string>? ParseProcArgs2(ReadOnlySpan<byte> data)
    {
        if (data.Length < sizeof(int))
        {
            return null;
        }

        var argc = MemoryMarshal.Read<int>(data);
        var rest = data[sizeof(int)..];
        var executableEnd = rest.IndexOf((byte)0);
        if (executableEnd < 0)
        {
            return null;
        }

        rest = rest[executableEnd..];
        var argumentsStart = rest.IndexOfAnyExcept((byte)0);
        if (argumentsStart < 0)
        {
            return null;
        }

        rest = rest[argumentsStart..];
        var arguments = new List<string>();
        for (var i = 0; i < argc; i++)
        {
            var end = rest.IndexOf((byte)0);
            if (end < 0)
            {
                return null;
            }

            arguments.Add(Encoding.UTF8.GetString(rest[..end]));
            rest = rest[(end + 1)..];
        }

        return arguments;
    }

    [SupportedOSPlatform("windows")]
    private static List<string>? ReadOnWindows(int processId)
    {
        var process = OpenProcess(ProcessQueryLimitedInformation, false, (uint)processId);
        if (process == IntPtr.Zero)
        {
            return null;
        }

        try
        {
            // The first call fails with STATUS_INFO_LENGTH_MISMATCH and returns the size of the buffer.
            NtQueryInformationProcess(process, ProcessCommandLineInformation, IntPtr.Zero, 0, out var length);
            if (length == 0)
            {
                return null;
            }

            var buffer = Marshal.AllocHGlobal((int)length);
            try
            {
                if (NtQueryInformationProcess(process, ProcessCommandLineInformation, buffer, length, out _) != 0)
                {
                    return null;
                }

                // The buffer starts with a UNICODE_STRING that points to the characters after it.
                var commandLine = Marshal.PtrToStructure<UnicodeString>(buffer);
                return SplitCommandLine(
                    Marshal.PtrToStringUni(commandLine.Buffer, commandLine.Length / sizeof(char)));
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        finally
        {
            CloseHandle(process);
        }
    }

    [SupportedOSPlatform("windows")]
    private static List<string>? SplitCommandLine(string commandLine)
    {
        // CommandLineToArgvW returns the path of the current executable for an empty string.
        if (commandLine.Length == 0)
        {
            return [];
        }

        var argv = CommandLineToArgvW(commandLine, out var argc);
        if (argv == IntPtr.Zero)
        {
            return null;
        }

        try
        {
            var arguments = new List<string>(argc);
            for (var i = 0; i < argc; i++)
            {
                arguments.Add(Marshal.PtrToStringUni(Marshal.ReadIntPtr(argv, i * IntPtr.Size)) ?? "");
            }

            return arguments;
        }
        finally
        {
            LocalFree(argv);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct UnicodeString
    {
        public readonly ushort Length;
        public readonly ushort MaximumLength;
        public readonly IntPtr Buffer;
    }

    [DllImport("libc")]
    private static extern int sysctl(int[] name, uint nameLength, ref int oldValue, ref nint oldLength,
        IntPtr newValue, nint newLength);

    [DllImport("libc")]
    private static extern int sysctl(int[] name, uint nameLength, [Out] byte[] oldValue, ref nint oldLength,
        IntPtr newValue, nint newLength);

    [DllImport("kernel32.dll")]
    private static extern IntPtr OpenProcess(uint desiredAccess, bool inheritHandle, uint processId);

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr handle);

    [DllImport("ntdll.dll")]
    private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass,
        IntPtr processInformation, uint processInformationLength, out uint returnLength);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr CommandLineToArgvW(string commandLine, out int argc);

    [DllImport("kernel32.dll")]
    private static extern IntPtr LocalFree(IntPtr memory);
}
