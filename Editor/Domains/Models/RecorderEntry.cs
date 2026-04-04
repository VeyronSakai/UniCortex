using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class RecorderEntry
    {
        public int index;
        public string name;
        public bool enabled;
        public string outputPath;
        public string encoder;
        public string encodingQuality;
        public string[] errors;

        public RecorderEntry(int index, string name, bool enabled, string outputPath,
            string encoder, string encodingQuality, string[] errors)
        {
            this.index = index;
            this.name = name;
            this.enabled = enabled;
            this.outputPath = outputPath;
            this.encoder = encoder;
            this.encodingQuality = encodingQuality;
            this.errors = errors;
        }
    }
}
