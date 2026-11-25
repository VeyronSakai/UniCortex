using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SerializedPropertyEntry
    {
        public string path;
        public string type;
        public string value;

        // For an object reference, what it points to (e.g. "Canvas/Button (UnityEngine.UI.Image)"), or "Missing" when
        // it is set but its target no longer exists. Unity writes an empty string otherwise; Core omits it.
        public string? reference;

        public SerializedPropertyEntry(string path, string type, string value, string? reference = null)
        {
            this.path = path;
            this.type = type;
            this.value = value;
            this.reference = reference;
        }
    }
}
