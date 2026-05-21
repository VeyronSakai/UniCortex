using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SetProjectSettingResponse
    {
        public bool success;

        public SetProjectSettingResponse(bool success)
        {
            this.success = success;
        }
    }
}
