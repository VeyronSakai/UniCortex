using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SendInputSystemMouseEventRequest
    {
        public float x;
        public float y;
        public int button;
        public string eventType;
    }
}
