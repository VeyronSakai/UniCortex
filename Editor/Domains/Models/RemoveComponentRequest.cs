using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class RemoveComponentRequest
    {
        public int instanceId;
        public string componentType;
        public string assemblyName;
        public int componentIndex;
    }
}
