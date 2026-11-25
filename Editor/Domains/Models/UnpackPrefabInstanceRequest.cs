using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class UnpackPrefabInstanceRequest
    {
        // The outermost root of the prefab instance to unpack.
        public int instanceId;

        // A PrefabUnpackModes value. Empty means OutermostRoot.
        public string mode = "";
    }
}
