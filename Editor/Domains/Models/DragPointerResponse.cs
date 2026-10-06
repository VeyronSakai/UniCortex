using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class DragPointerResponse
    {
        public bool success;

        // Start and end of the drag, in Game View coordinates.
        public float fromX;
        public float fromY;
        public float toX;
        public float toY;

        public DragPointerResponse(bool success, float fromX, float fromY, float toX, float toY)
        {
            this.success = success;
            this.fromX = fromX;
            this.fromY = fromY;
            this.toX = toX;
            this.toY = toY;
        }
    }
}
