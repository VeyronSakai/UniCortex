using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class CreateAssetResponse
    {
        public bool success;

        public CreateAssetResponse(bool success)
        {
            this.success = success;
        }
    }
}
