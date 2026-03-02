using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class ExecuteMenuItemResponse
    {
        public bool success;

        public ExecuteMenuItemResponse(bool success)
        {
            this.success = success;
        }
    }
}
