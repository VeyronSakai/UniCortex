using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class PressKeyResponse
    {
        public bool success;

        public PressKeyResponse(bool success)
        {
            this.success = success;
        }
    }
}
