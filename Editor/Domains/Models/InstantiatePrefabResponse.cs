using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class InstantiatePrefabResponse
    {
        public string name;
        public int instanceId;

        public InstantiatePrefabResponse(string name, int instanceId)
        {
            this.name = name;
            this.instanceId = instanceId;
        }
    }
}
