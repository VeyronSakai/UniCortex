using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class StartProfilerRecordingResponse
    {
        public bool success;

        public StartProfilerRecordingResponse(bool success)
        {
            this.success = success;
        }
    }
}
