using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SavePrefabResponse
    {
        public bool success;

        public SavePrefabResponse(bool success)
        {
            this.success = success;
        }
    }
}
