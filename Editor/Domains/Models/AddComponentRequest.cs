using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class AddComponentRequest
    {
        public int instanceId;
        public string componentType;
    }
}
