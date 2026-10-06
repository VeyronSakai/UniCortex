using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class DragPointerRequest
    {
        // Start of the drag.
        public float? x;
        public float? y;
        public int? instanceId;

        // End of the drag.
        public float? toX;
        public float? toY;
        public int? toInstanceId;

        public string button;
        public int? frames;
        public int? holdFrames;
    }
}
