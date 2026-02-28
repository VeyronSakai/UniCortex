using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GameObjectData
    {
        public string name;
        public int instanceId;
        public bool activeSelf;
        public string tag;
        public int layer;
        public bool isStatic;
        public bool isLocked;
        public List<string> components;
        public List<GameObjectData> children;

        public GameObjectData(string name, int instanceId, bool activeSelf, string tag, int layer, bool isStatic,
            bool isLocked, List<string> components, List<GameObjectData> children)
        {
            this.name = name;
            this.instanceId = instanceId;
            this.activeSelf = activeSelf;
            this.tag = tag;
            this.layer = layer;
            this.isStatic = isStatic;
            this.isLocked = isLocked;
            this.components = components;
            this.children = children;
        }
    }
}
