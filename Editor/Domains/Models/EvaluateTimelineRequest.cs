using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class EvaluateTimelineRequest
    {
        public int instanceId;
        public double time;
    }
}
