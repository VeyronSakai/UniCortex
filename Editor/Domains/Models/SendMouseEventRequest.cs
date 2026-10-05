using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SendMouseEventRequest
    {
        public float? x;
        public float? y;
        public int? instanceId;
        public string button;
        public string eventType;

        // Only for the drag event type.
        public float? toX;
        public float? toY;
        public int? toInstanceId;
        public int? frames;
        public int? holdFrames;
    }
}
