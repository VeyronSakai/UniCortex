using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetTimelineClipPropertiesRequest
    {
        public int instanceId;
        public string? assetPath;
        public int trackIndex;
        public int clipIndex;
    }
}
