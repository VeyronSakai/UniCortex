using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class ComponentInfoEntry
    {
        public string type;
        public int index;

        public ComponentInfoEntry(string type, int index)
        {
            this.type = type;
            this.index = index;
        }
    }
}
