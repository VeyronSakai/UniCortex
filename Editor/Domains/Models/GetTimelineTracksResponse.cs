using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetTimelineTracksResponse
    {
        public string assetPath;
        public double duration;
        public double frameRate;
        public List<TimelineTrackEntry> tracks;

        public GetTimelineTracksResponse(string assetPath, double duration, double frameRate,
            List<TimelineTrackEntry> tracks)
        {
            this.assetPath = assetPath;
            this.duration = duration;
            this.frameRate = frameRate;
            this.tracks = tracks;
        }
    }
}
