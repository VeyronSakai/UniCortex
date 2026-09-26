using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class RemoveAnimationCurveResponse
    {
        public bool success;

        public RemoveAnimationCurveResponse(bool success)
        {
            this.success = success;
        }
    }
}
