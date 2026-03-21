using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class RefreshAssetDatabaseResponse
    {
        public bool success;

        public RefreshAssetDatabaseResponse(bool success)
        {
            this.success = success;
        }
    }
}
