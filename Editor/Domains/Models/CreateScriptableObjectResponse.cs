using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class CreateScriptableObjectResponse
    {
        public bool success;

        public CreateScriptableObjectResponse(bool success)
        {
            this.success = success;
        }
    }
}
