using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class AssetDatabaseRefreshResponse
    {
        public bool success;

        public AssetDatabaseRefreshResponse(bool success)
        {
            this.success = success;
        }
    }
}
