using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class PackageManagerListRequest
    {
        public bool offlineMode;
        public bool includeIndirectDependencies;
    }
}
