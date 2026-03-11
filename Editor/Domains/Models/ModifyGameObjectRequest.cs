using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class ModifyGameObjectRequest
    {
        public int instanceId;
        public string name;
        public bool activeSelf;
        public string tag;
        public int layer;
        public int parentInstanceId;
    }
}
