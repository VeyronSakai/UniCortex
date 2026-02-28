using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class CreatePrefabRequest
    {
        public int instanceId;
        public string assetPath;
    }
}
