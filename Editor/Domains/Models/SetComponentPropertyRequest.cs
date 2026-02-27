using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SetComponentPropertyRequest
    {
        public int instanceId;
        public string componentType;
        public string propertyPath;
        public string value;
    }
}
