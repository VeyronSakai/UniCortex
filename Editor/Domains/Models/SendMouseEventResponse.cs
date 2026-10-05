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

        // End position of a drag, in Game View coordinates. Same as x and y for the other event types.
        public float toX;
        public float toY;

        public SendMouseEventResponse(bool success, float x, float y, float toX, float toY)
        {
            this.success = success;
            this.x = x;
            this.y = y;
            this.toX = toX;
            this.toY = toY;
        }
    }
}
