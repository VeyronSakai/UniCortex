using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyProjectSettingsOperations : IProjectSettingsOperations
    {
        public int GetCategoriesCallCount { get; private set; }
        public ListProjectSettingsCategoriesResponse GetCategoriesResult { get; set; }

        public int GetSettingsCallCount { get; private set; }
        public string LastGetSettingsCategory { get; private set; }
        public GetProjectSettingsResponse GetSettingsResult { get; set; }

        public int SetSettingCallCount { get; private set; }
        public string LastSetSettingCategory { get; private set; }
        public string LastSetSettingPropertyPath { get; private set; }
        public string LastSetSettingValue { get; private set; }

        public ListProjectSettingsCategoriesResponse GetCategories()
        {
            GetCategoriesCallCount++;
            return GetCategoriesResult;
        }

        public GetProjectSettingsResponse GetSettings(string category)
        {
            GetSettingsCallCount++;
            LastGetSettingsCategory = category;
            return GetSettingsResult;
        }

        public void SetSetting(string category, string propertyPath, string value)
        {
            SetSettingCallCount++;
            LastSetSettingCategory = category;
            LastSetSettingPropertyPath = propertyPath;
            LastSetSettingValue = value;
        }
    }
}
