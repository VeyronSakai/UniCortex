using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class OpenPrefabResponse
    {
        public bool success;

        public OpenPrefabResponse(bool success)
        {
            this.success = success;
        }
    }
}
