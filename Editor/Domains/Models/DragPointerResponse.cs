using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class DragPointerResponse
    {
        public bool success;

        // Start and end of the drag, in Game View coordinates.
        public float x;
        public float y;
        public float toX;
        public float toY;

        public DragPointerResponse(bool success, float x, float y, float toX, float toY)
        {
            this.success = success;
            this.x = x;
            this.y = y;
            this.toX = toX;
            this.toY = toY;
        }
    }
}
