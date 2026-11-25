using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetComponentPropertiesRequest
    {
        public int instanceId;
        public string componentType;
        public string assemblyName;
        public int componentIndex;

        // Lists only this property and its children. Null lists every property of the component.
        public string propertyPath;

        // How many levels of children of arrays and nested serializable values to list. Null lists every level.
        public int? depth;
    }
}
