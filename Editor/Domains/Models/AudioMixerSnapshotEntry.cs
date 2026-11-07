using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class AudioMixerSnapshotEntry
    {
        public string name;
        public bool isStartSnapshot;

        public AudioMixerSnapshotEntry(string name, bool isStartSnapshot)
        {
            this.name = name;
            this.isStartSnapshot = isStartSnapshot;
        }
    }
}
