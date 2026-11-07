using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class ExposeAudioMixerParameterRequest
    {
        public string assetPath;
        public string group;
        public string parameter;
        public string exposedName;
    }
}
