using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IScriptableObjectOperations
    {
        void Create(string type, string assetPath);
        ScriptableObjectInfoResponse GetInfo(string assetPath);
        void SetProperty(string assetPath, string propertyPath, string value);
    }
}
