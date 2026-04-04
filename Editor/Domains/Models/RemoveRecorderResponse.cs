using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class RemoveRecorderResponse
    {
        public bool success;

        public RemoveRecorderResponse(bool success)
        {
            this.success = success;
        }
    }
}
