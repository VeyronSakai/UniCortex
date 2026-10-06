using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class PointerTarget
    {
        public string path;
        public int instanceId;

        // Game View coordinates (origin at the bottom-left), same as the mouse tools (click_mouse, etc.).
        public ScreenRect rect;

        public PointerTarget(string path, int instanceId, ScreenRect rect)
        {
            this.path = path;
            this.instanceId = instanceId;
            this.rect = rect;
        }
    }
}
