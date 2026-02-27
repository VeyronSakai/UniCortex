using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyAssetOperations : IAssetOperations
    {
        public int RefreshCallCount { get; private set; }

        public int CreateAssetCallCount { get; private set; }
        public string LastCreateType { get; private set; }
        public string LastCreateAssetPath { get; private set; }

        public int GetInfoCallCount { get; private set; }
        public string LastGetInfoAssetPath { get; private set; }
        public AssetInfoResponse GetInfoResult { get; set; }

        public int SetPropertyCallCount { get; private set; }
        public string LastSetPropertyAssetPath { get; private set; }
        public string LastSetPropertyPath { get; private set; }
        public string LastSetPropertyValue { get; private set; }

        public void Refresh()
        {
            RefreshCallCount++;
        }

        public void CreateAsset(string type, string assetPath)
        {
            CreateAssetCallCount++;
            LastCreateType = type;
            LastCreateAssetPath = assetPath;
        }

        public AssetInfoResponse GetInfo(string assetPath)
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
