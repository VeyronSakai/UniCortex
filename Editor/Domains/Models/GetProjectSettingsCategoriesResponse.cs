using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetProjectSettingsCategoriesResponse
    {
        public List<ProjectSettingsCategoryEntry> categories;

        public GetProjectSettingsCategoriesResponse(List<ProjectSettingsCategoryEntry> categories)
        {
            this.categories = categories;
        }
    }
}
