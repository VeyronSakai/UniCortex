using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class TimelineTrackInfo
    {
        public string name;
        public string type;
        public bool muted;
        public bool locked;
        public TimelineClipInfo[] clips;
    }
}
