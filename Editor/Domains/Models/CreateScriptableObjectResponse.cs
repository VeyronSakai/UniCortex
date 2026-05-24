using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class CreateScriptableObjectResponse
    {
        public bool success;
        public int instanceId;

        public CreateScriptableObjectResponse(bool success, int instanceId)
        {
            this.success = success;
            this.instanceId = instanceId;
        }
    }
}
