using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class CreateTimelineRequest
    {
        public int instanceId;
        public string assetPath;
    }
}
