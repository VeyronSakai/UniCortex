using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SetTimelineClipPropertyResponse
    {
        public bool success;

        public SetTimelineClipPropertyResponse(bool success)
        {
            this.success = success;
        }
    }
}
