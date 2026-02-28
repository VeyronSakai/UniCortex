using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class CreatePrefabResponse
    {
        public bool success;

        public CreatePrefabResponse(bool success)
        {
            this.success = success;
        }
    }
}
