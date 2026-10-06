using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class DragPointerRequest
    {
        // Start of the drag.
        public float? fromX;
        public float? fromY;
        public int? fromInstanceId;

        // End of the drag.
        public float? toX;
        public float? toY;
        public int? toInstanceId;

        public string button;
        // Seconds to move from the start to the end.
        public float? duration;

        // Seconds to keep the button pressed at the start before moving.
        public float? holdDuration;
    }
}
