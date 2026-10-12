using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using UniCortex.Core.Domains.Interfaces;

namespace UniCortex.Core.Infrastructures;

public sealed class UnityEditorProcessProvider : IUnityEditorProcessProvider
{
    public int GetProcessId()
    {
        var projectPath = Environment.GetEnvironmentVariable("UNICORTEX_PROJECT_PATH");
        if (projectPath is null)
        {
            throw new InvalidOperationException(
                "UNICORTEX_PROJECT_PATH environment variable is not set. " +
                "Set it to your Unity project path to find the Unity Editor process.");
        }

        return GetProcessId(projectPath);
    }

    internal static int GetProcessId(string projectPath)
    {
        // The Unity Editor writes its process ID to Library/EditorInstance.json while the project is open.
        var path = Path.Combine(projectPath, "Library", "EditorInstance.json");
        if (!File.Exists(path))
        {
            throw new InvalidOperationException(
                $"Unity Editor instance file not found: {path}. Make sure the Unity project is open.");
        }

        var instance = JsonSerializer.Deserialize<EditorInstance>(File.ReadAllText(path));
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

    private sealed class EditorInstance
    {
        [JsonPropertyName("process_id")]
        public int ProcessId { get; set; }
    }
}
