using System.Text.Json;
using UniCortex.Core.Domains.CodeGraph;

namespace UniCortex.Core.Infrastructures.CodeGraph;

/// <summary>
/// Discovers the C# sources of a Unity project: Assets/, embedded packages under Packages/,
/// and local packages referenced with "file:" in Packages/manifest.json. Mirrors Unity's
/// import rules for hidden folders (leading '.', trailing '~') and maps each file to the
/// assembly it is compiled into (nearest .asmdef, otherwise Assembly-CSharp[-Editor]).
/// </summary>
public static class UnitySourceLocator
{
    // Only pruned directly under a source root (e.g. a "file:" package that is a repository
    // root); Unity compiles code in same-named folders anywhere deeper.
    private static readonly HashSet<string> s_skippedRootDirectoryNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Library", "Temp", "Logs", "obj", "bin", "node_modules"
    };

    public static UnityProjectSource Locate(string projectPath)
    {
        var roots = CollectRoots(projectPath);

        var files = new List<SourceFileRecord>();
        // Ordinal: paths come from filesystem enumeration, so casing is consistent, and
        // case-insensitive comparison would drop distinct files on case-sensitive filesystems.
        var seenFiles = new HashSet<string>(StringComparer.Ordinal);
        var assemblyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var root in roots)
        {
            var asmdefNamesByDirectory = new Dictionary<string, string>(StringComparer.Ordinal);
            var csFiles = new List<string>();
            Walk(root, 0, csFiles, asmdefNamesByDirectory);

            foreach (var name in asmdefNamesByDirectory.Values)
            {
                assemblyNames.Add(name);
            }

            foreach (var csFile in csFiles)
            {
                if (!seenFiles.Add(csFile))
                {
                    continue;
                }

                var assemblyName = ResolveAssemblyName(csFile, root, asmdefNamesByDirectory);
                assemblyNames.Add(assemblyName);

                var info = new FileInfo(csFile);
                var relativePath = Path.GetRelativePath(projectPath, csFile).Replace('\\', '/');
                files.Add(new SourceFileRecord(
                    csFile, relativePath, assemblyName, info.LastWriteTimeUtc.Ticks, info.Length));
            }
        }

        files.Sort((a, b) => string.CompareOrdinal(a.RelativePath, b.RelativePath));

        return new UnityProjectSource { Files = files, AssemblyNames = assemblyNames };
    }

    private static List<string> CollectRoots(string projectPath)
    {
        var roots = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void AddRoot(string path)
        {
            // Trim the trailing separator (e.g. from a "file:../MyLib/" manifest entry);
            // otherwise the root never matches Path.GetDirectoryName results during
            // assembly-name resolution.
            var fullPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
            if (Directory.Exists(fullPath) && seen.Add(fullPath))
            {
                roots.Add(fullPath);
            }
        }

        AddRoot(Path.Combine(projectPath, "Assets"));

        var packagesDirectory = Path.Combine(projectPath, "Packages");
        if (Directory.Exists(packagesDirectory))
        {
            // Embedded packages: direct subdirectories of Packages/ containing a package.json.
            foreach (var directory in Directory.EnumerateDirectories(packagesDirectory))
            {
                if (File.Exists(Path.Combine(directory, "package.json")))
                {
                    AddRoot(directory);
                }
            }

            // Local packages referenced with "file:" in the manifest.
            foreach (var localPath in ReadLocalPackagePaths(packagesDirectory))
            {
                AddRoot(localPath);
            }
        }

        return roots;
    }

    private static IEnumerable<string> ReadLocalPackagePaths(string packagesDirectory)
    {
        var manifestPath = Path.Combine(packagesDirectory, "manifest.json");
        if (!File.Exists(manifestPath))
        {
            yield break;
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(File.ReadAllText(manifestPath));
        }
        catch (Exception)
        {
            yield break;
        }

        using (document)
        {
            if (!document.RootElement.TryGetProperty("dependencies", out var dependencies)
                || dependencies.ValueKind != JsonValueKind.Object)
            {
                yield break;
            }

            foreach (var dependency in dependencies.EnumerateObject())
            {
                var value = dependency.Value.GetString();
                if (value is null || !value.StartsWith("file:", StringComparison.Ordinal))
                {
                    continue;
                }

                var reference = value["file:".Length..];
                yield return Path.IsPathRooted(reference)
                    ? reference
                    : Path.Combine(packagesDirectory, reference);
            }
        }
    }

    private static void Walk(
        string directory, int depth, List<string> csFiles, Dictionary<string, string> asmdefNamesByDirectory)
    {
        List<string> entries;
        try
        {
            entries = Directory.EnumerateFiles(directory).ToList();
        }
        catch (Exception)
        {
            // Unreadable directory (permissions, races): skip it, keep indexing the rest.
            return;
        }

        foreach (var file in entries)
        {
            var fileName = Path.GetFileName(file);
            if (fileName.StartsWith('.'))
            {
                continue;
            }

            if (fileName.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            {
                csFiles.Add(file);
            }
            else if (fileName.EndsWith(".asmdef", StringComparison.OrdinalIgnoreCase))
            {
                var assemblyName = ReadAsmdefName(file);
                if (assemblyName is not null)
                {
                    asmdefNamesByDirectory[directory] = assemblyName;
                }
            }
        }

        List<string> subDirectories;
        try
        {
            subDirectories = Directory.EnumerateDirectories(directory).ToList();
        }
        catch (Exception)
        {
            return;
        }

        foreach (var subDirectory in subDirectories)
        {
            var name = Path.GetFileName(subDirectory);
            if (name.StartsWith('.') || name.EndsWith('~')
                || (depth == 0 && s_skippedRootDirectoryNames.Contains(name)))
            {
                continue;
            }

            // Skip symlinked directories to guarantee cycle-free traversal (roots themselves
            // may still be symlinks; reference such code via "file:" in manifest.json).
            try
            {
                if ((File.GetAttributes(subDirectory) & FileAttributes.ReparsePoint) != 0)
                {
                    continue;
                }
            }
            catch (Exception)
            {
                continue;
            }

            Walk(subDirectory, depth + 1, csFiles, asmdefNamesByDirectory);
        }
    }

    private static string? ReadAsmdefName(string asmdefPath)
    {
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(asmdefPath));
            if (document.RootElement.TryGetProperty("name", out var nameElement)
                && nameElement.ValueKind == JsonValueKind.String)
            {
                return nameElement.GetString();
            }
        }
        catch (Exception)
        {
            // Malformed asmdef: fall back to the default assembly names.
        }

        return null;
    }

    private static string ResolveAssemblyName(
        string csFile, string root, Dictionary<string, string> asmdefNamesByDirectory)
    {
        var directory = Path.GetDirectoryName(csFile);
        while (directory is not null && directory.Length >= root.Length)
        {
            if (asmdefNamesByDirectory.TryGetValue(directory, out var assemblyName))
            {
                return assemblyName;
            }

            if (string.Equals(directory, root, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            directory = Path.GetDirectoryName(directory);
        }

        var relative = Path.GetRelativePath(root, csFile).Replace('\\', '/');
        return relative.Split('/').Contains("Editor")
            ? "Assembly-CSharp-Editor"
            : "Assembly-CSharp";
    }
}
