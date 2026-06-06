using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SetGameViewScaleResponse
    {
        public bool success;
        public float scale;

        public SetGameViewScaleResponse(bool success, float scale)
        {
            this.success = success;
            this.scale = scale;
        }
    }
}
