using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GameObjectNode
    {
        public string name;
        public int instanceId;
        public List<GameObjectNode> children;

        public GameObjectNode(string name, int instanceId, List<GameObjectNode> children)
        {
            this.name = name;
            this.instanceId = instanceId;
            this.children = children;
        }
    }
}
