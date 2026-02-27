using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class ConsoleClearResponse
    {
        public bool success;

        public ConsoleClearResponse(bool success)
        {
            this.success = success;
        }
    }
}
