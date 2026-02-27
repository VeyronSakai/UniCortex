using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class AddComponentResponse
    {
        public bool success;

        public AddComponentResponse(bool success)
        {
            this.success = success;
        }
    }
}
