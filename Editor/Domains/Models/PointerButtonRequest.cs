using System;

namespace UniCortex.Editor.Domains.Models
{
    // Request for click, press and release of a pointer button at Game View coordinates.
    [Serializable]
    public class PointerButtonRequest
    {
        public float x;
        public float y;
        public string button;
    }
}
