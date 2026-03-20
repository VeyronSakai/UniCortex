using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SetTimelineTimeRequest
    {
        public int instanceId;
        public double time;
    }
}
