using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class AudioMixerExposedParameterEntry
    {
        public string name;
        public string groupPath;
        public string parameter;

        public AudioMixerExposedParameterEntry(string name, string groupPath, string parameter)
        {
            this.name = name;
            this.groupPath = groupPath;
            this.parameter = parameter;
        }
    }
}
