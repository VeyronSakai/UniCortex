using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class BindTimelineTrackResponse
    {
        public bool success;

        public BindTimelineTrackResponse(bool success)
        {
            this.success = success;
        }
    }
}
