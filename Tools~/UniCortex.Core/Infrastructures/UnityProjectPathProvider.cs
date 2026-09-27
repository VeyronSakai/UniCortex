using UniCortex.Core.Domains.Interfaces;

namespace UniCortex.Core.Infrastructures;

public sealed class UnityProjectPathProvider : IUnityProjectPathProvider
{
    public string GetProjectPath()
    {
        var projectPath = Environment.GetEnvironmentVariable("UNICORTEX_PROJECT_PATH");
        if (string.IsNullOrEmpty(projectPath))
        {
            throw new InvalidOperationException(
                "UNICORTEX_PROJECT_PATH environment variable is not set. " +
                "Code graph tools read C# sources from the Unity project directory, " +
                "so UNICORTEX_PROJECT_PATH must point to the Unity project root.");
        }

        if (!Directory.Exists(projectPath))
        {
            throw new InvalidOperationException(
                $"UNICORTEX_PROJECT_PATH points to a directory that does not exist: {projectPath}");
        }

        return Path.GetFullPath(projectPath);
    }
}
