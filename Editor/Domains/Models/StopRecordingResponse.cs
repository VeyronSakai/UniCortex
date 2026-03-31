using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class StopRecordingResponse
    {
        public string outputPath;

        public StopRecordingResponse(string outputPath)
        {
            this.outputPath = outputPath;
        }
    }
}
