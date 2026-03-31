using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class StartRecordingRequest
    {
        public int fps;
        public string frameRatePlayback;
        public string recordMode;
        public float startTime;
        public float endTime;
        public int startFrame;
        public int endFrame;
        public int frameNumber;
    }
}
