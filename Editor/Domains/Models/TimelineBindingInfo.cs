using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class TimelineBindingInfo
    {
        public int trackIndex;
        public string trackName;
        public string sourceType;
        public string boundObjectName;
        public int boundObjectInstanceId;
    }
}
