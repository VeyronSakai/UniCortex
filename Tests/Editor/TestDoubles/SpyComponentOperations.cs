using System.Collections.Generic;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyComponentOperations : IComponentOperations
    {
        public int AddComponentCallCount { get; private set; }
        public int LastAddComponentInstanceId { get; private set; }
        public string LastAddComponentType { get; private set; }

        public int RemoveComponentCallCount { get; private set; }
        public int LastRemoveComponentInstanceId { get; private set; }
        public string LastRemoveComponentType { get; private set; }
        public int LastRemoveComponentIndex { get; private set; }

        public int GetPropertiesCallCount { get; private set; }
        public int LastGetPropertiesInstanceId { get; private set; }
        public string LastGetPropertiesComponentType { get; private set; }
        public int LastGetPropertiesComponentIndex { get; private set; }
        public ComponentPropertiesResponse GetPropertiesResult { get; set; }

        public int SetPropertyCallCount { get; private set; }
        public int LastSetPropertyInstanceId { get; private set; }
        public string LastSetPropertyComponentType { get; private set; }
        public string LastSetPropertyPath { get; private set; }
        public string LastSetPropertyValue { get; private set; }

        public void AddComponent(int instanceId, string componentType)
        {
            AddComponentCallCount++;
            LastAddComponentInstanceId = instanceId;
            LastAddComponentType = componentType;
        }

        public void RemoveComponent(int instanceId, string componentType, int componentIndex)
        {
            RemoveComponentCallCount++;
            LastRemoveComponentInstanceId = instanceId;
            LastRemoveComponentType = componentType;
            LastRemoveComponentIndex = componentIndex;
        }

        public ComponentPropertiesResponse GetProperties(int instanceId, string componentType, int componentIndex)
        {
            GetPropertiesCallCount++;
            LastGetPropertiesInstanceId = instanceId;
            LastGetPropertiesComponentType = componentType;
            LastGetPropertiesComponentIndex = componentIndex;
            return GetPropertiesResult;
        }

        public void SetProperty(int instanceId, string componentType, string propertyPath, string value)
        {
            SetPropertyCallCount++;
            LastSetPropertyInstanceId = instanceId;
            LastSetPropertyComponentType = componentType;
            LastSetPropertyPath = propertyPath;
            LastSetPropertyValue = value;
        }
    }
}
