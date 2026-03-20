using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class PauseTimelineResponse
    {
        public bool success;

        public PauseTimelineResponse(bool success)
        {
            this.success = success;
        }
    }
}
