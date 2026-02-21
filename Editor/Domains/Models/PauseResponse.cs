using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class PauseResponse
    {
        public bool success;

        public PauseResponse(bool success)
        {
            this.success = success;
        }
    }
}
