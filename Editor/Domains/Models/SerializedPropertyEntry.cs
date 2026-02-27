using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SerializedPropertyEntry
    {
        public string path;
        public string type;
        public string value;

        public SerializedPropertyEntry(string path, string type, string value)
        {
            this.path = path;
            this.type = type;
            this.value = value;
        }
    }
}
