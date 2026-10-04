using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SendMouseEventResponse
    {
        public bool success;

        // Position the event was sent to, in Game View coordinates.
        public float x;
        public float y;

        // Set only when a target is given: true when other UI covers the target at its center.
        public bool blocked;

        public SendMouseEventResponse(bool success, float x, float y, bool blocked = false)
        {
            this.success = success;
            this.x = x;
            this.y = y;
            this.blocked = blocked;
        }
    }
}
