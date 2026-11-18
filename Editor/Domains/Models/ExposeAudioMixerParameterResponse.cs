using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class ExposeAudioMixerParameterResponse
    {
        public bool success;

        public ExposeAudioMixerParameterResponse(bool success)
        {
            this.success = success;
        }
    }
}
