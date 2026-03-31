using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class StartRecordingResponse
    {
        public bool success;

        public StartRecordingResponse(bool success)
        {
            this.success = success;
        }
    }
}
