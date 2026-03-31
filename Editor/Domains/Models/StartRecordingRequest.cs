using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class StartRecordingRequest
    {
        public int fps;
        public string outputPath;
    }
}
