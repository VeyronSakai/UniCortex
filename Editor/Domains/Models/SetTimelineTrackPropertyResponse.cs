using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SetTimelineTrackPropertyResponse
    {
        public bool success;

        public SetTimelineTrackPropertyResponse(bool success)
        {
            this.success = success;
        }
    }
}
