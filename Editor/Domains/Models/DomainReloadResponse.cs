using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class DomainReloadResponse
    {
        public bool success;

        public DomainReloadResponse(bool success)
        {
            this.success = success;
        }
    }
}
