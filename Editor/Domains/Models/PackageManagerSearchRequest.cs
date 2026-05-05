using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class PackageManagerSearchRequest
    {
        public string packageIdOrName = string.Empty;
        public bool offlineMode;
    }
}
