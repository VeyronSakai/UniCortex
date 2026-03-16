using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class UnpauseResponse
    {
        public bool success;

        public UnpauseResponse(bool success)
        {
            this.success = success;
        }
    }
}
