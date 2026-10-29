using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class CreateAudioMixerResponse
    {
        public bool success;
        public string assetPath;

        public CreateAudioMixerResponse(bool success, string assetPath)
        {
            this.success = success;
            this.assetPath = assetPath;
        }
    }
}
