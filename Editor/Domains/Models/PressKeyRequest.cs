using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class PressKeyRequest
    {
        // Keys pressed together, e.g. ["LeftCtrl", "S"] for Ctrl+S.
        public string[] keys;

        // Seconds to keep the keys pressed before releasing them.
        public float? holdDuration;
    }
}
