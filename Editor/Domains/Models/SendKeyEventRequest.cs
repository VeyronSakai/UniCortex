using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SendKeyEventRequest
    {
        public string key;
        public string eventType;
    }
}
