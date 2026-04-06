using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class StopMovieRecordingResponse
    {
        public string outputPath;

        public StopMovieRecordingResponse(string outputPath)
        {
            this.outputPath = outputPath;
        }
    }
}
