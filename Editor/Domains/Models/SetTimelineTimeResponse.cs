using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SetTimelineTimeResponse
    {
        public bool success;

        public SetTimelineTimeResponse(bool success)
        {
            this.success = success;
        }
    }
}
