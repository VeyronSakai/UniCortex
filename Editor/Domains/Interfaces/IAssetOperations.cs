using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IAssetOperations
    {
        void Refresh();
        void CreateAsset(string type, string assetPath);
        AssetInfoResponse GetInfo(string assetPath);
        void SetProperty(string assetPath, string propertyPath, string value);
    }
}
