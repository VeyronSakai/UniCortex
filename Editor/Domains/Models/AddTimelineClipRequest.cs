using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class AddTimelineClipRequest
    {
        public int instanceId;
        public int trackIndex;
        public double start;
        public double duration;
        public string clipName;
    }
}
