using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SetTimelineClipPropertyRequest
    {
        public int instanceId;
        public int trackIndex;
        public int clipIndex;
        public string propertyPath;
        public string value;
    }
}
