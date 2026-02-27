using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class RemoveComponentResponse
    {
        public bool success;

        public RemoveComponentResponse(bool success)
        {
            this.success = success;
        }
    }
}
