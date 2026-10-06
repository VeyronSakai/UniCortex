using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class MovePointerRequest
    {
        public float? x;
        public float? y;
        public int? instanceId;
    }
}
