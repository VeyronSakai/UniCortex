using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SetComponentPropertyResponse
    {
        public bool success;

        public SetComponentPropertyResponse(bool success)
        {
            this.success = success;
        }
    }
}
