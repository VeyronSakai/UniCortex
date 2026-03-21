using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetComponentPropertiesRequest
    {
        public int instanceId;
        public string componentType;
        public int componentIndex;
    }
}
