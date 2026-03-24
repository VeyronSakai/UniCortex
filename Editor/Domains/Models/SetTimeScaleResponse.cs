using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SetTimeScaleResponse
    {
        public bool success;

        public SetTimeScaleResponse(bool success)
        {
            this.success = success;
        }
    }
}
