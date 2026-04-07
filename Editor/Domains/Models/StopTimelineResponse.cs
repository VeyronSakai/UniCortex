using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class StopTimelineResponse
    {
        public bool success;

        public StopTimelineResponse(bool success)
        {
            this.success = success;
        }
    }
}
