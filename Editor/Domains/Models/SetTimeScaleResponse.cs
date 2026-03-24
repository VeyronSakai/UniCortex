using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SetTimeScaleResponse
    {
        public bool success;
        public float timeScale;

        public SetTimeScaleResponse(bool success, float timeScale)
        {
            this.success = success;
            this.timeScale = timeScale;
        }
    }
}
