using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class ConfigureRecorderRequest
    {
        public string outputPath;
        public string source;
        public string cameraSource;
        public string cameraTag;
        public bool captureUI;
        public int outputWidth;
        public int outputHeight;
        public string outputFormat;
    }
}
