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
        public string tag;
        public int layer;
        public bool isStatic;
        public int hideFlags;
        public List<string> components;
        public List<GameObjectNode> children;

        public GameObjectNode(string name, int instanceId, bool activeSelf, string tag, int layer, bool isStatic,
            int hideFlags, List<string> components, List<GameObjectNode> children)
        {
            this.name = name;
            this.instanceId = instanceId;
            this.activeSelf = activeSelf;
            this.tag = tag;
            this.layer = layer;
            this.isStatic = isStatic;
            this.hideFlags = hideFlags;
            this.components = components;
            this.children = children;
        }
    }
}
