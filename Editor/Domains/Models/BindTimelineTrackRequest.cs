using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class BindTimelineTrackRequest
    {
        public int instanceId;
        public int trackIndex;
        public int targetInstanceId;
    }
}
