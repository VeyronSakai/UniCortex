using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class ClearConsoleResponse
    {
        public bool success;

        public ClearConsoleResponse(bool success)
        {
            this.success = success;
        }
    }
}
