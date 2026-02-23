using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class RedoResponse
    {
        public bool success;

        public RedoResponse(bool success)
        {
            this.success = success;
        }
    }
}
