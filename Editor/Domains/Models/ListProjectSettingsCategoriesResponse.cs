using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class ListProjectSettingsCategoriesResponse
    {
        public List<ProjectSettingsCategoryEntry> categories;

        public ListProjectSettingsCategoriesResponse(List<ProjectSettingsCategoryEntry> categories)
        {
            this.categories = categories;
        }
    }
}
