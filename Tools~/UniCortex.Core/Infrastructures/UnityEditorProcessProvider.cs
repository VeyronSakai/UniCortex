using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using UniCortex.Core.Domains.Interfaces;

namespace UniCortex.Core.Infrastructures;

public sealed class UnityEditorProcessProvider : IUnityEditorProcessProvider
{
    // The executable of the Unity Editor is Unity.app/Contents/MacOS/Unity on macOS and Unity.exe on Windows.
    private const string EditorProcessName = "Unity";

    // The number of symbolic links followed in a path, which stops a loop of links (MAXSYMLINKS on macOS).
    private const int MaxSymbolicLinks = 32;

    private static StringComparison PathComparison =>
        OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

    public int GetProcessId()
    {
        var projectPath = Environment.GetEnvironmentVariable("UNICORTEX_PROJECT_PATH");
        if (projectPath is null)
        {
            throw new InvalidOperationException(
                "UNICORTEX_PROJECT_PATH environment variable is not set. " +
                "Set it to your Unity project path to find the Unity Editor process.");
        }

        return GetProcessId(projectPath, GetUnityProcesses);
    }

    /// <param name="projectPath">The path of the Unity project.</param>
    /// <param name="getUnityProcesses">Returns the running Unity processes with their command-line arguments.</param>
    internal static int GetProcessId(string projectPath, Func<IEnumerable<UnityProcess>> getUnityProcesses)
    {
        try
        {
            return ReadProcessIdFromEditorInstance(projectPath);
        }
        catch (InvalidOperationException)
        {
            // Unity rewrites EditorInstance.json only after the project has opened. While an Editor starts up (e.g.
            // while it shows the "Recovering Scene Backups" dialog), the file is missing or still holds the process ID
            // of the previous session, so the Editor is found by its -projectPath instead.
            var processIds = FindEditorProcessIds(projectPath, getUnityProcesses());
            if (processIds.Count == 1)
            {
                return processIds[0];
            }

            throw;
        }
    }

    /// <summary>
    /// Returns the IDs of the Unity Editor processes whose -projectPath is the project.
    /// </summary>
    internal static List<int> FindEditorProcessIds(string projectPath, IEnumerable<UnityProcess> processes)
    {
        var normalizedProjectPath = NormalizePath(projectPath);
        return processes
            .Where(process => IsEditorOf(process.Arguments, normalizedProjectPath))
            .Select(process => process.ProcessId)
            .ToList();
    }

    /// <summary>
    /// Returns the full path with the symbolic links resolved (e.g. /tmp is a link to /private/tmp on macOS), and
    /// without a trailing separator.
    /// </summary>
    internal static string NormalizePath(string path)
    {
        var linkCount = 0;
        return ResolveSymbolicLinks(Path.GetFullPath(path), ref linkCount);
    }

    private static string ResolveSymbolicLinks(string fullPath, ref int linkCount)
    {
        fullPath = Path.TrimEndingDirectorySeparator(fullPath);
        var root = Path.GetPathRoot(fullPath) ?? "";
        var resolved = root;

        // ResolveLinkTarget resolves only the last part of a path, so each part is resolved in turn.
        var parts = fullPath[root.Length..].Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            resolved = Path.Combine(resolved, part);
            if (linkCount >= MaxSymbolicLinks)
            {
                continue;
            }

            try
            {
                var target = new DirectoryInfo(resolved).ResolveLinkTarget(returnFinalTarget: false);
                if (target != null)
                {
                    // The target can have links in it as well, e.g. a link to /var/folders/... on macOS, where /var
                    // is a link to /private/var.
                    linkCount++;
                    resolved = ResolveSymbolicLinks(target.FullName, ref linkCount);
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // The path does not exist or cannot be read, so the rest of it is compared as it is.
            }
        }

        return resolved;
    }

    private static int ReadProcessIdFromEditorInstance(string projectPath)
    {
        // The Unity Editor writes its process ID to Library/EditorInstance.json while the project is open.
        var path = Path.Combine(projectPath, "Library", "EditorInstance.json");
        if (!File.Exists(path))
        {
            throw new InvalidOperationException(
                $"Unity Editor instance file not found: {path}. Make sure the Unity project is open.");
        }

        EditorInstance? instance;
        try
        {
            instance = JsonSerializer.Deserialize<EditorInstance>(File.ReadAllText(path));
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            // Unity may be writing the file.
            throw new InvalidOperationException($"Failed to read {path}: {ex.Message}", ex);
        }

        if (instance is null || instance.ProcessId <= 0)
        {
            throw new InvalidOperationException($"process_id is missing in {path}.");
        }

        if (!IsRunning(instance.ProcessId))
        {
            throw new InvalidOperationException(
                $"The Unity Editor process {instance.ProcessId} written in {path} is not running. " +
                "Make sure the Unity project is open.");
        }

        return instance.ProcessId;
    }

    private static bool IsEditorOf(IReadOnlyList<string> arguments, string normalizedProjectPath)
    {
        // The Asset Import Worker processes (-batchMode -name AssetImportWorkerN) have the same -projectPath as the
        // Editor. A process in batch mode shows no dialogs.
        if (arguments.Any(argument => argument.Equals("-batchMode", StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        // Unity reads its options ignoring case (e.g. Unity Hub has passed -projectpath).
        for (var i = 0; i < arguments.Count - 1; i++)
        {
            if (!arguments[i].Equals("-projectPath", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // A relative path is relative to the working directory of the process, which is not known here.
            var path = arguments[i + 1];
            return Path.IsPathFullyQualified(path) &&
                   string.Equals(NormalizePath(path), normalizedProjectPath, PathComparison);
        }

        return false;
    }

    private static List<UnityProcess> GetUnityProcesses()
    {
        var processes = new List<UnityProcess>();
        foreach (var process in Process.GetProcessesByName(EditorProcessName))
        {
            using (process)
            {
                var arguments = ProcessArgumentsReader.Read(process.Id);
                if (arguments != null)
                {
                    processes.Add(new UnityProcess(process.Id, arguments));
                }
            }
        }

        return processes;
    }

    private static bool IsRunning(int processId)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            return !process.HasExited;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    /// <summary>
    /// A running process with its command-line arguments, starting with the executable.
    /// </summary>
    internal sealed record UnityProcess(int ProcessId, IReadOnlyList<string> Arguments);

    private sealed class EditorInstance
    {
        [JsonPropertyName("process_id")]
        public int ProcessId { get; set; }
    }
}
