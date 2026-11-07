using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class AudioMixerGroupEntry
    {
        public string name;
        public string path;

        public AudioMixerGroupEntry(string name, string path)
        {
            this.name = name;
            this.path = path;
        }
    }
}
