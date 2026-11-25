using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class UnpackPrefabInstanceResponse
    {
        public bool success;

        public UnpackPrefabInstanceResponse(bool success)
        {
            this.success = success;
        }
    }
}
