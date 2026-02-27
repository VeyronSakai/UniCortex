using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class DeleteGameObjectResponse
    {
        public bool success;

        public DeleteGameObjectResponse(bool success)
        {
            this.success = success;
        }
    }
}
