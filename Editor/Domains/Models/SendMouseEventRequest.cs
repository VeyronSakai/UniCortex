using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SendMouseEventRequest
    {
        public float? x;
        public float? y;
        public int? targetInstanceId;
        public string targetPath;
        public string button;
        public string eventType;
    }
}
