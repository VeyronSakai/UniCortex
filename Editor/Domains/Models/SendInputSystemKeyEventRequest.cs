using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SendInputSystemKeyEventRequest
    {
        public string key;
        public string eventType;
    }
}
