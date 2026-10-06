using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class ClickPointerRequest
    {
        public float? x;
        public float? y;
        public int? instanceId;
        public string button;

        // Seconds to keep the button pressed before releasing it, e.g. for a long press.
        public float? holdDuration;
    }
}
