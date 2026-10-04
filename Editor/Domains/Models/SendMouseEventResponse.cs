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

        // Set only when a target is given. See PointerTarget.blocked / blockedBy.
        public bool targetBlocked;
        public string blockedBy;

        public SendMouseEventResponse(bool success, float x, float y, bool targetBlocked = false,
            string blockedBy = "")
        {
            this.success = success;
            this.x = x;
            this.y = y;
            this.targetBlocked = targetBlocked;
            this.blockedBy = blockedBy;
        }
    }
}
