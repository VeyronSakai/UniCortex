using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SendKeyEventResponse
    {
        public bool success;

        public SendKeyEventResponse(bool success)
        {
            this.success = success;
        }
    }
}
