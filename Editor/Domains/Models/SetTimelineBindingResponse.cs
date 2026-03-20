using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SetTimelineBindingResponse
    {
        public bool success;

        public SetTimelineBindingResponse(bool success)
        {
            this.success = success;
        }
    }
}
