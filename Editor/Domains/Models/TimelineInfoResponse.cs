using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class TimelineInfoResponse
    {
        public string timelineAssetName;
        public double duration;
        public double currentTime;
        public bool isPlaying;
        public TimelineTrackInfo[] tracks;
        public TimelineBindingInfo[] bindings;
    }
}
