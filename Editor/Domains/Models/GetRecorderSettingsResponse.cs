using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetRecorderSettingsResponse
    {
        public string outputPath;
        public string source;
        public string cameraSource;
        public string cameraTag;
        public bool captureUI;
        public int outputWidth;
        public int outputHeight;
        public string outputFormat;

        public GetRecorderSettingsResponse(string outputPath, string source, string cameraSource,
            string cameraTag, bool captureUI, int outputWidth, int outputHeight, string outputFormat)
        {
            this.outputPath = outputPath;
            this.source = source;
            this.cameraSource = cameraSource;
            this.cameraTag = cameraTag;
            this.captureUI = captureUI;
            this.outputWidth = outputWidth;
            this.outputHeight = outputHeight;
            this.outputFormat = outputFormat;
        }
    }
}
