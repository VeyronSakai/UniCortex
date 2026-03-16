using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class StepResponse
    {
        public bool success;

        public StepResponse(bool success)
        {
            this.success = success;
        }
    }
}
