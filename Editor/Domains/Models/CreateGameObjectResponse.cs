using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class CreateGameObjectResponse
    {
        public string name;
        public int instanceId;

        public CreateGameObjectResponse(string name, int instanceId)
        {
            this.name = name;
            this.instanceId = instanceId;
        }
    }
}
