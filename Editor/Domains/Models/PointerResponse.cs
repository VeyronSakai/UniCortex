using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class PointerResponse
    {
        public bool success;

        // Position the event was sent to, in Game View coordinates.
        public float x;
        public float y;

        public PointerResponse(bool success, float x, float y)
        {
            this.success = success;
            this.x = x;
            this.y = y;
        }
    }
}
