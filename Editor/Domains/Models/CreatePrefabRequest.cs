using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class CreatePrefabRequest
    {
        public int instanceId;
        public string assetPath;

        // Replaces the source GameObject with an instance of the new Prefab, so that it stays linked to it.
        public bool connect;
    }
}
