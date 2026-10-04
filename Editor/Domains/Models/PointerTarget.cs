using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class PointerTarget
    {
        public string name;
        public string path;
        public int instanceId;

        // Game View coordinates (origin at the bottom-left), same as send_mouse_event.
        public float centerX;
        public float centerY;
        public ScreenRect rect;

        // Pointer events handled by the object (see PointerEventName).
        public List<string> events;

        public bool activeInHierarchy;

        // Selectable.IsInteractable() for Selectable objects; true for other objects.
        public bool interactable;

        // True when the topmost EventSystem raycast hit at the center is neither the object nor its child.
        public bool blocked;

        // Hierarchy path of the topmost hit when blocked. Empty when nothing was hit.
        public string blockedBy;

        public PointerTarget(string name, string path, int instanceId, float centerX, float centerY,
            ScreenRect rect, List<string> events, bool activeInHierarchy, bool interactable, bool blocked,
            string blockedBy)
        {
            this.name = name;
            this.path = path;
            this.instanceId = instanceId;
            this.centerX = centerX;
            this.centerY = centerY;
            this.rect = rect;
            this.events = events;
            this.activeInHierarchy = activeInHierarchy;
            this.interactable = interactable;
            this.blocked = blocked;
            this.blockedBy = blockedBy;
        }
    }
}
