using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IComponentOperations
    {
        void AddComponent(int instanceId, string componentType, string assemblyName);
        void RemoveComponent(int instanceId, string componentType, string assemblyName, int componentIndex);
        GetComponentPropertiesResponse GetProperties(int instanceId, string componentType, string assemblyName,
            int componentIndex);
        void SetProperty(int instanceId, string componentType, string assemblyName, string propertyPath, string value);
    }
}
