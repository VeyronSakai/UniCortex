using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class ConfigureRecorderResponse
    {
        public bool success;

        public ConfigureRecorderResponse(bool success)
        {
            this.success = success;
        }
    }
}
