using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SetAudioSourceOutputResponse
    {
        public bool success;

        public SetAudioSourceOutputResponse(bool success)
        {
            this.success = success;
        }
    }
}
