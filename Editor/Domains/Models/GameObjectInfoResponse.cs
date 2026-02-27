using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GameObjectInfoResponse
    {
        public string name;
        public int instanceId;
        public bool activeSelf;
        public string tag;
        public int layer;
        public List<ComponentInfoEntry> components;

        public GameObjectInfoResponse(string name, int instanceId, bool activeSelf, string tag, int layer,
            List<ComponentInfoEntry> components)
        {
            this.name = name;
            this.instanceId = instanceId;
            this.activeSelf = activeSelf;
            this.tag = tag;
            this.layer = layer;
            this.components = components;
        }
    }
}
