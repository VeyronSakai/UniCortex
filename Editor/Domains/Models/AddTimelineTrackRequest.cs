using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class AddTimelineTrackRequest
    {
        public int instanceId;
        public string trackType;
        public string trackName;
    }
}
