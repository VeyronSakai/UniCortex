using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetTimelineTracksRequest
    {
        public int instanceId;
        public string? assetPath;
    }
}
