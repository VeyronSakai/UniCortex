using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class TimelineClipInfo
    {
        public string name;
        public double start;
        public double duration;
        public double end;
    }
}
