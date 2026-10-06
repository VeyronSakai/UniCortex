using System;

namespace UniCortex.Editor.Domains.Models
{
    // Request for click, press and release of a pointer button.
    [Serializable]
    public class PointerButtonRequest
    {
        public float? x;
        public float? y;
        public int? instanceId;
        public string button;
    }
}
