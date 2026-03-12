using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SendInputSystemMouseEventResponse
    {
        public bool success;

        public SendInputSystemMouseEventResponse(bool success)
        {
            this.success = success;
        }
    }
}
