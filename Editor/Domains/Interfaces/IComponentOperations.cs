using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IComponentOperations
    {
        void AddComponent(int instanceId, string componentType);
        void RemoveComponent(int instanceId, string componentType, int componentIndex);
        ComponentPropertiesResponse GetProperties(int instanceId, string componentType, int componentIndex);
        void SetProperty(int instanceId, string componentType, string propertyPath, string value);
    }
}
