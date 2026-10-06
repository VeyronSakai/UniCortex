using System;

namespace UniCortex.Editor.Domains.Models
{
    // Request for click, press and release of a pointer button at the center of a GameObject.
    [Serializable]
    public class GameObjectButtonRequest
    {
        public int instanceId;
        public string button;
    }
}
