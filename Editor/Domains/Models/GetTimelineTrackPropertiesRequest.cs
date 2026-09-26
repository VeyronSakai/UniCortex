using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetTimelineTrackPropertiesRequest
    {
        public int instanceId;
        public string? assetPath;
        public int trackIndex;
    }
}
