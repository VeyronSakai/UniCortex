using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class CreateAnimationClipResponse
    {
        public bool success;
        public string assetPath;

        public CreateAnimationClipResponse(bool success, string assetPath)
        {
            this.success = success;
            this.assetPath = assetPath;
        }
    }
}
