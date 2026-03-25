using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class ClosePrefabResponse
    {
        public bool success;

        public ClosePrefabResponse(bool success)
        {
            this.success = success;
        }
    }
}
