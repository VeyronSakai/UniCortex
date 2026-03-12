using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SendMouseEventResponse
    {
        public bool success;

        public SendMouseEventResponse(bool success)
        {
            this.success = success;
        }
    }
}
