using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class AddRecorderRequest
    {
        public string name;
        public string outputPath;
        public string encoder;
        public string encodingQuality;
    }
}
