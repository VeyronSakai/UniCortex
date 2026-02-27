using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GameObjectBasicInfo
    {
        public string name;
        public int instanceId;
        public bool activeSelf;

        public GameObjectBasicInfo(string name, int instanceId, bool activeSelf)
        {
            this.name = name;
            this.instanceId = instanceId;
            this.activeSelf = activeSelf;
        }
    }
}
