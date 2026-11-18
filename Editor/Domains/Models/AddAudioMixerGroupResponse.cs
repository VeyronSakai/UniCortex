using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class AddAudioMixerGroupResponse
    {
        public bool success;
        public string path;

        public AddAudioMixerGroupResponse(bool success, string path)
        {
            this.success = success;
            this.path = path;
        }
    }
}
