using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class DuplicateGameObjectResponse
    {
        public string name;
        public int instanceId;

        public DuplicateGameObjectResponse(string name, int instanceId)
        {
            this.name = name;
            this.instanceId = instanceId;
        }
    }
}
