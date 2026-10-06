using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class MoveMouseRequest
    {
        public float? x;
        public float? y;
        public int? instanceId;
    }
}
