using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class StopProfilerRecordingResponse
    {
        public bool success;

        public StopProfilerRecordingResponse(bool success)
        {
            this.success = success;
        }
    }
}
