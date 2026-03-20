using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class RemoveTimelineClipResponse
    {
        public bool success;

        public RemoveTimelineClipResponse(bool success)
        {
            this.success = success;
        }
    }
}
