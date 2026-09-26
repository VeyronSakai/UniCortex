using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class CreateAnimationClipRequest
    {
        public string assetPath;
        public bool loop;
        public float frameRate;
    }
}
