using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace EditorBridge.Editor.Installer
{
    [InitializeOnLoad]
    internal static class CliInstaller
    {
        [Serializable]
        private class PackageJson
        {
            public string version;
        }

        private static readonly string[] s_dotnetSearchPaths =
        {
            "/usr/local/share/dotnet/dotnet",
            "/opt/homebrew/bin/dotnet",
            "/usr/local/bin/dotnet"
        };

        private static string s_dotnetPath;

        static CliInstaller()
        {
            Install();
        }

        private static void Install()
        {
            if (!IsDotnetAvailable())
            {
                Debug.LogWarning("[EditorBridge] dotnet CLI not found. Skipping CLI installation.");
                return;
            }

            var projectRoot = Path.GetDirectoryName(Application.dataPath);
            var packageDir = GetPackageDirectory();
            if (packageDir == null)
            {
                Debug.LogError("[EditorBridge] Package directory not found. Skipping CLI installation.");
                return;
            }

            var version = GetPackageVersion(packageDir);
            if (string.IsNullOrEmpty(version))
            {
                Debug.LogError("[EditorBridge] Failed to read package version. Skipping CLI installation.");
                return;
            }

            if (NeedsNupkgRebuild(projectRoot, version))
            {
                if (!PackCli(projectRoot, packageDir, version))
                {
                    return;
                }
            }

            if (!EnsureToolManifest(projectRoot))
            {
                return;
            }

            var installedVersion = GetInstalledToolVersion(projectRoot);

            if (installedVersion == version)
            {
                return;
            }

            if (installedVersion == null)
            {
                InstallTool(projectRoot, version);
            }
            else
            {
                UpdateTool(projectRoot, version);
            }
        }

        private static bool IsDotnetAvailable()
        {
            s_dotnetPath = FindDotnetPath();
            return s_dotnetPath != null;
        }

        private static string FindDotnetPath()
        {
            // Try "dotnet" directly first (works if PATH is set)
            var (success, _) = RunDotnetWithPath("dotnet", "--version", null);
            if (success)
            {
                return "dotnet";
            }

            // On macOS, Unity doesn't inherit the shell PATH.
            // Search well-known installation locations.
            foreach (var candidate in s_dotnetSearchPaths)
            {
                if (!File.Exists(candidate))
                {
                    continue;
                }

                var (ok, _) = RunDotnetWithPath(candidate, "--version", null);
                if (ok)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static string GetPackageDirectory()
        {
            var packageDir = Path.GetFullPath(Path.Combine(
                "Packages",
                "com.veyron-sakai.editor-bridge"));
            return Directory.Exists(packageDir) ? packageDir : null;
        }

        private static string GetPackageVersion(string packageDir)
        {
            var fullPath = Path.Combine(packageDir, "package.json");

            if (!File.Exists(fullPath))
            {
                return null;
            }

            var json = File.ReadAllText(fullPath);
            var packageJson = JsonUtility.FromJson<PackageJson>(json);
            return packageJson?.version;
        }

        private static bool NeedsNupkgRebuild(string projectRoot, string version)
        {
            var nupkgPath = Path.Combine(
                projectRoot,
                "Library",
                "EditorBridge",
                "nupkg",
                $"UnityEditorBridge.CLI.{version}.nupkg");
            return !File.Exists(nupkgPath);
        }

        private static bool PackCli(string projectRoot, string packageDir, string version)
        {
            var csprojDir = Path.Combine(packageDir, "Tools~", "UnityEditorBridge.CLI");
            var outputDir = Path.Combine(projectRoot, "Library", "EditorBridge", "nupkg");

            var arguments = $"pack \"{csprojDir}\" -c Release -o \"{outputDir}\" /p:Version={version}";
            var (success, output) = RunDotnet(arguments, projectRoot);
            if (!success)
            {
                Debug.LogError($"[EditorBridge] dotnet pack failed: {output}");
            }

            return success;
        }

        private static bool EnsureToolManifest(string projectRoot)
        {
            var manifestPath = Path.Combine(projectRoot, ".config", "dotnet-tools.json");
            if (File.Exists(manifestPath))
            {
                return true;
            }

            var (success, output) = RunDotnet("new tool-manifest", projectRoot);
            if (!success)
            {
                Debug.LogError($"[EditorBridge] Failed to create tool manifest: {output}");
            }

            return success;
        }

        private static string GetInstalledToolVersion(string projectRoot)
        {
            var (success, output) = RunDotnet("tool list --local", projectRoot);
            if (!success)
            {
                return null;
            }

            using var reader = new StringReader(output);
            while (reader.ReadLine() is { } line)
            {
                if (line.StartsWith("unityeditorbridge.cli", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    return parts.Length >= 2 ? parts[1] : null;
                }
            }

            return null;
        }

        private static void InstallTool(string projectRoot, string version)
        {
            var nupkgDir = Path.Combine(projectRoot, "Library", "EditorBridge", "nupkg");
            var arguments =
                $"tool install UnityEditorBridge.CLI --local --version {version} --add-source \"{nupkgDir}\"";
            var (success, output) = RunDotnet(arguments, projectRoot);
            if (success)
            {
                Debug.Log("[EditorBridge] CLI installed: dotnet ueb");
            }
            else
            {
                Debug.LogError($"[EditorBridge] dotnet tool install failed: {output}");
            }
        }

        private static void UpdateTool(string projectRoot, string version)
        {
            var nupkgDir = Path.Combine(projectRoot, "Library", "EditorBridge", "nupkg");
            var arguments =
                $"tool update UnityEditorBridge.CLI --local --version {version} --add-source \"{nupkgDir}\"";
            var (success, output) = RunDotnet(arguments, projectRoot);
            if (success)
            {
                Debug.Log("[EditorBridge] CLI installed: dotnet ueb");
            }
            else
            {
                Debug.LogError($"[EditorBridge] dotnet tool update failed: {output}");
            }
        }

        private static (bool success, string output) RunDotnet(string arguments, string workingDirectory)
        {
            return RunDotnetWithPath(s_dotnetPath, arguments, workingDirectory);
        }

        private static (bool success, string output) RunDotnetWithPath(string fileName, string arguments,
            string workingDirectory)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                if (workingDirectory != null)
                {
                    startInfo.WorkingDirectory = workingDirectory;
                }

                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    return (false, "Failed to start dotnet process.");
                }

                var stdout = process.StandardOutput.ReadToEnd();
                var stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();

                var output = string.IsNullOrEmpty(stderr) ? stdout : stderr;
                return (process.ExitCode == 0, output.Trim());
            }
            catch (Win32Exception)
            {
                return (false, "dotnet not found.");
            }
        }
    }
}
