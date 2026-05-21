using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IProjectSettingsOperations
    {
        ListProjectSettingsCategoriesResponse GetCategories();
        GetProjectSettingsResponse GetSettings(string category);
        void SetSetting(string category, string propertyPath, string value);
    }
}
