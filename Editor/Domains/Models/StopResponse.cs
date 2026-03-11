using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class StopResponse
    {
        public bool success;

        public StopResponse(bool success)
        {
            this.success = success;
        }
    }
}
