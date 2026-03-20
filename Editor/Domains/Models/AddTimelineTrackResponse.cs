using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class AddTimelineTrackResponse
    {
        public bool success;

        public AddTimelineTrackResponse(bool success)
        {
            this.success = success;
        }
    }
}
