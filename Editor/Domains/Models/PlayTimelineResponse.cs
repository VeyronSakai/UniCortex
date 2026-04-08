using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class PlayTimelineResponse
    {
        public bool success;

        public PlayTimelineResponse(bool success)
        {
            this.success = success;
        }
    }
}
