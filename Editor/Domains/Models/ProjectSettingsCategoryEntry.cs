using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class ProjectSettingsCategoryEntry
    {
        public string name;
        public string assetPath;

        public ProjectSettingsCategoryEntry(string name, string assetPath)
        {
            this.name = name;
            this.assetPath = assetPath;
        }
    }
}
