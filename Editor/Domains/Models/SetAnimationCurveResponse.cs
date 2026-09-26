using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SetAnimationCurveResponse
    {
        public bool success;

        public SetAnimationCurveResponse(bool success)
        {
            this.success = success;
        }
    }
}
