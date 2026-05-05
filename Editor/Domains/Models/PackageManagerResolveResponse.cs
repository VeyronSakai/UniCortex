using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class PackageManagerResolveResponse
    {
        public bool success;

        public PackageManagerResolveResponse(bool success)
        {
            this.success = success;
        }
    }
}
