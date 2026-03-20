using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class RemoveTimelineTrackRequest
    {
        public int instanceId;
        public int trackIndex;
    }
}
