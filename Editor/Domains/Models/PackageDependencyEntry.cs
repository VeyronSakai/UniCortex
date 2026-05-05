using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class PackageDependencyEntry
    {
        public string name = string.Empty;
        public string version = string.Empty;

        public PackageDependencyEntry()
        {
        }

        public PackageDependencyEntry(string name, string version)
        {
            this.name = name;
            this.version = version;
        }
    }
}
