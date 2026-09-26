using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SetTimelineTrackPropertyRequest
    {
        public int instanceId;
        public int trackIndex;
        public string propertyPath;
        public string value;
    }
}
