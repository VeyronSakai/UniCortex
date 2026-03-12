using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SendInputSystemKeyEventResponse
    {
        public bool success;

        public SendInputSystemKeyEventResponse(bool success)
        {
            this.success = success;
        }
    }
}
