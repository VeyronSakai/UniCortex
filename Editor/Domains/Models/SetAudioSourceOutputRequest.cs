using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SetAudioSourceOutputRequest
    {
        public int instanceId;
        public int componentIndex;
        public string assetPath;
        public string group;
    }
}
