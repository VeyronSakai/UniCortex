using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SaveResponse
    {
        public bool success;

        public SaveResponse(bool success)
        {
            this.success = success;
        }
    }
}
