using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class StartMovieRecordingResponse
    {
        public bool success;

        public StartMovieRecordingResponse(bool success)
        {
            this.success = success;
        }
    }
}
