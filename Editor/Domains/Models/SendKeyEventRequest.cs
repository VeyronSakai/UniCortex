using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SendKeyEventRequest
    {
        public string keyName;
        public string eventType;
    }
}
