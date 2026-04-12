using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class FocusProfilerWindowResponse
    {
        public bool success;

        public FocusProfilerWindowResponse(bool success)
        {
            this.success = success;
        }
    }
}
