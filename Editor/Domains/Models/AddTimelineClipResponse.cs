using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class AddTimelineClipResponse
    {
        public bool success;

        public AddTimelineClipResponse(bool success)
        {
            this.success = success;
        }
    }
}
