using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class EvaluateTimelineResponse
    {
        public bool success;

        public EvaluateTimelineResponse(bool success)
        {
            this.success = success;
        }
    }
}
