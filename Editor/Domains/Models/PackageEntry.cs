using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class PackageEntry
    {
        public string name = string.Empty;
        public string displayName = string.Empty;
        public string version = string.Empty;
        public string packageId = string.Empty;
        public string source = string.Empty;
        public string status = string.Empty;
        public bool isDirectDependency;
        public string resolvedPath = string.Empty;
        public string assetPath = string.Empty;
        public string description = string.Empty;
        public PackageDependencyEntry[] dependencies = Array.Empty<PackageDependencyEntry>();
        public string[] errors = Array.Empty<string>();

        public PackageEntry()
        {
        }

        public PackageEntry(
            string name,
            string displayName,
            string version,
            string packageId,
            string source,
            string status,
            bool isDirectDependency,
            string resolvedPath,
            string assetPath,
            string description,
            PackageDependencyEntry[] dependencies,
            string[] errors)
        {
            this.name = name;
            this.displayName = displayName;
            this.version = version;
            this.packageId = packageId;
            this.source = source;
            this.status = status;
            this.isDirectDependency = isDirectDependency;
            this.resolvedPath = resolvedPath;
            this.assetPath = assetPath;
            this.description = description;
            this.dependencies = dependencies;
            this.errors = errors;
        }
    }
}
