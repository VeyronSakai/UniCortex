using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class AssetRefreshResponse
    {
        public bool success;

        public AssetRefreshResponse(bool success)
        {
            this.success = success;
        }
    }
}
