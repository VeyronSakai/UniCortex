using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SelectProjectWindowAssetResponse
    {
        public bool success;

        public SelectProjectWindowAssetResponse(bool success)
        {
            this.success = success;
        }
    }
}
