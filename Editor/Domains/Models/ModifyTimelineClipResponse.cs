using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class ModifyTimelineClipResponse
    {
        public bool success;

        public ModifyTimelineClipResponse(bool success)
        {
            this.success = success;
        }
    }
}
