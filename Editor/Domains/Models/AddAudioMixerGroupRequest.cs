using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class AddAudioMixerGroupRequest
    {
        public string assetPath;
        public string groupName;
        public string parentGroup;
    }
}
