using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class PackageManagerPackagesResponse
    {
        public PackageEntry[] packages;

        public PackageManagerPackagesResponse(PackageEntry[] packages)
        {
            this.packages = packages;
        }
    }
}
