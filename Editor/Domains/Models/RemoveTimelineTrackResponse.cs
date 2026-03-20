using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class RemoveTimelineTrackResponse
    {
        public bool success;

        public RemoveTimelineTrackResponse(bool success)
        {
            this.success = success;
        }
    }
}
