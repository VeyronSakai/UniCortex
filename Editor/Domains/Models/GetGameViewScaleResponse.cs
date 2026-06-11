using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetGameViewScaleResponse
    {
        public float scale;
        public float minScale;
        public float maxScale;

        public GetGameViewScaleResponse(float scale, float minScale, float maxScale)
        {
            this.scale = scale;
            this.minScale = minScale;
            this.maxScale = maxScale;
        }
    }
}
