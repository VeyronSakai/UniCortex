using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GameObjectNode
    {
        public string name;
        public int instanceId;
        public bool activeSelf;
        public List<string> components;
        public List<GameObjectNode> children;

        public GameObjectNode(string name, int instanceId, bool activeSelf, List<string> components,
            List<GameObjectNode> children)
        {
            this.name = name;
            this.instanceId = instanceId;
            this.activeSelf = activeSelf;
            this.components = components;
            this.children = children;
        }
    }
}
