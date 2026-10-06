using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class DragGameObjectRequest
    {
        public int fromInstanceId;
        public int toInstanceId;
        public string button;
        public int? frames;
        public int? holdFrames;
    }
}
