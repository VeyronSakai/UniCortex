using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class AddMovieRecorderRequest
    {
        public string name;
        public string outputPath;
        public string encoder;
        public string encodingQuality;
        public bool captureAudio;
    }
}
