using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetTimeScaleResponse
    {
        public float timeScale;

        public GetTimeScaleResponse(float timeScale)
        {
            this.timeScale = timeScale;
        }
    }
}
