using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class RemoveTimelineClipRequest
    {
        public int instanceId;
        public int trackIndex;
        public int clipIndex;
    }
}
