using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class CreatePrefabResponse
    {
        public bool success;

        // GUID of the saved Prefab asset.
        public string guid;

        public CreatePrefabResponse(bool success, string guid)
        {
            this.success = success;
            this.guid = guid;
        }
    }
}
