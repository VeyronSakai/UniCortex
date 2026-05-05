using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class PackageManagerRemoveResponse
    {
        public bool success;

        public PackageManagerRemoveResponse(bool success)
        {
            this.success = success;
        }
    }
}
