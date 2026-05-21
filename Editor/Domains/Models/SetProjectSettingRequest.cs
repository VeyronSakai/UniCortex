using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SetProjectSettingRequest
    {
        public string category;
        public string propertyPath;
        public string value;
    }
}
