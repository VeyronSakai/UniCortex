using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IComponentOperations
    {
        void AddComponent(int instanceId, string componentType, string assemblyName);
        void RemoveComponent(int instanceId, string componentType, string assemblyName, int componentIndex);
        // Lists the properties of the component, or only the subtree at propertyPath when it is given, expanding
        // arrays and nested serializable values down to depth levels (every level when depth is null).
        GetComponentPropertiesResponse GetProperties(int instanceId, string componentType, string assemblyName,
            int componentIndex, string propertyPath, int? depth);
        void SetProperty(int instanceId, string componentType, string assemblyName, string propertyPath, string value);
    }
}
