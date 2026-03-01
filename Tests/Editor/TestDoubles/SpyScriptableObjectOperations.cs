using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyScriptableObjectOperations : IScriptableObjectOperations
    {
        public int CreateCallCount { get; private set; }
        public string LastCreateType { get; private set; }
        public string LastCreateAssetPath { get; private set; }

        public int GetInfoCallCount { get; private set; }
        public string LastGetInfoAssetPath { get; private set; }
        public ScriptableObjectInfoResponse GetInfoResult { get; set; }

        public int SetPropertyCallCount { get; private set; }
        public string LastSetPropertyAssetPath { get; private set; }
        public string LastSetPropertyPath { get; private set; }
        public string LastSetPropertyValue { get; private set; }

        public void Create(string type, string assetPath)
        {
            CreateCallCount++;
            LastCreateType = type;
            LastCreateAssetPath = assetPath;
        }

        public ScriptableObjectInfoResponse GetInfo(string assetPath)
        {
            GetInfoCallCount++;
            LastGetInfoAssetPath = assetPath;
            return GetInfoResult;
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
