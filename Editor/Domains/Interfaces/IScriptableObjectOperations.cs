using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IScriptableObjectOperations
    {
        CreateScriptableObjectResponse Create(string typeName, string assemblyName, string assetPath);
        GetScriptableObjectPropertiesResponse GetProperties(string assetPath);
        void SetProperty(string assetPath, string propertyPath, string value);
    }
}
