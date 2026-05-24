using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyScriptableObjectOperations : IScriptableObjectOperations
    {
        public int CreateCallCount { get; private set; }
        public string LastCreateTypeName { get; private set; }
        public string LastCreateAssetPath { get; private set; }
        public CreateScriptableObjectResponse CreateResult { get; set; }

        public int GetPropertiesCallCount { get; private set; }
        public string LastGetPropertiesAssetPath { get; private set; }
        public GetScriptableObjectPropertiesResponse GetPropertiesResult { get; set; }

        public int SetPropertyCallCount { get; private set; }
        public string LastSetPropertyAssetPath { get; private set; }
        public string LastSetPropertyPath { get; private set; }
        public string LastSetPropertyValue { get; private set; }

        public CreateScriptableObjectResponse Create(string typeName, string assetPath)
        {
            CreateCallCount++;
            LastCreateTypeName = typeName;
            LastCreateAssetPath = assetPath;
            return CreateResult;
        }

        public GetScriptableObjectPropertiesResponse GetProperties(string assetPath)
        {
            GetPropertiesCallCount++;
            LastGetPropertiesAssetPath = assetPath;
            return GetPropertiesResult;
        }

        public void SetProperty(string assetPath, string propertyPath, string value)
        {
            SetPropertyCallCount++;
            LastSetPropertyAssetPath = assetPath;
            LastSetPropertyPath = propertyPath;
            LastSetPropertyValue = value;
        }
    }
}
