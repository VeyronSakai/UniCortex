using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SelectProjectViewAssetResponse
    {
        public bool success;

        public SelectProjectViewAssetResponse(bool success)
        {
            this.success = success;
        }
    }
}
