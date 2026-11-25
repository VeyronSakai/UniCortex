using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class CreatePrefabResponse
    {
        public bool success;

        // GUID of the saved Prefab asset.
        public string guid;

        // Asset path of the Prefab open in Prefab Mode when the source GameObject was connected inside it, so that
        // its contents now hold an instance of the new Prefab and it has to be saved. Empty otherwise.
        public string modifiedPrefabStagePath;

        public CreatePrefabResponse(bool success, string guid, string modifiedPrefabStagePath)
        {
            this.success = success;
            this.guid = guid;
            this.modifiedPrefabStagePath = modifiedPrefabStagePath;
        }
    }
}
