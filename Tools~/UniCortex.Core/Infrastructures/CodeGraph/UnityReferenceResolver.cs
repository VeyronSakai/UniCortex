using Microsoft.CodeAnalysis;

namespace UniCortex.Core.Infrastructures.CodeGraph;

/// <summary>
/// Collects metadata references for semantic analysis of Unity project sources, best-effort:
/// the host .NET runtime assemblies, precompiled script assemblies from Library/ScriptAssemblies
/// (excluding the ones compiled from the indexed sources), and the managed assemblies of the
/// Unity Editor installation when it can be located. Missing references only degrade call
/// resolution — indexing itself never fails because of them.
/// </summary>
internal static class UnityReferenceResolver
{
    public static (List<MetadataReference> References, bool UnityReferencesResolved) Resolve(
        string projectPath, IReadOnlySet<string> sourceAssemblyNames)
    {
        var references = new List<MetadataReference>();
        var seenAssemblyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void TryAdd(string dllPath)
        {
            var simpleName = Path.GetFileNameWithoutExtension(dllPath);
            if (sourceAssemblyNames.Contains(simpleName) || !seenAssemblyNames.Add(simpleName))
            {
                return;
            }

            try
            {
                references.Add(MetadataReference.CreateFromFile(dllPath));
            }
            catch (Exception)
            {
                seenAssemblyNames.Remove(simpleName);
            }
        }

        AddRuntimeReferences(TryAdd);

        var scriptAssembliesDirectory = Path.Combine(projectPath, "Library", "ScriptAssemblies");
        if (Directory.Exists(scriptAssembliesDirectory))
        {
            foreach (var dll in Directory.EnumerateFiles(scriptAssembliesDirectory, "*.dll"))
            {
                TryAdd(dll);
            }
        }

        var unityReferencesResolved = AddUnityEditorReferences(projectPath, TryAdd);

        return (references, unityReferencesResolved);
    }

    private static void AddRuntimeReferences(Action<string> tryAdd)
    {
        if (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") is not string trustedAssemblies)
        {
            return;
        }

        var runtimeDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location);
        foreach (var path in trustedAssemblies.Split(Path.PathSeparator))
        {
            // Only take the shared runtime assemblies, not the application's own DLLs.
            if (Path.GetDirectoryName(path) == runtimeDirectory)
            {
                tryAdd(path);
            }
        }
    }

    private static bool AddUnityEditorReferences(string projectPath, Action<string> tryAdd)
    {
        var editorVersion = ReadEditorVersion(projectPath);
        if (editorVersion is null)
        {
            return false;
        }

        var managedDirectory = FindManagedDirectory(editorVersion);
        if (managedDirectory is null)
        {
            return false;
        }

        // Reference only Managed/UnityEngine/ like Unity's generated .csproj files do.
        // The monolithic UnityEngine.dll/UnityEditor.dll at the Managed/ top level define
        // the same types as the module assemblies and would cause CS0433 ambiguities.
        var moduleDirectory = Path.Combine(managedDirectory, "UnityEngine");
        if (!Directory.Exists(moduleDirectory))
        {
            return false;
        }

        var found = false;
        foreach (var dll in Directory.EnumerateFiles(moduleDirectory, "*.dll"))
        {
            tryAdd(dll);
            found = true;
        }

        return found;
    }

    private static string? ReadEditorVersion(string projectPath)
    {
        var versionFile = Path.Combine(projectPath, "ProjectSettings", "ProjectVersion.txt");
        if (!File.Exists(versionFile))
        {
            return null;
        }

        try
        {
            foreach (var line in File.ReadLines(versionFile))
            {
                const string prefix = "m_EditorVersion:";
                if (line.StartsWith(prefix, StringComparison.Ordinal))
                {
                    return line[prefix.Length..].Trim();
                }
            }
        }
        catch (Exception)
        {
            // Unreadable version file: skip Unity references.
        }

        return null;
    }

    private static string? FindManagedDirectory(string editorVersion)
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var candidates = new[]
        {
            // macOS
            $"/Applications/Unity/Hub/Editor/{editorVersion}/Unity.app/Contents/Managed",
            // Windows
            $@"C:\Program Files\Unity\Hub\Editor\{editorVersion}\Editor\Data\Managed",
            // Linux
            Path.Combine(home, "Unity", "Hub", "Editor", editorVersion, "Editor", "Data", "Managed")
        };

        return candidates.FirstOrDefault(Directory.Exists);
    }
}
